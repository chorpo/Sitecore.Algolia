﻿using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore.ContentSearch;
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
            _source = new DbItem("source", new ID(TestData.TestItemGuid), TestData.TestTemplateId);
        }

        [Test]
        public void RebuildTest()
        {
            // arrange
            using (var db = new Db {_source})
            {
                var item = db.GetItem("/sitecore/content/source");
                item.Should().NotBeNull();

                var repository = new Mock<IAlgoliaRepository>();
                repository.Setup(t => t.ClearIndexAsync()).ReturnsAsync(JObject.Parse(@"{""taskID"": 722}"));

                var sut = new AlgoliaBaseIndex("test", repository.Object);
                sut.PropertyStore = new NullPropertyStore();
                var configuration = new AlgoliaIndexConfiguration();
                configuration.DocumentOptions = new DocumentBuilderOptions();
                sut.Configuration = configuration;
                var crawler = new SitecoreItemCrawler
                {
                    Database = "master",
                    Root = "/sitecore/content"
                };
                sut.Crawlers.Add(crawler);
                crawler.Initialize(sut);
                sut.Initialize();

                //Act
                sut.Rebuild();

                //Assert
                repository.Verify(t => t.SaveObjectsAsync(It.Is<IEnumerable<JObject>>(o => o.Count() == 1)), Times.Once);
            }
        }

        [Test]
        public void CrawlerShouldExcludeTemplates()
        {
            // arrange
            using (var db = new Db {_source})
            {
                var item = db.GetItem("/sitecore/content/source");
                item.Should().NotBeNull();

                var repository = new Mock<IAlgoliaRepository>();
                repository.Setup(t => t.ClearIndexAsync()).ReturnsAsync(JObject.Parse(@"{""taskID"": 722}"));

                var sut = new AlgoliaBaseIndex("test", repository.Object);
                sut.PropertyStore = new NullPropertyStore();
                var configuration = new AlgoliaIndexConfiguration();
                configuration.DocumentOptions = new DocumentBuilderOptions();
                configuration.DocumentOptions.AddExcludedTemplate(TestData.TestTemplateId.ToString());

                sut.Configuration = configuration;
                var crawler = new SitecoreItemCrawler();
                crawler.Database = "master";
                crawler.Root = "/sitecore/content";
                sut.Crawlers.Add(crawler);
                crawler.Initialize(sut);
                sut.Initialize();

                //Act
                sut.Rebuild();

                //Assert
                repository.Verify(t => t.SaveObjectsAsync(It.Is<IEnumerable<JObject>>(o => !o.Any())), Times.Never);
            }
        }

        [Test]
        public void CrawlerShouldIncludeTemplates()
        {
            // arrange
            using (var db = new Db {_source})
            {
                var item = db.GetItem("/sitecore/content/source");
                item.Should().NotBeNull();

                var repository = new Mock<IAlgoliaRepository>();
                repository.Setup(t => t.ClearIndexAsync()).ReturnsAsync(JObject.Parse(@"{""taskID"": 722}"));

                var sut = new AlgoliaBaseIndex("test", repository.Object);
                sut.PropertyStore = new NullPropertyStore();
                var configuration = new AlgoliaIndexConfiguration();
                configuration.DocumentOptions = new DocumentBuilderOptions();
                configuration.DocumentOptions.AddIncludedTemplate(TestData.TestTemplateId.ToString());

                sut.Configuration = configuration;
                var crawler = new SitecoreItemCrawler();
                crawler.Database = "master";
                crawler.Root = "/sitecore/content";
                sut.Crawlers.Add(crawler);
                crawler.Initialize(sut);
                sut.Initialize();

                //Act
                sut.Rebuild();

                //Assert
                repository.Verify(t => t.SaveObjectsAsync(It.Is<IEnumerable<JObject>>(o => o.Any())), Times.Once);
            }
        }

        [Test]
        public void CrawlerShouldIncludeOnlyDefinedTemplates()
        {
            // arrange
            using (var db = new Db {_source})
            {
                var item = db.GetItem("/sitecore/content/source");
                item.Should().NotBeNull();

                var repository = new Mock<IAlgoliaRepository>();
                repository.Setup(t => t.ClearIndexAsync()).ReturnsAsync(JObject.Parse(@"{""taskID"": 722}"));

                var sut = new AlgoliaBaseIndex("test", repository.Object);
                sut.PropertyStore = new NullPropertyStore();
                var configuration = new AlgoliaIndexConfiguration();
                configuration.DocumentOptions = new DocumentBuilderOptions();
                //Our Template should be exluded in IncludeTemplate is not empty
                configuration.DocumentOptions.AddIncludedTemplate(ID.NewID.ToString());

                sut.Configuration = configuration;
                var crawler = new SitecoreItemCrawler();
                crawler.Database = "master";
                crawler.Root = "/sitecore/content";
                sut.Crawlers.Add(crawler);
                crawler.Initialize(sut);
                sut.Initialize();

                //Act
                sut.Rebuild();

                //Assert
                repository.Verify(t => t.SaveObjectsAsync(It.Is<IEnumerable<JObject>>(o => !o.Any())), Times.Never);
            }
        }

        [Test]
        public void DeleteTest()
        {
            // arrange
            using (var db = new Db {_source})
            {
                var item = db.GetItem("/sitecore/content/source");
                item.Should().NotBeNull();

                string id = string.Empty;

                var repository = new Mock<IAlgoliaRepository>();
                repository.Setup(t => t.DeleteAllObjByTag(It.IsAny<string>()))
                    .ReturnsAsync(1)
                    .Callback<string>(s => id = s);

                var sut = new AlgoliaBaseIndex("test", repository.Object);
                sut.PropertyStore = new NullPropertyStore();
                var configuration = new AlgoliaIndexConfiguration();
                configuration.DocumentOptions = new DocumentBuilderOptions();
                sut.Configuration = configuration;
                var crawler = new SitecoreItemCrawler();
                crawler.Database = "master";
                crawler.Root = "/sitecore/content";
                sut.Crawlers.Add(crawler);
                crawler.Initialize(sut);
                sut.Initialize();

                //Act
                sut.Delete(new IndexableId<ID>(item.ID));

                //Assert
                repository.Verify(t => t.DeleteAllObjByTag(It.IsAny<string>()), Times.Once);
                id.Should().Be("id_" + item.ID);
                repository.Verify(t => t.DeleteAllObjByTag("id_" + item.ID.ToString()), Times.Once);
            }
        }

    }
}
