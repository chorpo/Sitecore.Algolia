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
    public class AlgoliaSearchContext: IProviderSearchContext
    {
        public AlgoliaSearchContext(AlgoliaBaseIndex index,
            SearchSecurityOptions securityOptions = SearchSecurityOptions.EnableSecurityCheck)
        {
            Assert.ArgumentNotNull(index, "index");
            this.Index = index;
            this.SecurityOptions = securityOptions;
        }


        public void Dispose()
        {
            
        }

        public IQueryable<TItem> GetQueryable<TItem>()
        {
            return GetQueryable<TItem>((IExecutionContext)null);
        }

        public IQueryable<TItem> GetQueryable<TItem>(IExecutionContext executionContext)
        {
            var index = new LinqToAlgoliaIndex<TItem>(this, executionContext);
            return index.GetQueryable();
        }

        //public IQueryable<TItem> GetQueryable<TItem>(IExecutionContext executionContext) where TItem : new()
        //{
        //    var index = new LinqToXmlIndex<TItem>(this, executionContext);
        //    //START: logging
        //    if (ContentSearchConfigurationSettings.EnableSearchDebug)
        //    {
        //        var writeable = (IHasTraceWriter)index;
        //        writeable.TraceWriter = new LoggingTraceWriter(SearchLog.Log);
        //    }
        //    //END: logging
        //    return index.GetQueryable();
        //}

        public IQueryable<TItem> GetQueryable<TItem>(params IExecutionContext[] executionContexts)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SearchIndexTerm> GetTermsByFieldName(string fieldName, string prefix)
        {
            throw new NotImplementedException();
        }

        public ISearchIndex Index { get; private set; }
        public SearchSecurityOptions SecurityOptions { get; private set; }
    }
}
