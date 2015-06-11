using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;

namespace Score.ContentSearch.Algolia.ComputedFields
{
    public class ParentsField: IComputedIndexField
    {
        public object ComputeFieldValue(IIndexable indexable)
        {
            var item = (SitecoreIndexableItem)indexable;

            if (item == null)
                return string.Empty;

            if (item.Item.Parent != null)
                return item.Item.Parent.ID.ToString();

            return "no parent";
        }

        public string FieldName { get; set; }
        public string ReturnType { get; set; }
    }
}
