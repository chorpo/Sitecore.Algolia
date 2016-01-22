using System.Collections.Generic;
using System.Linq;
using Sitecore;
using Sitecore.Caching.Generics;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Data.Items;

namespace Score.ContentSearch.Algolia.ComputedFields
{
    public class ParentsField: IComputedIndexField
    {
        /// <summary>
        /// Returns IDs of all Parent Items
        /// </summary>
        /// <param name="indexable"></param>
        /// <returns></returns>
        public object ComputeFieldValue(IIndexable indexable)
        {
            var item = (SitecoreIndexableItem)indexable;

            var data = new List<string>();

            AddParent(item.Item.Parent, data);

            if (!data.Any())
                return null;

            return data;
        }

        private void AddParent(Item item, List<string> storage)
        {
            if (item == null || 
                item.ID == ItemIDs.ContentRoot ||
                item.ID == ItemIDs.RootID)
                return;

            storage.Add(item.ID.ToString());

            if (item.Parent == null)
                return;

            AddParent(item.Parent, storage);
        }

        public string FieldName { get; set; }
        public string ReturnType { get; set; }
    }
}
