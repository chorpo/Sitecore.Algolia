using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Algolia.SitecoreProvider.Abstract;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Abstractions;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.Maintenance.Strategies;
using Sitecore.ContentSearch.Security;

namespace Algolia.SitecoreProvider
{
    public class SearchIndex : ISearchIndex
    {
        private readonly IAlgoliaConfig _config;

        public SearchIndex(IAlgoliaConfig config)
        {
            _config = config;
            this.Crawlers = new List<IProviderCrawler>();
        }

        public void AddCrawler(IProviderCrawler crawler)
        {
            crawler.Initialize(this);
            this.Crawlers.Add(crawler);
        }

        public void AddStrategy(IIndexUpdateStrategy strategy)
        {
            
        }

        public void Rebuild()
        {
            
        }

        public void Rebuild(IndexingOptions indexingOptions)
        {
            
        }

        public Task RebuildAsync(IndexingOptions indexingOptions, CancellationToken cancellationToken)
        {
            return Task.Run(() => Console.WriteLine(""));
        }

        public void Refresh(IIndexable indexableStartingPoint)
        {
           Refresh(indexableStartingPoint, IndexingOptions.Default);
        }

        public void Refresh(IIndexable indexableStartingPoint, IndexingOptions indexingOptions)
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

        public Task RefreshAsync(IIndexable indexableStartingPoint, IndexingOptions indexingOptions,
            CancellationToken cancellationToken)
        {
            return Task.Run(() => Console.WriteLine(""));
        }

        public void Update(IIndexableUniqueId indexableUniqueId)
        {
        
        }

        public void Update(IIndexableUniqueId indexableUniqueId, IndexingOptions indexingOptions)
        {
          
        }

        public void Update(IEnumerable<IIndexableUniqueId> indexableUniqueIds)
        {
        
        }

        public void Update(IEnumerable<IIndexableUniqueId> indexableUniqueIds, IndexingOptions indexingOptions)
        {
         
        }

        public void Update(IEnumerable<IndexableInfo> indexableInfo)
        {
           
        }

        public void Delete(IIndexableId indexableId)
        {
          
        }

        public void Delete(IIndexableId indexableId, IndexingOptions indexingOptions)
        {
         
        }

        public void Delete(IIndexableUniqueId indexableUniqueId)
        {
      
        }

        public void Delete(IIndexableUniqueId indexableUniqueId, IndexingOptions indexingOptions)
        {
           
        }

        public void Reset()
        {
           
        }

        public void Initialize()
        {
          
        }

        public IProviderUpdateContext CreateUpdateContext()
        {
            var repository = new AlgoliaRepository(_config);
            return new ProviderUpdateContext(this, repository);
        }

        public IProviderDeleteContext CreateDeleteContext()
        {
            return null;
        }

        public IProviderSearchContext CreateSearchContext(SearchSecurityOptions options = SearchSecurityOptions.EnableSecurityCheck)
        {
            return null;
        }

        public void StopIndexing()
        {
       
        }

        public void PauseIndexing()
        {
      
        }

        public void ResumeIndexing()
        {
  
        }

        public string Name { get; private set; }
        public ISearchIndexSummary Summary { get; private set; }
        public ISearchIndexSchema Schema { get; private set; }
        public IIndexPropertyStore PropertyStore { get; set; }
        public AbstractFieldNameTranslator FieldNameTranslator { get; set; }
        public ProviderIndexConfiguration Configuration { get; set; }
        public IIndexOperations Operations { get; private set; }
        public IndexingState IndexingState { get; private set; }
        public IList<IProviderCrawler> Crawlers { get; private set; }
        public IObjectLocator Locator { get; private set; }
    }
}
