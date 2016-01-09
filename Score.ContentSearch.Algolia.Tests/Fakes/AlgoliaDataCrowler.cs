using System.Threading;
using Sitecore.ContentSearch;
using Sitecore.Diagnostics;

namespace Score.ContentSearch.Algolia.Tests.Fakes
{
    public class AlgoliaDataCrowler: SitecoreItemCrawler
    {
        public override void RebuildFromRoot(IProviderUpdateContext context, IndexingOptions indexingOptions, CancellationToken cancellationToken)
        {
            Assert.ArgumentNotNull((object)context, "context");
            if (!this.ShouldStartIndexing(indexingOptions))
                return;
            var indexableRoot = this.GetIndexableRoot();
            Assert.IsNotNull((object)indexableRoot, "RebuildFromRoot: Unable to retrieve root item");
            Assert.IsNotNull((object)this.DocumentOptions, "DocumentOptions");
            //context.Index.Locator.GetInstance<IEvent>()
            //    .RaiseEvent("indexing:addingrecursive", (object)context.Index.Name, (object)indexableRoot.UniqueId, 
            //    (object)indexableRoot.AbsolutePath);
            AddHierarchicalRecursive(indexableRoot, context, this.index.Configuration, cancellationToken);
            //context.Index.Locator.GetInstance<IEvent>()
            //    .RaiseEvent("indexing:addedrecursive", (object)context.Index.Name, (object)indexableRoot.UniqueId, 
            //    (object)indexableRoot.AbsolutePath);
        }
    }
}
