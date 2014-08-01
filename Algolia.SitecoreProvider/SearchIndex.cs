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
            throw new NotImplementedException();
        }

        public void Rebuild()
        {
            throw new NotImplementedException();
        }

        public void Rebuild(IndexingOptions indexingOptions)
        {
            throw new NotImplementedException();
        }

        public Task RebuildAsync(IndexingOptions indexingOptions, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Refresh(IIndexable indexableStartingPoint)
        {
            throw new NotImplementedException();
        }

        public void Refresh(IIndexable indexableStartingPoint, IndexingOptions indexingOptions)
        {
            throw new NotImplementedException();
        }

        public Task RefreshAsync(IIndexable indexableStartingPoint, IndexingOptions indexingOptions,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Update(IIndexableUniqueId indexableUniqueId)
        {
            throw new NotImplementedException();
        }

        public void Update(IIndexableUniqueId indexableUniqueId, IndexingOptions indexingOptions)
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<IIndexableUniqueId> indexableUniqueIds)
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<IIndexableUniqueId> indexableUniqueIds, IndexingOptions indexingOptions)
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<IndexableInfo> indexableInfo)
        {
            throw new NotImplementedException();
        }

        public void Delete(IIndexableId indexableId)
        {
            throw new NotImplementedException();
        }

        public void Delete(IIndexableId indexableId, IndexingOptions indexingOptions)
        {
            throw new NotImplementedException();
        }

        public void Delete(IIndexableUniqueId indexableUniqueId)
        {
            throw new NotImplementedException();
        }

        public void Delete(IIndexableUniqueId indexableUniqueId, IndexingOptions indexingOptions)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public IProviderUpdateContext CreateUpdateContext()
        {
            throw new NotImplementedException();
        }

        public IProviderDeleteContext CreateDeleteContext()
        {
            throw new NotImplementedException();
        }

        public IProviderSearchContext CreateSearchContext(SearchSecurityOptions options = SearchSecurityOptions.EnableSecurityCheck)
        {
            throw new NotImplementedException();
        }

        public void StopIndexing()
        {
            throw new NotImplementedException();
        }

        public void PauseIndexing()
        {
            throw new NotImplementedException();
        }

        public void ResumeIndexing()
        {
            throw new NotImplementedException();
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
