using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algolia.SitecoreProvider;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.Data;
using Sitecore.FakeDb;

namespace Algolia.SitecoreProviderTests
{
    [TestFixture]
    public class AlgoliaIndexOperationsTests
    {
        private DbItem _source;

        [SetUp]
        public void SetUp()
        {
            _source = new DbItem("source", new ID(TestData.TestItemId));
        }

        [Test]
        public void UpdateTest()
        {
            // arrange
            using (var db = new Db { _source })
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

                var operations = new AlgoliaIndexOperations();

                //Act
                operations.Update(indexable, context.Object, new ProviderIndexConfiguration());

                //Assert
                context.Verify(t => t.UpdateDocument(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<IExecutionContext>()), Times.Once);
                Assert.AreEqual(TestData.TestItemKey.ToLower(), (string)doc["objectID"]);
            }
        }

        [Test]
        public void AddTest()
        {
            // arrange
            using (var db = new Db { _source })
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

                var operations = new AlgoliaIndexOperations();

                //Act
                operations.Add(indexable, context.Object, new ProviderIndexConfiguration());

                //Assert
                context.Verify(t => t.AddDocument(It.IsAny<object>(), It.IsAny<IExecutionContext>()), Times.Once);
                Assert.AreEqual(TestData.TestItemKey.ToLower(), (string)doc["objectID"]);
            }
        }
    }

}
