using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
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
                var parents = (JArray)doc["parents"];
                parents.Count.Should().Be(1);
                ((string)parents.First).Should().Be(TestData.TestItemId.ToString());
            }
        }
    }
}
