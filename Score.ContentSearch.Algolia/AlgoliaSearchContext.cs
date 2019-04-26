using System;
using System.Collections.Generic;
using System.Linq;
using Score.ContentSearch.Algolia.Queries;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Security;
using Sitecore.Diagnostics;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaSearchContext : IProviderSearchContext
    {
        public ISearchIndex Index { get; private set; }
        public bool ConvertQueryDatesToUtc { get; set; }
        public SearchSecurityOptions SecurityOptions { get; private set; }

        public AlgoliaSearchContext(AlgoliaBaseIndex index,
            SearchSecurityOptions securityOptions = SearchSecurityOptions.EnableSecurityCheck)
        {
            Assert.ArgumentNotNull(index, "index");
            this.Index = index;
            this.SecurityOptions = securityOptions;
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public IQueryable<TItem> GetQueryable<TItem>()
        {
            return this.GetQueryable<TItem>(Array.Empty<IExecutionContext>());
        }

        public IQueryable<TItem> GetQueryable<TItem>(IExecutionContext executionContext)
        {
            return this.GetQueryable<TItem>(new IExecutionContext[1]
                  {
                    executionContext
                  });
        }

        public IQueryable<TItem> GetQueryable<TItem>(params IExecutionContext[] executionContexts)
        {
            var instance = new LinqToAlgoliaIndex<TItem>(this, executionContexts);
            return instance.GetQueryable();
        }

        public IEnumerable<SearchIndexTerm> GetTermsByFieldName(string fieldName, string prefix)
        {
            yield break;
        }        
    }
}
