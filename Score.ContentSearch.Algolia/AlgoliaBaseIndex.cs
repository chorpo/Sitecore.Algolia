using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Score.ContentSearch.Algolia.Abstract;
using Score.ContentSearch.Algolia.Factories;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.Events;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.Maintenance.Strategies;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.Sharding;
using Sitecore.Eventing;
using Sitecore.Events;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaBaseIndex : AbstractSearchIndex
    {
        private readonly IAlgoliaRepository _repository;
        protected object IndexUpdateLock = new object();
        private const string LogPreffix = "AlgoliaIndexOperations: ";

        private readonly HashSet<IIndexUpdateStrategy> _strategies = new HashSet<IIndexUpdateStrategy>();

        public AlgoliaBaseIndex(string name, IAlgoliaRepository repository)
            : base(new AlgoliaContextFactory(), null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentOutOfRangeException(nameof(name));

            Name = name;

            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        private ISearchIndexSummary _summary;
        private readonly ISearchIndexSchema schema;

        public override IEnumerable<IIndexUpdateStrategy> UpdateStrategies
        {
            get
            {
                return (IEnumerable<IIndexUpdateStrategy>)this._strategies;
            }
        }

        public string Site { get; set; }

        public IAlgoliaRepository Repository => _repository;

        public override void AddCrawler(IProviderCrawler crawler)
        {
            crawler.Initialize(this);
            this.Crawlers.Add(crawler);
        }

        protected virtual object GetFullRebuildLockObject()
        {
            return this.IndexUpdateLock;
        }

        protected virtual void DoReset(IProviderUpdateContext context)
        {
            var result = _repository.ClearIndexAsync().Result;

            var taskId = result["taskID"].ToString();

            _repository.WaitTaskAsync(taskId).Wait();
        }

        protected override void PerformRebuild(IndexingOptions indexingOptions, CancellationToken cancellationToken)
        {
            CrawlingLog.Log.Debug($"{LogPreffix} {Name} PerformRebuild()");

            if (!base.ShouldStartIndexing(indexingOptions))
            {
                return;
            }

            lock (this.GetFullRebuildLockObject())
            {
                using (IProviderUpdateContext providerUpdateContext = this.CreateFullRebuildContext())
                {
                    CrawlingLog.Log.Warn($"[Index={this.Name}] Reset Started", null);
                    this.DoReset(providerUpdateContext);
                    CrawlingLog.Log.Warn($"[Index={this.Name}] Reset Ended", null);
                    CrawlingLog.Log.Warn($"[Index={this.Name}] Full Rebuild Started", null);
                    this.DoRebuild(providerUpdateContext, indexingOptions, cancellationToken);
                    CrawlingLog.Log.Warn($"[Index={this.Name}] Full Rebuild Ended", null);
                }
            }
        }

        protected override void PerformRefresh(IIndexable indexableStartingPoint, IndexingOptions indexingOptions,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override void AddStrategy(IIndexUpdateStrategy strategy)
        {
            Sitecore.Diagnostics.Assert.IsNotNull((object)strategy, "The strategy cannot be null");
            strategy.Initialize(this);
            this._strategies.Add(strategy);
        }

        public override void Rebuild()
        {
            Rebuild(IndexingOptions.Default);
        }

        public override void Rebuild(IndexingOptions indexingOptions)
        {
            this.PerformRebuild(indexingOptions, CancellationToken.None);
        }

        protected virtual IProviderUpdateContext CreateFullRebuildContext()
        {
            return this.CreateUpdateContext();
        }

        protected virtual void DoRebuild(IProviderUpdateContext context, IndexingOptions indexingOptions, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using (IProviderUpdateContext providerUpdateContext = this.CreateFullRebuildContext())
            {
                foreach (IProviderCrawler current in base.Crawlers)
                {
                    current.RebuildFromRoot(providerUpdateContext, indexingOptions, cancellationToken);
                }
                if ((base.IndexingState & IndexingState.Stopped) != IndexingState.Stopped)
                {
                    providerUpdateContext.Optimize();
                }
                providerUpdateContext.Commit();
            }
            stopwatch.Stop();
            if ((base.IndexingState & IndexingState.Stopped) != IndexingState.Stopped)
            {
                this.PropertyStore.Set(IndexProperties.RebuildTime, stopwatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }

        public override Task RebuildAsync(IndexingOptions indexingOptions, CancellationToken cancellationToken)
        {
            return Task.Run(() => Rebuild(indexingOptions), cancellationToken);
        }

        public override void Refresh(IIndexable indexableStartingPoint)
        {
            Refresh(indexableStartingPoint, IndexingOptions.Default);
        }

        public override void Refresh(IIndexable indexableStartingPoint, IndexingOptions indexingOptions)
        {
            using (var context = this.CreateUpdateContext())
            {
                foreach (var crawler in this.Crawlers)
                {
                    crawler.RefreshFromRoot(context, indexableStartingPoint, indexingOptions);
                }
                context.Optimize();
                context.Commit();
            }
        }

        public override Task RefreshAsync(IIndexable indexableStartingPoint, IndexingOptions indexingOptions,
            CancellationToken cancellationToken)
        {
            return Task.Run(() => Console.WriteLine(""), cancellationToken);
        }

        public override void Update(IIndexableUniqueId indexableUniqueId)
        {
            Update(indexableUniqueId, IndexingOptions.Default);
        }

        public override void Update(IIndexableUniqueId indexableUniqueId, IndexingOptions indexingOptions)
        {
            Update(new List<IIndexableUniqueId> { indexableUniqueId }, IndexingOptions.Default);
        }

        public override void Update(IEnumerable<IIndexableUniqueId> indexableUniqueIds)
        {
            Update(indexableUniqueIds, IndexingOptions.Default);
        }

        public override void Update(IEnumerable<IIndexableUniqueId> indexableUniqueIds, IndexingOptions indexingOptions)
        {
            if (!ShouldStartIndexing(indexingOptions))
                return;

            CrawlingLog.Log.Debug($"Algolia: Updating {indexableUniqueIds.Count()} with indexing options {indexingOptions}");
            Event.RaiseEvent("indexing:start", (object)this.Name, (object)false);
            IndexingStartedEvent indexingStartedEvent = new IndexingStartedEvent();
            indexingStartedEvent.IndexName = this.Name;
            indexingStartedEvent.FullRebuild = false;
            EventManager.QueueEvent<IndexingStartedEvent>(indexingStartedEvent);


            using (var context = this.CreateUpdateContext())
            {
                foreach (var crawler in this.Crawlers)
                {
                    foreach (var indexableUniqueId in indexableUniqueIds)
                    {
                        crawler.Update(context, indexableUniqueId, indexingOptions);
                    }
                }
                context.Commit();
            }

            Event.RaiseEvent("indexing:end", (object)this.Name, (object)false);
            IndexingFinishedEvent indexingFinishedEvent = new IndexingFinishedEvent();
            indexingFinishedEvent.IndexName = this.Name;
            indexingFinishedEvent.FullRebuild = false;
            EventManager.QueueEvent<IndexingFinishedEvent>(indexingFinishedEvent);
            CrawlingLog.Log.Debug($"Algolia: End Updating {indexableUniqueIds.Count()} with indexing options {indexingOptions}");
        }

        public override void Update(IEnumerable<IndexableInfo> indexableInfo)
        {
            Update(indexableInfo.Select(t => t.IndexableUniqueId));
        }

        public override void Delete(IIndexableUniqueId indexableUniqueId)
        {
            Delete(indexableUniqueId, IndexingOptions.Default);
        }

        public override void Delete(IIndexableId indexableId)
        {
            Delete(indexableId, IndexingOptions.Default);
        }

        public override void Delete(IIndexableUniqueId indexableUniqueId, IndexingOptions indexingOptions)
        {
            using (var context = this.CreateUpdateContext())
            {
                foreach (var crawler in this.Crawlers)
                {
                    crawler.Delete(context, indexableUniqueId, indexingOptions);
                }
                context.Commit();
            }
        }

        public override void Delete(IIndexableId indexableId, IndexingOptions indexingOptions)
        {
            using (var context = this.CreateUpdateContext())
            {
                foreach (var crawler in this.Crawlers)
                {
                    crawler.Delete(context, indexableId, indexingOptions);
                }
                context.Commit();
            }
        }

        public override void Reset()
        {

        }

        public override void Initialize()
        {
            try
            {
                _summary = new AlgoliaSearchIndexSummary(_repository, PropertyStore);

                var config = this.Configuration as AlgoliaIndexConfiguration;
                if (config == null)
                {
                    throw new ConfigurationErrorsException("Index has no configuration.");
                }

                initialized = true;
                Sitecore.Diagnostics.Log.Info(string.Format("Algolia Provider is initialized for index {0}", Name), this);
            }
            catch (Exception ex)
            {
                initialized = false;
                Sitecore.Diagnostics.Log.Info(string.Format("Initialization failed for index {0}", Name), this);
            }

        }

        public override IProviderUpdateContext CreateUpdateContext()
        {
            return new AlgoliaUpdateContext(this, _repository);
        }

        public override IProviderDeleteContext CreateDeleteContext()
        {
            return null;
        }

        public override IProviderSearchContext CreateSearchContext(
            SearchSecurityOptions options = SearchSecurityOptions.EnableSecurityCheck)
        {
            return new AlgoliaSearchContext(this, options);
        }

        public override string Name { get; }

        public override ISearchIndexSummary Summary => _summary;

        public override ISearchIndexSchema Schema { get { return schema; } }
        public override IIndexPropertyStore PropertyStore { get; set; }
        public override AbstractFieldNameTranslator FieldNameTranslator { get; set; }
        public override ProviderIndexConfiguration Configuration { get; set; }
        public override IIndexOperations Operations => new AlgoliaIndexOperations(this);

        public override bool EnableItemLanguageFallback {
            get { return false; }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool EnableFieldLanguageFallback {
            get { return false; }

            set
            {
                throw new NotImplementedException();
            }
        }
        public override IShardingStrategy ShardingStrategy { get; set; }

        public override IShardFactory ShardFactory
        {
            get
            {
                return (IShardFactory)null;
            }
        }

        public override IEnumerable<Shard> Shards
        {
            get
            {
                yield break;
            }
        }

        public override bool IsSharded
        {
            get
            {
                return false;
            }
        }

        protected virtual void DoRebuild(IndexingOptions indexingOptions)
        {
            var timer = new Stopwatch();
            timer.Start();
            using (var context = this.CreateUpdateContext())
            {
                foreach (var crawler in this.Crawlers)
                {
                    crawler.RebuildFromRoot(context, indexingOptions);
                }
                context.Optimize();
                context.Commit();
            }
            timer.Stop();
            this.PropertyStore.Set(IndexProperties.RebuildTime,
                timer.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
        }

    }
}