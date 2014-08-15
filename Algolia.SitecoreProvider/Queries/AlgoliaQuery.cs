using System;
using Algolia.Search;
using Sitecore.ContentSearch.Linq.Parsing;
using IndexQuery = Sitecore.ContentSearch.Linq.Parsing.IndexQuery;

namespace Algolia.SitecoreProvider.Queries
{
    public class AlgoliaQuery : QueryMapper<AlgoliaQuery>
    {
        private readonly Query _query;

        public AlgoliaQuery(Query query)
        {
            if (query == null) throw new ArgumentNullException("query");
            _query = query;
        }


        public override AlgoliaQuery MapQuery(IndexQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
