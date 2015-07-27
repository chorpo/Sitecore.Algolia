using System;
using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Abstractions;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Pipelines.IndexingFilters;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Reflection;

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
            var doc = BuildDataToIndex(context, indexable);

            if (doc == null)
            {
                Event.RaiseEvent("indexing:excludedfromindex", new object[] {context.Index.Name, indexable.Id});
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
            var doc = BuildDataToIndex(context, indexable);

            if (doc == null)
            {
                Event.RaiseEvent("indexing:excludedfromindex", new object[] {context.Index.Name, indexable.Id});
                return;
            }

            context.AddDocument(doc, (IExecutionContext) null);
        }

        #endregion


        private JObject BuildDataToIndex(IProviderUpdateContext context, IIndexable version)
        {
            bool flag = InboundIndexFilterPipeline.Run(context.Index.Locator.GetInstance<ICorePipeline>(), new InboundIndexFilterArgs(version));
            if (flag)
            {
                //this.events.RaiseEvent("indexing:excludedfromindex", new object[]
                //{
                //    this.index.Name,
                //    version.UniqueId
                //});
                return null;
            }
            var indexData = this.GetIndexData(version, context);
            //if (indexData.IsEmpty)
            //{
            //    CrawlingLog.Log.Warn(string.Format("CrawlerLuceneIndexOperations : IndexVersion produced a NULL doc for version {0}. Skipping.", version.UniqueId), null);
            //    return null;
            //}
            return indexData;
        }



        internal JObject GetIndexData(IIndexable indexable, IProviderUpdateContext context)
        {
            Assert.ArgumentNotNull(indexable, "indexable");
            Assert.ArgumentNotNull(context, "context");

            var options = _index.Configuration.DocumentOptions;
            Assert.Required(options, "IDocumentBuilderOptions of wrong type for this crawler");
            if (indexable.Id.ToString() == string.Empty)
            {
                return new JObject();
            }
            var documentBuilder =
                (AbstractDocumentBuilder<JObject>)
                    ReflectionUtil.CreateObject(context.Index.Configuration.DocumentBuilderType, new object[]
                    {
                        indexable,
                        context
                    });
            if (documentBuilder == null)
            {
                CrawlingLog.Log.Error(
                    "Unable to create document builder (" + context.Index.Configuration.DocumentBuilderType +
                    "). Please check your configuration. We will fallback to the default for now.", null);
                documentBuilder = new AlgoliaDocumentBuilder(indexable, context);
            }

            documentBuilder.AddSpecialFields();
            documentBuilder.AddItemFields();
            documentBuilder.AddComputedIndexFields();
            //Sitecore8 does not implement this
            //documentBuilder.AddProviderCustomFields();
            documentBuilder.AddBoost();

            var algoliaDocumentBuilder = documentBuilder as AlgoliaDocumentBuilder;
            if (algoliaDocumentBuilder != null)
            {
                algoliaDocumentBuilder.GenerateTags();
            }
            
            return documentBuilder.Document;
        }

    }
}

