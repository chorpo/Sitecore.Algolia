using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algolia.SitecoreProvider.Abstract
{
    public interface IAlgoliaConfig
    {
        string ApplicationId { get; }
        string SearchApiKey { get; }
        string FullApiKey { get; }
        string IndexName { get; }
    }

}
