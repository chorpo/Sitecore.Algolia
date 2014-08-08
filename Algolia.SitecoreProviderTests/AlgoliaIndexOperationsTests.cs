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
    [Category("Integrated")]
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
                //var index = new AlgoliaSearchIndex("algolia_master_index", "3Q92VD0BCR", "ae8283da0a1342b03749ed1355d9d4a7", "sitecore", new NullPropertyStore());

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
                Assert.AreEqual(TestData.TestItemKey.ToLower(), (string)doc["id"]);
            }
        }
    }

}
