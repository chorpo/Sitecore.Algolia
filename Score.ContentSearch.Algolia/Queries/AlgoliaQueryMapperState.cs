using System.Collections.Generic;
using Sitecore.ContentSearch.Linq.Methods;

namespace Score.ContentSearch.Algolia.Queries
{
    public class AlgoliaQueryMapperState
    {
        public AlgoliaQueryMapperState()
        {
            AdditionalQueryMethods = new List<QueryMethod>();
        }
        
        public List<QueryMethod> AdditionalQueryMethods
        {
            get;
            set;
        }
    }
}
