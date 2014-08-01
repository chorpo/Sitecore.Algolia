using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Diagnostics;

namespace Algolia.SitecoreProvider
{
    public class AlgoliaCrawler : AbstractProviderCrawler
    {
        public override void Initialize(ISearchIndex index)
        {
            base.Initialize(index);
            var msg = string.Format("[Index={0}] Initializing AlgoliaCrawler. DB:{1} / Root:{2}", index.Name, base.Database, base.Root);
            CrawlingLog.Log.Info(msg, null);
        }
    }
}
