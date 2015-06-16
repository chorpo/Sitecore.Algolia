using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Score.ContentSearch.Algolia.Tests
{
    [TestFixture]
    public class AlgoliaTagsProcessorTests
    {
        [Test]
        public void EmptyConfigShouldNotFail()
        {
            //Arrange
            var config = new List<AlgoliaTagConfig>();
            var doc = new JObject();
            var sut = new AlgoliaTagsProcessor(config);

            //Act
            sut.ProcessDocument(doc);

            //Assert
            doc.Should().NotBeNull();
        }

        [Test]
        public void NoFieldShouldNotFail()
        {
            //Arrange
            var config = new List<AlgoliaTagConfig>
            {
                new AlgoliaTagConfig
                {
                    FieldName = "_id"
                }
            };

            var doc = new JObject();
            var sut = new AlgoliaTagsProcessor(config);

            //Act
            sut.ProcessDocument(doc);

            //Assert
            doc.Should().NotBeNull();
            doc["_id"].Should().BeNull();
            doc["_tags"].Should().BeNull();
        }

        [Test]
        public void FieldShouldBeAddedAsTag()
        {
            //Arrange
            var config = new List<AlgoliaTagConfig>
            {
                new AlgoliaTagConfig
                {
                    FieldName = "_id"
                }
            };

            var doc = new JObject();
            doc["_id"] = "myId";
            var sut = new AlgoliaTagsProcessor(config);

            //Act
            sut.ProcessDocument(doc);

            //Assert
            ((string) doc["_id"]).Should().Be("myId");
            (doc["_tags"]).First(token => token.Value<string>() == "myId");
        }

        [Test]
        public void ConfigWithHideFieldShouldRemoveField()
        {
            //Arrange
            var config = new List<AlgoliaTagConfig>
            {
                new AlgoliaTagConfig
                {
                    FieldName = "_id",
                    HideField = true
                }
            };

            var doc = new JObject();
            doc["_id"] = "myId";
            var sut = new AlgoliaTagsProcessor(config);

            //Act
            sut.ProcessDocument(doc);

            //Assert
            doc["_id"].Should().BeNull();
            (doc["_tags"]).First(token => token.Value<string>() == "myId");
        }

        [Test]
        public void PreffixShouldBeAddedToTag()
        {
            //Arrange
            var config = new List<AlgoliaTagConfig>
            {
                new AlgoliaTagConfig
                {
                    FieldName = "_id",
                    TagPreffix = "id_"
                }
            };

            var doc = new JObject();
            doc["_id"] = "myId";
            var sut = new AlgoliaTagsProcessor(config);

            //Act
            sut.ProcessDocument(doc);

            //Assert
            ((string)doc["_id"]).Should().Be("myId");
            (doc["_tags"]).First(token => token.Value<string>() == "id_myId");
        }
    }


}
