using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore.ContentSearch;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaIndexConfiguration : ProviderIndexConfiguration
    {
        public ITagsProcessor TagsProcessor { get; set; }
    }
}
