using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Links;
using Sitecore.Sites;
using Sitecore.Web;
using Sitecore.Web.UI.WebControls;

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
                if (siteContext == null)
                    throw new Exception($"Site {Site} cannot be reached");

                //we typicaly generate index on CM but site URLs in CM will not work for CD
                //replacing "targetHostName" is not good solution because it breaks PE in CM
                //"cdTargetHostName" site argument should solve that issue. Index uses it instead of targetHostName and PE continue using "targetHostName" 
                var cmTargetHostName = siteContext.Properties["cdTargetHostName"];
                if (!String.IsNullOrWhiteSpace(cmTargetHostName))
                {
                    var props = new Sitecore.Collections.StringDictionary(ToDictionary(siteContext.SiteInfo.Properties));
                    props["targetHostName"] = cmTargetHostName;

                    var siteInfo = new SiteInfo(props);
                    siteContext = new SiteContext(siteInfo);
                }

                UrlOptions.Site = siteContext;
            }
        }

        private static IDictionary<string, string> ToDictionary(NameValueCollection col)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var k in col.AllKeys)
            {
                dict.Add(k, col[k]);
            }
            return dict;
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
