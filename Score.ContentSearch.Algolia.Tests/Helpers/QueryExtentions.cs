using Algolia.Search;
using NUnit.Framework;

namespace Score.ContentSearch.Algolia.Tests.Helpers
{
    public static class QueryExtentions
    {
        public static void AssertContains(this Query query, string substring)
        {
            Assert.IsTrue(query.GetQueryString().Contains(substring));
        }
    }
}
