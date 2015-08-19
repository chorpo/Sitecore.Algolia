using System;
using Sitecore.ContentSearch.Linq.Extensions;
using Sitecore.ContentSearch.Linq.Nodes;
using Sitecore.ContentSearch.Linq.Parsing;

namespace Score.ContentSearch.Algolia.Queries
{
    public class AlgoliaQueryOptimizer : QueryOptimizer<AlgoliaQueryOptimizerState>
    {
        protected virtual QueryNode VisitTake(TakeNode node, AlgoliaQueryOptimizerState state)
        {
            var sourceNode = this.Visit(node.SourceNode, state);
            return new TakeNode(sourceNode, node.Count);
        }

        protected virtual QueryNode VisitConstant(ConstantNode node, AlgoliaQueryOptimizerState state)
        {
            Type typeFromHandle = typeof(System.Linq.IQueryable);
            if (node.Type.IsAssignableTo(typeFromHandle))
            {
                return new MatchAllNode();
            }
            return node;
        }

        protected virtual QueryNode VisitSkip(SkipNode node, AlgoliaQueryOptimizerState state)
        {
            QueryNode sourceNode = this.Visit(node.SourceNode, state);
            return new SkipNode(sourceNode, node.Count);
        }

        protected virtual QueryNode VisitWhere(WhereNode node, AlgoliaQueryOptimizerState state)
        {
            var predicate = this.Visit(node.PredicateNode, state);
            return predicate;
        }

        protected override QueryNode Visit(QueryNode node, AlgoliaQueryOptimizerState state)
        {
            switch (node.NodeType)
            {
                case QueryNodeType.Where:
                    return this.VisitWhere((WhereNode)node, state);
                case QueryNodeType.Take:
                    return this.VisitTake((TakeNode)node, state);
                case QueryNodeType.Constant:
                    return this.VisitConstant((ConstantNode)node, state);
            }
            return node;
        }
    }
}
