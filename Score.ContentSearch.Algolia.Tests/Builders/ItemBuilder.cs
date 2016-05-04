using System;
using System.Globalization;
using Sitecore;
using Sitecore.Data;
using Sitecore.FakeDb;

namespace Score.ContentSearch.Algolia.Tests.Builders
{
    internal class ItemBuilder
    {
        private readonly DbItem _item;
        private DbItem _subitem;

        public ItemBuilder()
        {
            _item = new DbItem("source", new ID(TestData.TestItemGuid));
        }

        public const string DispalyNameFieldName = "Display Name";
        public const string CountFieldName = "Count";
        public const string LocationFieldName = "Location";
        public const string DateFieldName = "Date";
        public const string PriceFieldName = "Price";
        public const string ReferenceFieldName = "Reference";
        public static ID SubitemId = ID.NewID;


        public ItemBuilder AddSubItem()
        {
            _subitem = new DbItem("subitem", SubitemId);
            _item.Children.Add(_subitem);
            _subitem.ParentID = _item.ID;
            return this;
        }

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

        public ItemBuilder WithReference(string fieldType)
        {
            var field = new DbField(ID.NewID)
            {
                Value = SubitemId.ToString(),
                Type = fieldType,
                Name = ReferenceFieldName
            };
            _item.Fields.Add(field);
            return this;
        }

        public ItemBuilder WithSubitemDisplayName(string value)
        {
            var field = new DbField(FieldIDs.DisplayName)
            {
                Value = value,
                Type = "single-line text",
                Name = "Display Name"
            };
            _subitem.Fields.Add(field);
            return this;
        }

        public DbItem Build()
        {
            return _item;
        }

    }
}
