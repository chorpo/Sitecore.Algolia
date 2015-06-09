using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaIndexConfiguration : ProviderIndexConfiguration
    {
        public IIndexDocumentPropertyMapper<JObject> IndexDocumentPropertyMapper { get; set; }
    }
}
