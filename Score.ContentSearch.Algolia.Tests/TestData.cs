using System;
using Sitecore.Data;

namespace Score.ContentSearch.Algolia.Tests
{
    public class TestData
    {
        public const string TestItemKey = "764A2503-59CA-4825-983C-A19000F44797";
        public static Guid TestItemGuid = Guid.Parse(TestItemKey);
        public static ID TestTemplateId = ID.Parse("{9CAAECFD-3BEB-44B1-9BE5-F7E30811EF2D}");
    }
}
