using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.Data;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaUpdateContext: IProviderUpdateContext
    {
        private readonly ISearchIndex _index;
        private readonly IAlgoliaRepository _repository;

        /// <summary>
        /// Items that needs to be Updated in Index
        /// </summary>
        private readonly Dictionary<string, JObject> _updateDocs;

        /// <summary>
        /// Items that needs to be deleted in Index
        /// </summary>
        private readonly List<ID> _deleteIds;

        public AlgoliaUpdateContext(
            ISearchIndex index,
            IAlgoliaRepository repository)
        {
            if (index == null) throw new ArgumentNullException("index");
            if (repository == null) throw new ArgumentNullException("repository");

            _index = index;
            _repository = repository;

            _updateDocs = new Dictionary<string, JObject>();
            _deleteIds = new List<ID>();
        }

        #region IProviderUpdateContext

        public void Dispose()
        {
            
        }

        public void Commit()
        {
            _repository.SaveObjectsAsyn(_updateDocs.Select(t => t.Value)).Wait();
            _updateDocs.Clear();

            _repository.DeleteObjectsAsync(_deleteIds.Select(t => t.ToGuid().ToString())).Wait();
            _deleteIds.Clear();
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
            var doc = itemToUpdate as JObject;

            if (doc == null)
                throw new Exception("Context only can save JObjects");

            var id = GetItemId(doc);

            KeepDocForIndexUpdate(id, doc);
        }

        public void UpdateDocument(object itemToUpdate, object criteriaForUpdate,
            params IExecutionContext[] executionContexts)
        {
            throw new NotImplementedException();
        }

        public void Delete(IIndexableUniqueId id)
        {
            _deleteIds.Add(id.Value as ID);
        }

        public void Delete(IIndexableId id)
        {
            _deleteIds.Add(id.Value as ID);
        }

        public bool IsParallel { get; private set; }
        public ParallelOptions ParallelOptions { get; private set; }

        public ISearchIndex Index
        {
            get { return _index; }
        }

        public ICommitPolicyExecutor CommitPolicyExecutor { get; private set; }

        #endregion


        #region Helpers

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

        #endregion


    }
}
