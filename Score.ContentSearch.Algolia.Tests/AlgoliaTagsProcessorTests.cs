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
        public void AtrrayShouldBeAddedAsTag()
        {
            //Arrange
            var config = new List<AlgoliaTagConfig>
            {
                new AlgoliaTagConfig
                {
                    FieldName = "data"
                }
            };

            var doc = new JObject();
            doc["data"] = new JArray()
            {
                "first",
                "second"
            };
            var sut = new AlgoliaTagsProcessor(config);

            //Act
            sut.ProcessDocument(doc);

            //Assert
            doc["_tags"].Count().Should().Be(2);
            (doc["_tags"]).First(token => token.Value<string>() == "first");
            (doc["_tags"]).First(token => token.Value<string>() == "second");
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

        [Test]
        public void TransformedTagsShouldNotBeDuplicated()
        {
            //Arrange
            var config = new List<AlgoliaTagConfig>
            {
                new AlgoliaTagConfig
                {
                    FieldName = "tag"
                },
                 new AlgoliaTagConfig
                {
                    FieldName = "tag"
                }
            };

            var doc = new JObject();
            doc["tag"] = "myId";
            var sut = new AlgoliaTagsProcessor(config);

            //Act
            sut.ProcessDocument(doc);

            //Assert
            var tags = (doc["_tags"]).ToObject<string[]>();
            tags.Count(t => t == "myId").Should().Be(1);
        }

        [Test]
        public void OverridenTagsShouldNotBeDuplicated()
        {
            //Arrange
            var config = new List<AlgoliaTagConfig>
            {
                new AlgoliaTagConfig
                {
                    FieldName = "tag"
                },
            };

            var doc = new JObject();
            doc["tag"] = "myId";
            doc["_tags"] = new JArray()
            {
                new JValue ("myId")
            };
            var sut = new AlgoliaTagsProcessor(config);

            //Act
            sut.ProcessDocument(doc);

            //Assert
            var tags = (doc["_tags"]).ToObject<string[]>();
            tags.Count(t => t == "myId").Should().Be(1);
        }
    }


}
