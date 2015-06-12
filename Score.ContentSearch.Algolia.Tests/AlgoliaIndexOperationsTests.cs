using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Score.ContentSearch.Algolia.Tests.Builders;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.Data;
using Sitecore.FakeDb;

namespace Score.ContentSearch.Algolia.Tests
{
    [TestFixture]
    public class AlgoliaIndexOperationsTests
    {
        private DbItem _source;

        [SetUp]
        public void SetUp()
        {
            _source = new DbItem("source", new ID(TestData.TestItemGuid));
        }

        [Test]
        public void UpdateTest()
        {
            // arrange
            using (var db = new Db {_source})
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);
                JObject doc = null;

                var context = new Mock<IProviderUpdateContext>();
                context.Setup(
                    t => t.UpdateDocument(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<IExecutionContext>()))
                    .Callback(
                        (object itemToUpdate, object criteriaForUpdate, IExecutionContext executionContext) =>
                            doc = itemToUpdate as JObject);

                var index = new IndexBuilder().Build();
                context.Setup(t => t.Index).Returns(index);

                var operations = new AlgoliaIndexOperations(index);

                //Act
                operations.Update(indexable, context.Object, new ProviderIndexConfiguration());

                //Assert
                context.Verify(
                    t => t.UpdateDocument(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<IExecutionContext>()),
                    Times.Once);
                Assert.AreEqual("en_" + TestData.TestItemKey.ToLower(), (string)doc["objectID"]);
                Assert.AreEqual("/sitecore/content/source", (string)doc["_fullpath"]);
                Assert.AreEqual("source", (string)doc["_name"]);
                Assert.AreEqual("en", (string)doc["_language"]);
                Assert.AreEqual(TestData.TestItemId.ToString(), (string)doc["_id"]);
            }
        }

        [Test]
        public void AddTest()
        {
            // arrange
            using (var db = new Db {_source})
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);
                JObject doc = null;

                var context = new Mock<IProviderUpdateContext>();
                context.Setup(
                    t => t.AddDocument(It.IsAny<object>(), It.IsAny<IExecutionContext>()))
                    .Callback(
                        (object itemToUpdate, IExecutionContext executionContext) =>
                            doc = itemToUpdate as JObject);

                var index = new IndexBuilder().Build();
                context.Setup(t => t.Index).Returns(index);

                var operations = new AlgoliaIndexOperations(index);

                //Act
                operations.Add(indexable, context.Object, new ProviderIndexConfiguration());

                //Assert
                context.Verify(t => t.AddDocument(It.IsAny<object>(), It.IsAny<IExecutionContext>()), Times.Once);
                Assert.AreEqual("en_" + TestData.TestItemKey.ToLower(), (string) doc["objectID"]);
                Assert.AreEqual("/sitecore/content/source", (string)doc["_fullpath"]);
                Assert.AreEqual("source", (string)doc["_name"]);
                Assert.AreEqual("en", (string)doc["_language"]);
                Assert.AreEqual(TestData.TestItemId.ToString(), (string)doc["_id"]);
            }
        }
    }
}
