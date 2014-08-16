using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algolia.Search;
using Algolia.SitecoreProvider;
using Algolia.SitecoreProvider.Abstract;
using Algolia.SitecoreProviderTests.Builders;
using Moq;
using NUnit.Framework;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.FakeDb;

namespace Algolia.SitecoreProviderTests.Queries
{
    [TestFixture]
    public class QueriesTests
    {
        [Test]
        public void Test()
        {
            using (var db = new Db {new ItemBuilder().WithDisplayName("test").Build()})
            {
                var repository = new Mock<IAlgoliaRepository>();

                var index = new AlgoliaBaseIndex("algolia_master_index", repository.Object, new NullPropertyStore());
                
                using (var context = index.CreateSearchContext())
                {
                    var queryable = context.GetQueryable<SearchResultItem>();

                    //Act
                    var actual =  queryable//.Where(item => item.Language == "en")
                        .Take(10).ToList();

                    //Assert
                    Assert.IsNotNull(actual);
                    repository.Verify(t => t.SearchAsync(It.IsAny<Query>()));
                }
            }
        }

    }
}
