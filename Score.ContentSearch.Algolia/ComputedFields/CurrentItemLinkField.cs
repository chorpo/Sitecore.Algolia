using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Links;
using Sitecore.Sites;

namespace Score.ContentSearch.Algolia.ComputedFields
{
    public class CurrentItemLinkField : IComputedIndexField, ISiteSpecificField
    {
        protected UrlOptions UrlOptions;

        public object ComputeFieldValue(IIndexable indexable)
        {
            var item = (SitecoreIndexableItem)indexable;

            if (UrlOptions == null)
                UrlOptions = UrlOptions.DefaultOptions;

            AdjustOptions(item);

            var url = LinkManager.GetItemUrl(item, UrlOptions);

            url = PostProcessUrl(url);

            return url;
        }

        protected virtual void AdjustOptions(SitecoreIndexableItem item) 
        {
            if (!string.IsNullOrEmpty(Site))
            {
                var siteContext = SiteContext.GetSite(Site);
                UrlOptions.Site = siteContext;
            }
        }

        protected virtual string PostProcessUrl(string url)
        {
            if (url.StartsWith(":"))
                return url.Remove(0, 1);

            return url;
        }

        public string FieldName { get; set; }
        public string ReturnType { get; set; }
        public string Site { get; set; }
    }
}
