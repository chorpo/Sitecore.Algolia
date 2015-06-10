using System.Linq;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.Maintenance.Strategies;

namespace Score.ContentSearch.Algolia.Tests.Configuration
{
    [TestFixture]
    public class AlgoliaSearchIndexTests
    {

        [Test]
        public void ShouldCreateFromConfig()
        {
            //Act
            var index = LoadIndexConfiguration();
            
            //Assert
            index.Should().NotBeNull();
            index.Name.Should().Be("products_unstopables");
        }

        [Test]
        public void ShouldLoadPropertyStoreFromConfig()
        {
            //Act
            var index = LoadIndexConfiguration();

            //Assert
            index.PropertyStore.Should().NotBeNull();
            index.PropertyStore.Should().BeOfType<IndexDatabasePropertyStore>();
            var propertyStore = index.PropertyStore as IndexDatabasePropertyStore;
            propertyStore.Key.Should().Be("products_unstopables");
            propertyStore.Database.Should().Be("core");
        }

        [Test]
        public void ShouldLoadStrategy()
        {
            //Act
            var index = LoadIndexConfiguration();

            //Assert
            index.Strategies.Count.Should().Be(1);
            index.Strategies.First().Should().BeOfType<SynchronousStrategy>();
        }

        [Test]
        public void ShouldLoadCrawler()
        {
            //Act
            var index = LoadIndexConfiguration();

            //Assert
            index.Crawlers.Count.Should().Be(1);
            index.Crawlers.First().Should().BeOfType<SitecoreItemCrawler>();
            var crowler = index.Crawlers.First() as SitecoreItemCrawler;
            crowler.Database.Should().Be("master");
            crowler.Root.Should().Be("/sitecore/content/Unstopables/North America/United States/home/all-products");
        }

        private AlgoliaSearchIndex LoadIndexConfiguration()
        {
            //Arrange
            string xmlPath = @"Configuration\Algolia.Search.config";
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            XmlElement root = xmlDoc.DocumentElement;
            var configNode = root.SelectSingleNode("//configuration//sitecore//contentSearch//configuration//indexes//index");

            configNode.Should().NotBeNull();
            var factory = new FakeFactory(xmlDoc);

            //Act
            return factory.CreateObject<AlgoliaSearchIndex>(configNode);
        }

    }
}
