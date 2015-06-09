using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algolia.SitecoreProvider.Abstract;

namespace Algolia.SitecoreProvider
{
    public class AlgoliaConfig: IAlgoliaConfig
    {
        public string ApplicationId { get; set; }
        public string SearchApiKey { get; set; }
        public string FullApiKey { get; set; }
        public string IndexName { get; set; }
    }
}
