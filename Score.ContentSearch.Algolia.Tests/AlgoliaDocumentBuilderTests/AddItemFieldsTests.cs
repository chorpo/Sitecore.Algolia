using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Score.ContentSearch.Algolia.Tests.Builders;
using Sitecore.ContentSearch;
using Sitecore.FakeDb;

namespace Score.ContentSearch.Algolia.Tests.AlgoliaDocumentBuilderTests
{
    public class AddItemFieldsTests
    {
        [Test]
        public void WithIndexAllFieldsShouldIncludeFields()
        {
            // arrange
            using (var db = new Db { new ItemBuilder().Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);

                var context = new Mock<IProviderUpdateContext>();
                var index = new IndexBuilder().Build();
                context.Setup(t => t.Index).Returns(index);
                var sut = new AlgoliaDocumentBuilder(indexable, context.Object);
                sut.Options.IndexAllFields = true;

                //Act
                sut.AddItemFields();

                //Assert
                JObject doc = sut.Document;
                doc.Properties().Count().Should().BeGreaterThan(4);
            }
        }
    }
}
