using System.Collections.Generic;
using System.Linq;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;

namespace Score.ContentSearch.Algolia.Tests
{
    [TestFixture]
    public class AlgoliaUpdateContextTests
    {
        [TestCase(1, 1)]
        [TestCase(100, 1)]
        [TestCase(101, 2)]
        public void ShouldChunkUpdate(int docCount, int chunksCount)
        {
            //Arrange
            var repository = new Mock<IAlgoliaRepository>();
            var index = new Mock<ISearchIndex>();
            var sut = new AlgoliaUpdateContext(index.Object, repository.Object);

            for (int i = 0; i < docCount; i++)
            {
                sut.AddDocument(JObject.Parse("{\"objectID\": " + i + "}"), (IExecutionContext) null);
            }

            //Act
            sut.Commit();

            //Assert
            repository.Verify(t => t.SaveObjectsAsync(It.Is<IEnumerable<JObject>>(o => o.Any())),
                Times.Exactly(chunksCount));
        }
    }
}
