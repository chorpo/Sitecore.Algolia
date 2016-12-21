namespace Score.ContentSearch.Algolia.Abstract
{
    public interface IIndexCustomOptions
    {
        /// <summary>
        /// Field content will be truncated to this length if it passed the limit
        /// </summary>
        int MaxFieldLength { get; set; }

        /// <summary>
        /// Include Template Id into Index Document
        /// </summary>
        bool IncludeTemplateId { get; set; }
    }
}
