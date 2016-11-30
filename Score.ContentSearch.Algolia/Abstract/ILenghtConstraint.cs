namespace Score.ContentSearch.Algolia.Abstract
{
    public interface ILenghtConstraint
    {
        /// <summary>
        /// Field content will be truncated to this length if it passed the limit
        /// </summary>
        int MaxFieldLength { get; set; }
    }
}
