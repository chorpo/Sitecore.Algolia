using Score.ContentSearch.Algolia.Abstract;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaConfig: IAlgoliaConfig
    {
        public string ApplicationId { get; set; }
        public string SearchApiKey { get; set; }
        public string FullApiKey { get; set; }
        public string IndexName { get; set; }
    }
}
