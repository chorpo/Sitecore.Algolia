using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Score.ContentSearch.Algolia.Dto;

namespace Score.ContentSearch.Algolia.Tests.Dto
{
    [TestFixture]
    public class AlgoliaIndexInfoTests
    {
        [Test]
        public void LoadFromJson()
        {
            //Arrange
            var str = @"{
    ""items"": [
        {
            ""name"": ""contacts"",
            ""createdAt"": ""2013-08-15T19:49:47.714Z"",
            ""updatedAt"": ""2013-08-17T07:59:28.313Z"",
            ""entries"": 2436442,
            ""pendingTask"": false,
            ""lastBuildTimeS"": 0,
            ""dataSize"": 224152664
        }
    ]
}";
            var data = JObject.Parse(str);

            //Act
            var actual = AlgoliaIndexInfo.LoadFromJson(data, "contacts");

            //Assert
            actual.CreatedAt.Date.Should().Be(15.August(2013));
            actual.UpdatedAt.Date.Should().Be(17.August(2013));
            actual.Entries.Should().Be(2436442);
            actual.PendingTask.Should().BeFalse();
            actual.LastBuildTimeS.Should().Be(0);
            actual.DataSize.Should().Be(224152664);
        }

        [Test]
        public void LoadFromJsonBadIndexName()
        {
            //Arrange
            var str = @"{
    ""items"": [
        {
            ""name"": ""wrong"",
        }
    ]
}";
            var data = JObject.Parse(str);

            //Act
            var actual = AlgoliaIndexInfo.LoadFromJson(data, "contacts");

            //Assert
            actual.Should().NotBeNull();
        }
    }
}
 