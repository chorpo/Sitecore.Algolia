using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algolia.SitecoreProvider.Abstract;
using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;

namespace Algolia.SitecoreProvider
{
    public class ProviderUpdateContext: IProviderUpdateContext
    {
        private readonly ISearchIndex _index;
        private readonly IAlgoliaRepository _repository;
        private Dictionary<string, JObject> _updateDocs;
        
        public ProviderUpdateContext(
            ISearchIndex index,
            IAlgoliaRepository repository)
        {
            _index = index;
            _repository = repository;
            _updateDocs = new Dictionary<string, JObject>();
        }

        #region IProviderUpdateContext

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            foreach (var item in _updateDocs)
            {
                _repository.AddObjectAsync(item.Value, item.Key).Wait();
            }
            _updateDocs.Clear();
        }

        public void Optimize()
        {
            //No need to optimize because items are stored in dictionary
        }

        public void AddDocument(object itemToAdd, IExecutionContext executionContext)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Delete(IIndexableId id)
        {
            throw new NotImplementedException();
        }

        public bool IsParallel { get; private set; }
        public ParallelOptions ParallelOptions { get; private set; }

        public ISearchIndex Index
        {
            get { return _index; }
        }

        public ICommitPolicyExecutor CommitPolicyExecutor { get; private set; }

        #endregion


        private static string GetItemId(JObject item)
        {
            var id = (string)item["id"];

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


    }
}
