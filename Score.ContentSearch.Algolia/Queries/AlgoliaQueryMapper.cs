using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentSearch.Linq.Extensions;
using Sitecore.ContentSearch.Linq.Methods;
using Sitecore.ContentSearch.Linq.Nodes;
using Sitecore.ContentSearch.Linq.Parsing;
using Query = Algolia.Search.Query;

namespace Score.ContentSearch.Algolia.Queries
{
    public class AlgoliaQueryMapper : QueryMapper<AlgoliaQuery>
    {
        public override AlgoliaQuery MapQuery(IndexQuery query)
        {
            var mappingState = new AlgoliaQueryMapperState();
            this.Visit(query.RootNode, mappingState);
            var algoliaQuery = LoadFromState(mappingState);
            return new AlgoliaQuery(algoliaQuery);
        }

        private Query LoadFromState(AlgoliaQueryMapperState mappingState)
        {
            var query = new Query();

            var takeMethod = mappingState.AdditionalQueryMethods.OfType<TakeMethod>().FirstOrDefault();
            var skipMethod = mappingState.AdditionalQueryMethods.OfType<SkipMethod>().FirstOrDefault();

            if (takeMethod != null)
            {
                int take = takeMethod.Count;
                query.SetNbHitsPerPage(take);
                if (skipMethod != null)
                {
                    var skip = skipMethod.Count;

                    if (skip % take > 0)
                        throw new Exception("Skip and Take cannot be translated to number of pages");

                    var page = skip/take;
                    query.SetPage(page);
                }
            }
            else
            {
                if (skipMethod != null)
                    throw new Exception("Skip cannot be used without Take.");
            }
            
            //foreach (var method in mappingState.AdditionalQueryMethods)
            //{
            //    var takeMethod = method as TakeMethod;
            //    if (takeMethod != null)
            //    {
            //        query.SetNbHitsPerPage(takeMethod.Count);
            //        continue;
            //    }
            //    var skipMethod = method as SkipMethod;
            //    if (skipMethod != null)
            //    {
            //        query.SetPage(skipMethod.Count);
            //        continue;
            //    }
            //}
            return query;
        }

        protected virtual QueryNode VisitConstant(ConstantNode node, AlgoliaQueryMapperState mappingState)
        {
            var queryableType = typeof(System.Linq.IQueryable);
            if (node.Type.IsAssignableTo(queryableType))
            {
                return new MatchAllNode();
            }
            return node;
        }

        protected virtual QueryNode VisitMatchAll(MatchAllNode node, AlgoliaQueryMapperState mappingState)
        {
            return new MatchAllNode();
        }

        protected virtual QueryNode Visit(QueryNode node, AlgoliaQueryMapperState mappingState)
        {
            switch (node.NodeType)
            {
                case QueryNodeType.Take:
                    		this.StripTake((TakeNode)node, mappingState.AdditionalQueryMethods);
                            return this.Visit(((TakeNode)node).SourceNode, mappingState);
                case QueryNodeType.Skip:
                            this.StripSkip((SkipNode)node, mappingState.AdditionalQueryMethods);
                            return this.Visit(((SkipNode)node).SourceNode, mappingState);
                case QueryNodeType.Constant:
                            return this.VisitConstant((ConstantNode)node, mappingState);
                case QueryNodeType.MatchAll:
                            return this.VisitMatchAll((MatchAllNode)node, mappingState);
                //return HandleTake((TakeNode)node);
                //case QueryNodeType.Equal:
                //    return HandleEqual((EqualNode)node);
                //case QueryNodeType.MatchAll:
                //    return string.Empty;
            }
            throw new NotSupportedException(string.Format("The query node type '{0}' is not supported in this context.", node.NodeType));
        }

        private void StripTake(TakeNode node, List<QueryMethod> methods)
        {
            methods.Add(new TakeMethod(node.Count));
        }

        protected virtual void StripSkip(SkipNode node, List<QueryMethod> methods)
        {
            methods.Add(new SkipMethod(node.Count));
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
