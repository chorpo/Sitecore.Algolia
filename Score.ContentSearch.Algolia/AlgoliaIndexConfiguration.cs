using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore.ContentSearch;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaIndexConfiguration : ProviderIndexConfiguration
    {
        //todo: Delete
        public IIndexDocumentPropertyMapper<JObject> IndexDocumentPropertyMapper { get; set; }

        public ITagsProcessor TagsProcessor { get; set; }
    }
}
