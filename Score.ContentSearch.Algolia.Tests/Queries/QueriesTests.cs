using System;
using System.Collections.Generic;
using System.Linq;
using Algolia.Search;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Score.ContentSearch.Algolia.Abstract;
using Score.ContentSearch.Algolia.Tests.Builders;
using Score.ContentSearch.Algolia.Tests.Helpers;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.FakeDb;

namespace Score.ContentSearch.Algolia.Tests.Queries
{
    [TestFixture]
    public class QueriesTests
    {
        [Test]
        public void TakeTest()
        {
            TestQuery(
                items => items.Take(10).ToList(), 
                query => query.AssertContains("hitsPerPage=10")
            );
        }

        [Test]
        public void TakeSkipTest()
        {
            TestQuery(
                items => items.Take(10).Skip(10).ToList(),
                query =>
                {
                    query.AssertContains("hitsPerPage=10");
                    query.AssertContains("page=1");
                }
            );
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void SkipShouldThrowWithoutTake()
        {
            TestQuery(
                items => items.Skip(10).ToList(),
                query =>
                {
                  
                }
            );
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void SkipShouldThrowIfCannotBeTranslatedToPaging()
        {
            TestQuery(
                items => items.Take(3).Skip(10).ToList(),
                query =>
                {

                }
            );
        }

        private void TestQuery(
            Func<IQueryable<SearchResultItem>, IList<SearchResultItem>> actFunction, 
            Action<Query> assert)
        {
            using (var db = new Db { new ItemBuilder().WithDisplayName("test").Build() })
            {
                var repository = new Mock<IAlgoliaRepository>();
                Query query = null;
                repository.Setup(t => t.SearchAsync(It.IsAny<Query>()))
                    .ReturnsAsync(new JObject())
                    .Callback((Query q) => query = q);

                var index = new AlgoliaBaseIndex("algolia_master_index", repository.Object, new NullPropertyStore());

                using (var context = index.CreateSearchContext())
                {
                    var queryable = context.GetQueryable<SearchResultItem>();

                    //Act
                    var actual = actFunction(queryable);

                    //Assert
                    Assert.IsNotNull(actual);
                    repository.Verify(t => t.SearchAsync(It.IsAny<Query>()), Times.Once);

                    assert(query);
                }
            }
        }

    }
}
