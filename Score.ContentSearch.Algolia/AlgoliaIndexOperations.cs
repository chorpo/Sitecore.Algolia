using System;
using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Pipelines.IndexingFilters;
using Sitecore.Events;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaIndexOperations : IIndexOperations
    {
        private readonly ISearchIndex _index;

        public AlgoliaIndexOperations(ISearchIndex index)
        {
            if (index == null) throw new ArgumentNullException("index");
            _index = index;
        }

        #region IIndexOperations

        public void Update(IIndexable indexable, IProviderUpdateContext context,
            ProviderIndexConfiguration indexConfiguration)
        {
            var doc = GetDocument(indexable, context);

            if (doc == null)
            {
                Event.RaiseEvent("indexing:excludedfromindex", new object[] { context.Index.Name, indexable.Id });
                return;
            }

            context.UpdateDocument(doc, null, (IExecutionContext) null);
        }

        public void Delete(IIndexable indexable, IProviderUpdateContext context)
        {
            throw new NotImplementedException();
        }

        public void Delete(IIndexableId id, IProviderUpdateContext context)
        {
            context.Delete(id);
        }

        public void Delete(IIndexableUniqueId indexableUniqueId, IProviderUpdateContext context)
        {
            context.Delete(indexableUniqueId);
        }

        public void Add(IIndexable indexable, IProviderUpdateContext context,
            ProviderIndexConfiguration indexConfiguration)
        {
            var doc = GetDocument(indexable, context);

            if (doc == null)
            {
                Event.RaiseEvent("indexing:excludedfromindex", new object[] { context.Index.Name, indexable.Id });
                return;
            }

            context.AddDocument(doc, (IExecutionContext)null);
        }

        #endregion

        protected virtual JObject GetDocument(IIndexable indexable, IProviderUpdateContext context)
        {
            if (InboundIndexFilterPipeline.Run(new InboundIndexFilterArgs(indexable)))
            {
                return null;
            }

            var builder = new AlgoliaDocumentBuilder(indexable, context);

            builder.AddSpecialFields();
            builder.AddItemFields();
            builder.AddComputedIndexFields();
            builder.AddProviderCustomFields();
            builder.AddBoost();

            return builder.Document;
        }
      
    }
}

