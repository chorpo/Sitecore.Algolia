using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.Abstract;
using Score.ContentSearch.Algolia.Extensions;
using Sitecore.Abstractions;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Sharding;
using Sitecore.Data;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaUpdateContext: IProviderUpdateContext, ITrackingIndexingContext
    {
        private readonly ISearchIndex _index;
        private readonly IAlgoliaRepository _repository;
        private readonly IEvent _events;
        private volatile bool _isDisposed;
        private volatile bool _isDisposing;

        /// <summary>
        /// Items that needs to be Updated in Index
        /// </summary>
        private readonly Dictionary<string, JObject> _updateDocs;

        /// <summary>
        /// Items that needs to be deleted in Index
        /// </summary>
        private readonly List<ID> _deleteIds;

        public ConcurrentDictionary<IIndexableUniqueId, object> Processed
        {
            get; set;
        }

        public AlgoliaUpdateContext(
            ISearchIndex index,
            IAlgoliaRepository repository)
        {
            if (index == null) throw new ArgumentNullException(nameof(index));
            if (repository == null) throw new ArgumentNullException(nameof(repository));

            _index = index;
            _repository = repository;

            _updateDocs = new Dictionary<string, JObject>();
            _deleteIds = new List<ID>();
            Processed = new ConcurrentDictionary<IIndexableUniqueId, object>();
            CommitPolicyExecutor = new NullCommitPolicyExecutor();
            _events = _index.Locator.GetInstance<IEvent>();
        }

        #region IProviderUpdateContext

        public void Dispose()
        {
            if (_isDisposed)
                return;
            lock (this)
            {
                _isDisposing = true;
                _isDisposed = true;
            }
            CrawlingLog.Log.Debug($"Algolia: Disposed");
        }

        public void Commit()
        {
            CrawlingLog.Log.Debug("Starting Algolia Commit");
            var data = _updateDocs.Select(t => t.Value).ToList();
            CrawlingLog.Log.Debug($"Have {data.Count} documents to to Update");
            _events.RaiseEvent("indexing:committing", _index.Name);
            var chunks = data.ChunkBy(100).ToList();
            CrawlingLog.Log.Debug($"Documents split into {chunks.Count} Chunks");
            chunks.ForEach(chunk => _repository.SaveObjectsAsync(chunk).Wait());
            _updateDocs.Clear();

            var stringsToDelete = _deleteIds.Select(t => t.ToString()).ToList();

            foreach (var id in stringsToDelete)
            {
                _repository.DeleteAllObjByTag("id_" + id).Wait();
            }
            _deleteIds.Clear();
            CommitPolicyExecutor.Committed();
            _events.RaiseEvent("indexing:committed", _index.Name);
            CrawlingLog.Log.Info($"Update Context Committed: {data.Count} updated, {stringsToDelete.Count} deleted");
        }

        public void Optimize()
        {
            //No need to optimize because items are stored in dictionary
        }

        public void AddDocument(object itemToAdd, IExecutionContext executionContext)
        {
            UpdateDocument(itemToAdd, null, executionContext);
        }

        public void AddDocument(object itemToAdd, params IExecutionContext[] executionContexts)
        {
            throw new NotImplementedException();
        }

        public void UpdateDocument(object itemToUpdate, object criteriaForUpdate, IExecutionContext executionContext)
        {
            if (_isDisposed || _isDisposing)
            {
                CrawlingLog.Log.Debug($"Algolia: cannot update doc, disposed");
                return;
            }

            var doc = itemToUpdate as JObject;

            if (doc == null)
            {
                CrawlingLog.Log.Debug($"Algolia: doc is null, not adding");
                throw new Exception("Context only can save JObjects");
            }

            var id = GetItemId(doc);

            CrawlingLog.Log.Debug($"Algolia: Keeping doc {doc}");
            KeepDocForIndexUpdate(id, doc);

            var job = Sitecore.Context.Job;
            if (job == null)
            {
                return;
            }
            ++job.Status.Processed;
            CommitPolicyExecutor.IndexModified(this, itemToUpdate, IndexOperation.Update);
        }

        public void UpdateDocument(object itemToUpdate, object criteriaForUpdate,
            params IExecutionContext[] executionContexts)
        {
            throw new NotImplementedException();
        }

        public void Delete(IIndexableUniqueId id)
        {
            _deleteIds.Add(id.Value as ID);
            CommitPolicyExecutor.IndexModified(this, id, IndexOperation.Delete);
        }

        public void Delete(IIndexableId id)
        {
            _deleteIds.Add(id.Value as ID);
            CommitPolicyExecutor.IndexModified(this, id, IndexOperation.Delete);
        }

        public bool IsParallel { get; private set; }
        public ParallelOptions ParallelOptions { get; private set; }

        public ISearchIndex Index => _index;

        public ICommitPolicyExecutor CommitPolicyExecutor { get; private set; }

        public IEnumerable<Shard> ShardsWithPendingChanges { get; private set; }

        private static string GetItemId(JObject item)
        {
            var id = (string) item["objectID"];

            if (string.IsNullOrEmpty(id))
                throw new Exception("Cannot load id field");

            return id;
        }

        private void KeepDocForIndexUpdate(string id, JObject item)
        {
            if (_updateDocs.ContainsKey(id))
                _updateDocs[id] = item;
            else
            {
                _updateDocs.Add(id, item);
            }
        }

        public void Delete(IIndexableUniqueId id, params IExecutionContext[] executionContexts)
        {
            throw new NotImplementedException();
        }

        public void Delete(IIndexableId id, params IExecutionContext[] executionContexts)
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}
