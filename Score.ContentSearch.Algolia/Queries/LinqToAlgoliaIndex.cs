using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Indexing;
using Sitecore.ContentSearch.Linq.Parsing;
using Sitecore.ContentSearch.Security;
using Sitecore.Diagnostics;

namespace Score.ContentSearch.Algolia.Queries
{
    public class LinqToAlgoliaIndex<TItem> : Index<TItem, AlgoliaQuery>
    {
        private readonly AlgoliaSearchContext _context;
        private readonly IAlgoliaRepository _repository;
        private readonly ProviderIndexConfiguration _configuration;
        private readonly AlgoliaQueryOptimizer _queryOptimizer;
        private readonly AlgoliaQueryMapper _mapper;
        private readonly AbstractFieldNameTranslator _fieldNameTranslator;

        public LinqToAlgoliaIndex(AlgoliaSearchContext context) : this(context, null)
        {
        }

        public LinqToAlgoliaIndex(AlgoliaSearchContext context,
            IExecutionContext executionContext)
        {
            Assert.ArgumentNotNull(context, "context");
            _context = context;

            var index = context.Index as AlgoliaBaseIndex;

            if (index == null)
                throw new ArgumentException("context.Index should be instance of AlgoliaBaseIndex");

            _repository = index.Repository;
            _configuration = context.Index.Configuration;
            _queryOptimizer = new AlgoliaQueryOptimizer();
            _mapper = new AlgoliaQueryMapper();
            _fieldNameTranslator = context.Index.FieldNameTranslator;
        }

        //public override IQueryable<TItem> GetQueryable()
        //{
        //    IQueryable<TItem> queryable = new GenericQueryable<TItem, AlgoliaQuery>(this, this.QueryMapper, this.QueryOptimizer, this.FieldNameTranslator);
        //    //(queryable as IHasTraceWriter).TraceWriter = this.TraceWriter;
        //    foreach (IPredefinedQueryAttribute predefinedQueryAttribute in 
        //        GetTypeInheritanceEx(typeof(TItem))
        //        .SelectMany((t => t.GetCustomAttributes(typeof(IPredefinedQueryAttribute), true)
        //            .Cast<IPredefinedQueryAttribute>())).ToList())
        //        queryable = predefinedQueryAttribute.ApplyFilter(queryable, ValueFormatter);
        //    return queryable;
        //}

        //private IEnumerable<Type> GetTypeInheritanceEx(Type type)
        //{
        //    yield return type;
        //    for (Type baseType = type.BaseType; baseType != (Type)null; baseType = baseType.BaseType)
        //        yield return baseType;
        //}

        public override TResult Execute<TResult>(AlgoliaQuery query)
        {
            return default(TResult); 
        }

        public override IEnumerable<TElement> FindElements<TElement>(AlgoliaQuery query)
        {
            SearchLog.Log.Debug("Executing query: " + query.ToString());
            var index = _context.Index as AlgoliaBaseIndex;
            Assert.IsNotNull(index, "context.Index is not an instance of AlgoliaSearchIndex");

            var response = _repository.SearchAsync(query.Query).Result;

            var nodes = response["hits"];
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var jobj = node as JObject;

                    if (jobj == null)
                        throw new Exception("Wrong type");

                    yield return   new DefaultAlgoliaDocumentTypeMapper().MapToType<TElement>(jobj, null, null, 
                        SearchSecurityOptions.DisableSecurityCheck);
                }
            }
        }

        protected override QueryMapper<AlgoliaQuery> QueryMapper
        {
            get { return _mapper; }
        }

        protected override IQueryOptimizer QueryOptimizer
        {
            get { return _queryOptimizer; }
        }

        protected override FieldNameTranslator FieldNameTranslator
        {
            get { return _fieldNameTranslator; }
        }

        protected override IIndexValueFormatter ValueFormatter
        {
            get { throw new NotImplementedException(); }
        }
    }
}
