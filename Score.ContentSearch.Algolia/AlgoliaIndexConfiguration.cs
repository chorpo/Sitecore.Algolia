using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore.ContentSearch;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaIndexConfiguration : ProviderIndexConfiguration, ILenghtConstraint
    {
        public AlgoliaIndexConfiguration()
        {
            MaxFieldLength = 4000;
        }

        public ITagsProcessor TagsProcessor { get; set; }

        public int MaxFieldLength { get; set; }
    }
}
