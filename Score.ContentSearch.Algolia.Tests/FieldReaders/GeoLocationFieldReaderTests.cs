using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Score.ContentSearch.Algolia.FieldReaders;
using Score.ContentSearch.Algolia.Tests.Builders;
using Sitecore.ContentSearch;
using Sitecore.FakeDb;

namespace Score.ContentSearch.Algolia.Tests.FieldReaders
{
    [TestFixture]
    public class GeoLocationFieldReaderTests
    {
        [Test]
        public void GetFieldValueTests()
        {
            //Arrange
            using (var db = new Db {new ItemBuilder().WithGeoLocation("34.0385737,-84.56821339999999,").Build()})
            {
                var item = db.GetItem("/sitecore/content/source");
                var field = item.Fields[ItemBuilder.LocationFieldName];
                var args = new SitecoreItemDataField(field);
                var sut = new GeoLocationFieldReader();

                //Act
                var actual = sut.GetFieldValue(args);

                //Assert
                var latLong = actual as JObject;
                Assert.AreEqual(34.0385737, (double)latLong["_geoloc"]["lat"]);
                Assert.AreEqual(-84.56821339999999, (double)latLong["_geoloc"]["lng"]);
            }
        }
    }
}
