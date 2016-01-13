using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Score.ContentSearch.Algolia.Abstract;
using Score.ContentSearch.Algolia.Tests.Builders;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.Data;
using Sitecore.FakeDb;

namespace Score.ContentSearch.Algolia.Tests
{
    [TestFixture]
    public class AlgoliaCrawlerTests
    {
        [TestCase("Show In Search Results", false)]
        [TestCase("show in search results", false)]
        [TestCase("", true)]
        [TestCase(null, true)]
        [TestCase("Another Field", true)]
        public void CrawlerExludeNotShowInSearchResultsDocuments(string showInSearchResultsFieldName, bool shouldBeCrawled)
        {
            // arrange
            var source = new DbItem("source", TestData.TestItemId, TestData.TestTemplateId)
            {
                {"Show In Search Results", "" }
            };
            using (var db = new Db {source})
            {
                var item = db.GetItem("/sitecore/content/source");
                item.Should().NotBeNull();

                var repository = new Mock<IAlgoliaRepository>();
                repository.Setup(t => t.ClearIndexAsync()).ReturnsAsync(JObject.Parse(@"{""taskID"": 722}"));

                var sut = new AlgoliaBaseIndex("test", repository.Object);
                sut.PropertyStore = new NullPropertyStore();
                var configuration = new AlgoliaIndexConfiguration
                {
                    DocumentOptions = new DocumentBuilderOptions()
                };

                sut.Configuration = configuration;
                var crawler = new AlgoliaCrawler
                {
                    Database = "master",
                    Root = "/sitecore/content",
                    ShowInSearchResultsFieldName = showInSearchResultsFieldName,
                };
                sut.Crawlers.Add(crawler);
                crawler.Initialize(sut);
                sut.Initialize();

                //Act
                sut.Rebuild();

                //Assert
                repository.Verify(t => t.SaveObjectsAsync(It.Is<IEnumerable<JObject>>(o => o.Any() == shouldBeCrawled)), Times.Once);
            }
        }
    }

}
