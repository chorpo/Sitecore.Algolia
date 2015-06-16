using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Score.ContentSearch.Algolia.Abstract;
using Score.ContentSearch.Algolia.Queries;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.Maintenance.Strategies;
using Sitecore.ContentSearch.Security;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaBaseIndex : AbstractSearchIndex
    {
        private readonly IAlgoliaRepository _repository;

        public AlgoliaBaseIndex(string name, IAlgoliaRepository repository)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentOutOfRangeException("name");

            _name = name;

            _repository = repository;
            this.Strategies = new List<IIndexUpdateStrategy>();
        }

        private readonly string _name;
        private ISearchIndexSummary _summary;
        private readonly ISearchIndexSchema schema;
        private bool initialized;

        public List<IIndexUpdateStrategy> Strategies { get; private set; }

        public string Site { get; set; }

        public IAlgoliaRepository Repository
        {
            get { return _repository; }
        }

        #region ISearchIndex

        public override void AddCrawler(IProviderCrawler crawler)
        {
            crawler.Initialize(this);
            this.Crawlers.Add(crawler);
        }

        public override void AddStrategy(IIndexUpdateStrategy strategy)
        {
            strategy.Initialize(this);
            this.Strategies.Add(strategy);
        }

        public override void Rebuild()
        {
            Rebuild(IndexingOptions.Default);
        }

        public override void Rebuild(IndexingOptions indexingOptions)
        {
            this.DoRebuild(indexingOptions, CancellationToken.None);
        }

        protected virtual IProviderUpdateContext CreateFullRebuildContext()
        {
            return this.CreateUpdateContext();
        }

        protected virtual void DoRebuild(IndexingOptions indexingOptions, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
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
            return Task.Run(() => Console.WriteLine(""));
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
        }

        public override void Update(IEnumerable<IndexableInfo> indexableInfo)
        {
            Update(indexableInfo.Select(t => t.IndexableUniqueId));
        }

        public override void Delete(IIndexableId indexableId)
        {
            Delete(indexableId, IndexingOptions.Default);
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

        public override void Delete(IIndexableUniqueId indexableUniqueId)
        {
            Delete(indexableUniqueId, IndexingOptions.Default);
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

        public override void Reset()
        {
            
        }

        public override void Initialize()
        {
            _summary = new AlgoliaSearchIndexSummary(_repository, PropertyStore);

            this.FieldNameTranslator = new AlgoliaFieldNameTranslator();

            var config = this.Configuration as AlgoliaIndexConfiguration;
            if (config == null)
            {
                throw new ConfigurationErrorsException("Index has no configuration.");
            }
            if (config.IndexDocumentPropertyMapper == null)
            {
                throw new ConfigurationErrorsException("AlgoliaDocumentPropertyMapper has not been configured.");
            }
            var mapper = config.IndexDocumentPropertyMapper as ISearchIndexInitializable;
            if (mapper != null)
            {
                mapper.Initialize(this);
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

        public override string Name { get { return _name; }  }

        public override ISearchIndexSummary Summary
        {
            get
            {
                return _summary;
            }
        }

        public override ISearchIndexSchema Schema { get { return schema; } }
        public override IIndexPropertyStore PropertyStore { get; set; }
        public override AbstractFieldNameTranslator FieldNameTranslator { get; set; }
        public override ProviderIndexConfiguration Configuration { get; set; }
        public override IIndexOperations Operations { get { return new AlgoliaIndexOperations(this); } }

        #endregion

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
