using Sitecore.ContentSearch.Maintenance;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaSearchIndex : AlgoliaBaseIndex
    {       
        public AlgoliaSearchIndex(string name, string applicationId, string fullApiKey, IIndexPropertyStore propertyStore) : 
            base(name, new AlgoliaRepository(new AlgoliaConfig{ApplicationId = applicationId, FullApiKey = fullApiKey, IndexName = name}), propertyStore)
        {
           
        }
    }
}
