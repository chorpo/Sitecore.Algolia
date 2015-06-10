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
    public class AlgoliaBaseIndexTests
    {
        private DbItem _source;

        [SetUp]
        public void SetUp()
        {
            _source = new DbItem("source", new ID(TestData.TestItemId));
        }

        [Test]
        public void RebuildTest()
        {
            // arrange
            using (var db = new Db { _source })
            {
                var item = db.GetItem("/sitecore/content/source");
                item.Should().NotBeNull();

                var repository = new Mock<IAlgoliaRepository>();

                var sut = new AlgoliaBaseIndex("test", repository.Object);
                sut.PropertyStore = new NullPropertyStore();
                var configuration = new AlgoliaIndexConfiguration();
                configuration.DocumentOptions = new DocumentBuilderOptions();
                sut.Configuration = configuration;
                var crowler = new SitecoreItemCrawler();
                crowler.Database = "master";
                crowler.Root = "/sitecore/content";
                sut.Crawlers.Add(crowler);
                crowler.Initialize(sut);
                
                //Act
                sut.Rebuild();

                //Assert
                repository.Verify(t => t.SaveObjectsAsync(It.Is<IEnumerable<JObject>>(o => o.Count() == 1)), Times.Once);
            }
        }
    }
}
