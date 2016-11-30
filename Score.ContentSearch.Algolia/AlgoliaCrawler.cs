using Sitecore.ContentSearch;
using Sitecore.Data.Items;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaCrawler: SitecoreItemCrawler
    {
        public string ShowInSearchResultsFieldName { get; set; }

        protected override bool IsExcludedFromIndex(SitecoreIndexableItem indexable, bool checkLocation = false)
        {
            var result = base.IsExcludedFromIndex(indexable, checkLocation);

            if (result)
                return true;

            var obj = (Item)indexable;

            if (!string.IsNullOrWhiteSpace(ShowInSearchResultsFieldName))
            {
                var showInSearchResultsField = obj.Fields[ShowInSearchResultsFieldName];
                if (showInSearchResultsField != null)
                {
                    result = string.IsNullOrWhiteSpace(showInSearchResultsField.Value);
                }
            }

            return result;
        }
    }
}
