using FluentAssertions;
using NUnit.Framework;
using Score.ContentSearch.Algolia.ComputedFields;
using Score.ContentSearch.Algolia.Tests.Builders;
using Sitecore;
using Sitecore.ContentSearch;
using Sitecore.FakeDb;
using Sitecore.FakeDb.Sites;

namespace Score.ContentSearch.Algolia.Tests.ComputedFields
{
    [TestFixture]
    public class CurrentItemLinkFieldTest
    {
        [Test]
        public void ShouldComputeValueForNoConfiguration()
        {
            //Arrange
            using (var db = new Db {new ItemBuilder().Build()})
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
            using (new FakeSiteContextSwitcher(fakeSite))
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

        [Test]
        public void ShouldLoadSite()
        {
            //Arrange
            var fakeSite = new FakeSiteContext(
                new Sitecore.Collections.StringDictionary
                {
                    {"name", "website"},
                    {"database", "web"},
                    {"cdTargetHostName", "cdsite"}
                });

            // switch the context site
            using (new FakeSiteContextSwitcher(fakeSite))
            using (var db = new Db {new ItemBuilder().Build()})
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);

                var sut = new CurrentItemLinkField();
                sut.Site = Context.Site.Name;

                //Act
                var actual = sut.ComputeFieldValue(indexable);

                //Assert
                actual.Should().Be("//cdsite/en/sitecore/content/source.aspx");
            }
        }
    }
}
