using System;
using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Reflection;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaIndexOperations : IIndexOperations
    {
        private readonly ISearchIndex _index;
        private const string LogPreffix = "AlgoliaIndexOperations: ";

        public AlgoliaIndexOperations(ISearchIndex index)
        {
            if (index == null) throw new ArgumentNullException(nameof(index));
            _index = index;
        }

        #region IIndexOperations

        public void Update(IIndexable indexable, IProviderUpdateContext context,
            ProviderIndexConfiguration indexConfiguration)
        {
            if (indexable == null) throw new ArgumentNullException(nameof(indexable));
            if (context == null) throw new ArgumentNullException(nameof(context));

            CrawlingLog.Log.Debug($"{LogPreffix} {_index.Name} Update {indexable.Id}");

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
            Delete(indexable.Id, context);
        }

        public void Delete(IIndexableId id, IProviderUpdateContext context)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            CrawlingLog.Log.Debug($"{LogPreffix} {_index.Name} Delete IIndexableId {id.Value}");
            context.Delete(id);
        }

        public void Delete(IIndexableUniqueId indexableUniqueId, IProviderUpdateContext context)
        {
            CrawlingLog.Log.Debug($"{LogPreffix} {_index.Name} Delete IIndexableUniqueId {indexableUniqueId.Value}");
            context.Delete(indexableUniqueId);
        }

        public void Add(IIndexable indexable, IProviderUpdateContext context,
            ProviderIndexConfiguration indexConfiguration)
        {
            if (indexable == null) throw new ArgumentNullException(nameof(indexable));

            CrawlingLog.Log.Debug($"{LogPreffix} {_index.Name} Add {indexable.Id}");

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
            //bool flag = InboundIndexFilterPipeline.Run(context.Index.Locator.GetInstance<ICorePipeline>(), new InboundIndexFilterArgs(version));
            //if (flag)
            //{
                //this.events.RaiseEvent("indexing:excludedfromindex", new object[]
                //{
                //    this.index.Name,
                //    version.UniqueId
                //});
                //return null;
            //}
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
            var documentBuilder = CreateDocumentBuilder(indexable, context);
            AssignLenghtConstraint(_index.Configuration as ILenghtConstraint, documentBuilder as ILenghtConstraint);

            documentBuilder.AddSpecialFields();
            documentBuilder.AddItemFields();
            documentBuilder.AddComputedIndexFields();
            //Sitecore8 does not implement this
            //documentBuilder.AddProviderCustomFields();
            documentBuilder.AddBoost();

            var algoliaDocumentBuilder = documentBuilder as AlgoliaDocumentBuilder;
            algoliaDocumentBuilder?.GenerateTags();

            return documentBuilder.Document;
        }

        private AbstractDocumentBuilder<JObject> CreateDocumentBuilder(IIndexable indexable, IProviderUpdateContext context)
        {
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

            return documentBuilder;
        }

        private void AssignLenghtConstraint(ILenghtConstraint source, ILenghtConstraint destination)
        {
            if (source == null || destination == null)
                return;

            if (source.MaxFieldLength <= 0)
                return;

            destination.MaxFieldLength = source.MaxFieldLength;
        }

    }
}

