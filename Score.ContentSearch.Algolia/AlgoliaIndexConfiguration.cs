using Score.ContentSearch.Algolia.Abstract;
using Sitecore.ContentSearch;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaIndexConfiguration : ProviderIndexConfiguration, IIndexCustomOptions
    {
        public AlgoliaIndexConfiguration()
        {
            MaxFieldLength = 4000;
        }

        public ITagsProcessor TagsProcessor { get; set; }

        public int MaxFieldLength { get; set; }

        public bool IncludeTemplateId { get; set; }
    }
}
