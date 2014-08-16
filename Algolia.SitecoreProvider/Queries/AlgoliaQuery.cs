using System;
using System.IO;
using Algolia.Search;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Parsing;
using IndexQuery = Sitecore.ContentSearch.Linq.Parsing.IndexQuery;

namespace Algolia.SitecoreProvider.Queries
{
    public class AlgoliaQuery : IDumpable
    {
        private readonly Query _query;

        public AlgoliaQuery(Query query)
        {
            if (query == null) throw new ArgumentNullException("query");
            _query = query;
        }

        public Query Query
        {
            get { return _query; }
        }

        //public string QueryForLog
        //{
        //    get { return _query.ToString(); }
        //}

        //public override AlgoliaQuery MapQuery(IndexQuery query)
        //{
        //    throw new NotImplementedException();
        //}

        public void WriteTo(TextWriter writer)
        {
            writer.Write(_query.ToString());
        }
    }
}
