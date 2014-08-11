using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Algolia.SitecoreProvider.Abstract;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Abstractions;
using Sitecore.ContentSearch.Events;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.Maintenance.Strategies;
using Sitecore.ContentSearch.Security;
using Sitecore.Eventing;
using Sitecore.Events;

namespace Algolia.SitecoreProvider
{
    public class AlgoliaSearchIndex : AlgoliaBaseIndex
    {       
        public AlgoliaSearchIndex(string name, string applicationId, string fullApiKey, IIndexPropertyStore propertyStore) : 
            base(name, new AlgoliaRepository(new AlgoliaConfig{ApplicationId = applicationId, FullApiKey = fullApiKey, IndexName = name}), propertyStore)
        {
           
        }
    }
}
