using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Score.ContentSearch.Algolia.ComputedFields;
using Score.ContentSearch.Algolia.Tests.Builders;
using Sitecore.ContentSearch;
using Sitecore.FakeDb;
using Sitecore.Links;

namespace Score.ContentSearch.Algolia.Tests.ComputedFields
{
    [TestFixture]
    public class CurrentItemLinkFieldTest
    {
        [Test]
        public void ShouldComputeValueForNoConfiguration()
        {
            //Arrange
            using (var db = new Db { new ItemBuilder().Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);

                var sut = new CurrentItemLinkField();

                //Act
                var actual = sut.ComputeFieldValue(indexable);

                //Assert
                actual.Should().Be("/en/sitecore/content/source.aspx");
            }
        }

        [Test]
        public void ShouldUseSite()
        {
            var fakeSite = new Sitecore.FakeDb.Sites.FakeSiteContext(
                new Sitecore.Collections.StringDictionary
                {
                    {"name", "website"},
                    {"rootPath", "/sitecore"}
                });
            //Arrange
            using (new Sitecore.Sites.SiteContextSwitcher(fakeSite))
            using (var db = new Db {new ItemBuilder().Build()})
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);

                var sut = new CurrentItemLinkField();

                //Act
                var actual = sut.ComputeFieldValue(indexable);

                //Assert
                actual.Should().Be("/en/content/source.aspx");
            }
        }
    }
}
