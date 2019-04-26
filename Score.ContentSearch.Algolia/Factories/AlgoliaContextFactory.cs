using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Abstractions.Factories;
using Sitecore.ContentSearch.Security;

namespace Score.ContentSearch.Algolia.Factories
{
    public class AlgoliaContextFactory : AbstractContextFactory<AlgoliaBaseIndex>
    {
        protected override IProviderDeleteContext GetDeleteContext(AlgoliaBaseIndex searchIndex)
        {
            return searchIndex.CreateDeleteContext();
        }

        protected override IProviderSearchContext GetSearchContext(AlgoliaBaseIndex searchIndex, SearchSecurityOptions options)
        {
            return searchIndex.CreateSearchContext(options);
        }

        protected override IProviderUpdateContext GetUpdateContext(AlgoliaBaseIndex searchIndex)
        {
            return searchIndex.CreateUpdateContext();
        }
    }
}
