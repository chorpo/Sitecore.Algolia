using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algolia.Search;
using NUnit.Framework;

namespace Algolia.SitecoreProviderTests.Helpers
{
    public static class QueryExtentions
    {
        public static void AssertContains(this Query query, string substring)
        {
            Assert.IsTrue(query.GetQueryString().Contains(substring));
        }
    }
}
