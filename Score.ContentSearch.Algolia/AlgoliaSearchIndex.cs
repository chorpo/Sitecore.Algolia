using Sitecore.ContentSearch.Maintenance;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaSearchIndex : AlgoliaBaseIndex
    {       
        public AlgoliaSearchIndex(string name, string applicationId, string fullApiKey, string indexName) : 
            base(name, new AlgoliaRepository(new AlgoliaConfig{ApplicationId = applicationId, FullApiKey = fullApiKey, IndexName = indexName}))
        {
           
        }
    }
}
