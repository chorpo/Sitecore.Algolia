using System;
using System.Text;
using Algolia.Search;
using Sitecore.ContentSearch.Linq.Helpers;
using Sitecore.ContentSearch.Linq.Nodes;
using Sitecore.ContentSearch.Linq.Parsing;
using IndexQuery = Sitecore.ContentSearch.Linq.Parsing.IndexQuery;

namespace Algolia.SitecoreProvider.Queries
{
    public class AlgoliaQueryMapper : QueryMapper<AlgoliaQuery>
    {
        public override AlgoliaQuery MapQuery(IndexQuery query)
        {
            var algoliaQuery = this.HandleNode(query.RootNode);
            return new AlgoliaQuery(algoliaQuery);
        }
        
        protected virtual Query HandleTake(TakeNode node)
        {
            var query = HandleNode(node.SourceNode);
            query.SetNbHitsPerPage(node.Count);
            return query;
        }

        protected virtual string HandleEqual(EqualNode node)
        {
            var fieldNode = QueryHelper.GetFieldNode(node);
            var valueNode = QueryHelper.GetValueNode<string>(node);
            var result = string.Format("[field[@name='{0}'] = '{1}']", fieldNode.FieldKey, valueNode.Value);
            return result;
        }

        protected virtual Query HandleNode(QueryNode node)
        {
            switch (node.NodeType)
            {
                case QueryNodeType.Take:
                    return HandleTake((TakeNode)node);
                //case QueryNodeType.Equal:
                //    return HandleEqual((EqualNode)node);
                //case QueryNodeType.MatchAll:
                //    return string.Empty;
            }
            throw new NotSupportedException(string.Format("The query node type '{0}' is not supported in this context.", node.NodeType));
        }

        //protected virtual string HandleTake(TakeNode node)
        //{
        //    var builder = new StringBuilder();
        //    var takeExpression = string.Format("[position()<={0}]", node.Count);
        //    var sourceExpression = HandleNode(node.SourceNode);
        //    if (sourceExpression != string.Empty)
        //    {
        //        builder.Append(sourceExpression);
        //    }
        //    builder.Append(takeExpression);
        //    return builder.ToString();
        //}

        //protected virtual string HandleEqual(EqualNode node)
        //{
        //    var fieldNode = QueryHelper.GetFieldNode(node);
        //    var valueNode = QueryHelper.GetValueNode<string>(node);
        //    var result = string.Format("[field[@name='{0}'] = '{1}']", fieldNode.FieldKey, valueNode.Value);
        //    return result;
        //}

        //protected virtual string HandleNode(QueryNode node)
        //{
        //    switch (node.NodeType)
        //    {
        //        case QueryNodeType.Take:
        //            return HandleTake((TakeNode)node);
        //        case QueryNodeType.Equal:
        //            return HandleEqual((EqualNode)node);
        //        case QueryNodeType.MatchAll:
        //            return string.Empty;
        //    }
        //    throw new NotSupportedException(string.Format("The query node type '{0}' is not supported in this context.", node.NodeType));
        //}
    }
}
