using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.ContentSearch.Linq.Methods;

namespace Algolia.SitecoreProvider.Queries
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
