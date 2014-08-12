using Sitecore.ContentSearch.Linq.Nodes;
using Sitecore.ContentSearch.Linq.Parsing;

namespace Algolia.SitecoreProvider.Queries
{
    class AlgoliaQueryOptimizer : QueryOptimizer<AlgoliaQueryOptimizerState>
    {
        protected override QueryNode Visit(QueryNode node, AlgoliaQueryOptimizerState state)
        {
            throw new System.NotImplementedException();
        }
    }
}
