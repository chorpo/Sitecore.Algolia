using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Score.ContentSearch.Algolia.Tests.Builders;
using Sitecore.ContentSearch;
using Sitecore.FakeDb;

namespace Score.ContentSearch.Algolia.Tests.AlgoliaDocumentBuilderTests
{
    [TestFixture]
    public class AddFieldTests
    {
        [Test]
        public void AddTextFieldTest()
        {
            // arrange
            using (var db = new Db { new ItemBuilder().WithDisplayName("test").Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);
                
                var context = new Mock<IProviderUpdateContext>();
                var index = new IndexBuilder()
                    .WithSimpleFieldTypeMap("text")
                    //.WithDefaultFieldReader("single-line text|multi-line text|text|memo")
                    .Build();
                context.Setup(t => t.Index).Returns(index);
                var sut = new AlgoliaDocumentBuilder(indexable, context.Object);

                var field = new SitecoreItemDataField(item.Fields[ItemBuilder.DispalyNameFieldName]);

                //Act
                sut.AddField(field);
                
                //Assert
                JObject doc = sut.Document;
                Assert.AreEqual("test", (string)doc["displayname"]);
            }
        }

        [Test]
        public void StringValueShouldBeTrimmed()
        {
            // arrange
            using (var db = new Db { new ItemBuilder().WithDisplayName("  test  ").Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);


                var context = new Mock<IProviderUpdateContext>();
                var index = new IndexBuilder()
                    .WithSimpleFieldTypeMap("text")
                    .Build();
                context.Setup(t => t.Index).Returns(index);
                var sut = new AlgoliaDocumentBuilder(indexable, context.Object);

                var field = new SitecoreItemDataField(item.Fields[ItemBuilder.DispalyNameFieldName]);

                //Act
                sut.AddField(field);

                //Assert
                JObject doc = sut.Document;
                Assert.AreEqual("test", (string)doc["displayname"]);
            }
        }

        [Test]
        public void AddIntFieldTest()
        {
            // arrange
            using (var db = new Db { new ItemBuilder().WithCount(10).Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);


                var context = new Mock<IProviderUpdateContext>();
                var index = new IndexBuilder()
                    .WithSimpleFieldTypeMap("number")
                    .WithNumericFieldReader("number")
                    .Build();
                context.Setup(t => t.Index).Returns(index);
                var sut = new AlgoliaDocumentBuilder(indexable, context.Object);

                var field = new SitecoreItemDataField(item.Fields[ItemBuilder.CountFieldName]);

                //Act
                sut.AddField(field);

                //Assert
                JObject doc = sut.Document;
                Assert.AreEqual(10, (int)doc["count"]);
                Assert.AreEqual(JTokenType.Integer, doc["count"].Type);
            }
        }

        [Test]
        public void AddDateFieldTest()
        {
            // arrange
            using (var db = new Db { new ItemBuilder().WithHardcodedDate().Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);


                var context = new Mock<IProviderUpdateContext>();
                var index = new IndexBuilder()
                    .WithSimpleFieldTypeMap("datetime")
                    .WithDateFieldReader("datetime")
                    .Build();
                context.Setup(t => t.Index).Returns(index);
                var sut = new AlgoliaDocumentBuilder(indexable, context.Object);
                var field = new SitecoreItemDataField(item.Fields[ItemBuilder.DateFieldName]);

                //Act
                sut.AddField(field);

                //Assert
                JObject doc = sut.Document;
                Assert.AreEqual(1418787000, (int)doc["date"]);
                Assert.AreEqual(JTokenType.Integer, doc["date"].Type);
            }
        }


        [Test]
        public void AddJobjectFieldTest()
        {
            // arrange
            using (var db = new Db {new ItemBuilder().WithHardcodedDate().Build()})
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);


                var context = new Mock<IProviderUpdateContext>();
                var index = new IndexBuilder()
                    .Build();
                context.Setup(t => t.Index).Returns(index);
                var sut = new AlgoliaDocumentBuilder(indexable, context.Object);

                var value = JObject.Parse(@"{'_geoloc': {
        'lat': 33.7489954,
        'lng': -84.3879824
      }}");
                //Act
                sut.AddField(ItemBuilder.LocationFieldName, value);

                //Assert
                JObject doc = sut.Document;
                Assert.AreEqual(33.7489954, (double) doc["_geoloc"]["lat"]);
                Assert.AreEqual(-84.3879824, (double) doc["_geoloc"]["lng"]);
            }
        }

        [Test]
        public void AddSimpleDoubleFieldTest()
        {
            // arrange
            using (var db = new Db { new ItemBuilder().WithPrice(123.456).Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);
                
                var context = new Mock<IProviderUpdateContext>();
                var index = new IndexBuilder()
                    .WithSimpleFieldTypeMap("number")
                    .WithNumericFieldReader("number")
                    .Build();
                context.Setup(t => t.Index).Returns(index);
                var sut = new AlgoliaDocumentBuilder(indexable, context.Object);

                var field = new SitecoreItemDataField(item.Fields[ItemBuilder.PriceFieldName]);

                //Act
                sut.AddField(field);

                //Assert
                JObject doc = sut.Document;
                Assert.AreEqual(123.456, (double)doc["price"]);
                Assert.AreEqual(JTokenType.Float, doc["price"].Type);
            }
        }


        [Test]
        public void CoreItemFieldsShouldBeLoaded()
        {
            // arrange
            using (var db = new Db { new ItemBuilder().Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);

                var context = new Mock<IProviderUpdateContext>();
                var index = new IndexBuilder()
                    .Build();
                context.Setup(t => t.Index).Returns(index);
                var sut = new AlgoliaDocumentBuilder(indexable, context.Object);

                //Act
                var actual = sut.Document;

                //Assert
                Assert.AreEqual(TestData.TestItemKey.ToLower(), (string)actual["objectID"]);
                Assert.AreEqual("/sitecore/content/source", (string)actual["path"]);
                Assert.AreEqual("source", (string)actual["name"]);
            }
        }

    }
}
