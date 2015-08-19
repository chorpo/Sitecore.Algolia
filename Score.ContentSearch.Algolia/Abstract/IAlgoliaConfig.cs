namespace Score.ContentSearch.Algolia.Abstract
{
    public interface IAlgoliaConfig
    {
        string ApplicationId { get; }
        string SearchApiKey { get; }
        string FullApiKey { get; }
        string IndexName { get; }
    }

}
