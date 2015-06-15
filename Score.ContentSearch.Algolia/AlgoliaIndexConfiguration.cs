using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaIndexConfiguration : ProviderIndexConfiguration
    {
        //todo: Delete
        public IIndexDocumentPropertyMapper<JObject> IndexDocumentPropertyMapper { get; set; }


    }
}
