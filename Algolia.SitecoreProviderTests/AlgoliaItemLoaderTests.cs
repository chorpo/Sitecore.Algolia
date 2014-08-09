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
using Sitecore.Data;
using Sitecore.FakeDb;

namespace Algolia.SitecoreProviderTests
{
    [TestFixture]
    public class AlgoliaItemLoaderTests
    {
        private DbItem _source;

        [SetUp]
        public void SetUp()
        {
            _source = new DbItem("source", new ID(TestData.TestItemId));
        }

        [Test]
        public void FieldsShouldBeLoaded()
        {
            // arrange
            using (var db = new Db { _source })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);

                var sut = new AlgoliaItemLoader();

                //Act
                var actual = sut.Load(indexable);

                //Assert
                Assert.AreEqual(TestData.TestItemKey.ToLower(), actual.objectID);
                Assert.AreEqual("/sitecore/content/source", actual.path);
                Assert.AreEqual("source", actual.name);
            }
        }
    }
}
