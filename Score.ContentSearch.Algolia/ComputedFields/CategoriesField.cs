using System.Collections.Generic;
using System.Linq;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Score.ContentSearch.Algolia.ComputedFields
{
    /// <summary>
    /// Returns Names of all Parent Items inside current Site
    /// </summary>
    public class CategoriesField: IComputedIndexField, ISiteSpecificField
    {
        private ID _homepageId;

        public object ComputeFieldValue(IIndexable indexable)
        {
            var item = (SitecoreIndexableItem)indexable;

            var data = new List<string>();

            var homepage = GetHomepageItem(item);
            if (homepage == null)
                _homepageId = ID.Null;
            else
            {
                _homepageId = homepage.ID;
            }

            AddParent(item.Item.Parent, data);

            if (!data.Any())
                return null;

            return data;
        }

        private Item GetHomepageItem(SitecoreIndexableItem item)
        {
            var site = Factory.GetSite(Site);

            if (site == null)
            {
                CrawlingLog.Log.Error("Cannot load site " + Site);
                return null;
            }

            var homepagePath = site.RootPath + site.StartItem;
            var database = item.Item.Database;

            return database?.GetItem(homepagePath);
        }


        private void AddParent(Item item, List<string> storage)
        {
            if (item == null || 
                item.ID == _homepageId ||
                item.ID == ItemIDs.ContentRoot ||
                item.ID == ItemIDs.RootID)
                return;

            storage.Add(item.Name);

            if (item.Parent == null)
                return;

            AddParent(item.Parent, storage);
        }

        public string FieldName { get; set; }
        public string ReturnType { get; set; }
        public string Site { get; set; }
    }
}
