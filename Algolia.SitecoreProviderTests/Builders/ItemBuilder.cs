using System.Globalization;
using Sitecore.Data;
using Sitecore.FakeDb;

namespace Algolia.SitecoreProviderTests.Builders
{
    internal class ItemBuilder
    {
         private readonly DbItem _item;

        public ItemBuilder()
        {
            _item = new DbItem("source", new ID(TestData.TestItemId));
        }

        public ItemBuilder WithDisplayName(string value)
        {
            var field = new DbField(ID.NewID)
            {
                Value = value,
                Type = "text",
                Name = "DisplayName"
            };
            _item.Fields.Add(field);
            return this;
        }

        public ItemBuilder WithCount(int value)
        {
            var field = new DbField(ID.NewID)
            {
                Value = value.ToString(CultureInfo.InvariantCulture),
                Type = "number",
                Name = "Count"
            };
            _item.Fields.Add(field);
            return this;
        }

        public ItemBuilder WithPrice(double value)
        {
            var field = new DbField(ID.NewID)
            {
                Value = value.ToString(CultureInfo.InvariantCulture),
                Type = "number",
                Name = "Price"
            };
            _item.Fields.Add(field);
            return this;
        }

        public DbItem Build()
        {
            return _item;
        }

    }
}
