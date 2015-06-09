using System;
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

        public const string DispalyNameFieldName = "DisplayName";
        public const string CountFieldName = "Count";
        public const string LocationFieldName = "Location";
        public const string DateFieldName = "Date";
        public const string PriceFieldName = "Price";

        public ItemBuilder WithDisplayName(string value)
        {
            var field = new DbField(ID.NewID)
            {
                Value = value,
                Type = "text",
                Name = DispalyNameFieldName
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
                Name = CountFieldName
            };
            _item.Fields.Add(field);
            return this;
        }

        public ItemBuilder WithGeoLocation(string value)
        {
            var field = new DbField(ID.NewID)
            {
                Value = value,
                Type = "geolocation",
                Name = LocationFieldName
            };
            _item.Fields.Add(field);
            return this;
        }

        public ItemBuilder WithHardcodedDate()
        {
            var field = new DbField(ID.NewID)
            {
                Value = "20141217T033000",
                Type = "datetime",
                Name = DateFieldName
            };
            _item.Fields.Add(field);
            return this;
        }

        public ItemBuilder WithDate(DateTime value)
        {
            var field = new DbField(ID.NewID)
            {
                //"20141217T033000"
                Value = Sitecore.DateUtil.ToIsoDate(value),
                Type = "datetime",
                Name = DateFieldName
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
                Name = PriceFieldName
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
