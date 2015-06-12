using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Score.ContentSearch.Algolia.Tests.Builders;
using Sitecore.ContentSearch;
using Sitecore.FakeDb;

namespace Score.ContentSearch.Algolia.Tests.AlgoliaDocumentBuilderTests
{
    [TestFixture]
    public class AddComputedIndexFieldsTests
    {
        [Test]
        public void ParentsShouldBeAdded()
        {
            // arrange
            using (var db = new Db { new ItemBuilder().AddSubItem().Build() })
            {
                var item = db.GetItem("/sitecore/content/source/subitem");
                var indexable = new SitecoreIndexableItem(item);

                var context = new Mock<IProviderUpdateContext>();
                var index = new IndexBuilder().WithParentsComputedField("parents")
                    .Build();
                context.Setup(t => t.Index).Returns(index);
                var sut = new AlgoliaDocumentBuilder(indexable, context.Object);

                //Act
                sut.AddComputedIndexFields();

                //Assert
                var doc = sut.Document;
                Assert.AreEqual(TestData.TestItemId.ToString(), (string)doc["parents"]);
            }
        }
    }
}
