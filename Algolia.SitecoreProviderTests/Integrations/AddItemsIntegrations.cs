using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algolia.SitecoreProvider;
using NUnit.Framework;
using Sitecore.ContentSearch;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.FakeDb;

namespace Algolia.SitecoreProviderTests.Integrations
{
    [TestFixture]
    public class AddItemsIntegrations
    {
        private DbItem _source;

        private Guid testItemId = Guid.Parse("764A2503-59CA-4825-983C-A19000F44797");

        [SetUp]
        public void SetUp()
        {
            _source = new DbItem("source", new ID(testItemId)) { new DbItem("child") };
        }

        [Test]
        public void AddItemsIntegration()
        {
            // arrange
            using (var db = new Db {_source})
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);
                var index = new AlgoliaSearchIndex("3Q92VD0BCR", "ae8283da0a1342b03749ed1355d9d4a7", "sitecore");
                var context = index.CreateUpdateContext();
                
                var operations = new AlgoliaIndexOperations();

                //Act
                operations.Update(indexable, context, new ProviderIndexConfiguration());
                context.Commit();
                //Assert
            }
        }
    }
}
