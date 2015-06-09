using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;

namespace Algolia.SitecoreProvider
{
    public class AlgoliaIndexConfiguration : ProviderIndexConfiguration
    {
        public IIndexDocumentPropertyMapper<JObject> IndexDocumentPropertyMapper { get; set; }
    }
}
