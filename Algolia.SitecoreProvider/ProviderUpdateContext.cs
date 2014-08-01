using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;

namespace Algolia.SitecoreProvider
{
    public class ProviderUpdateContext: IProviderUpdateContext
    {
        private readonly ISearchIndex _index;
        private List<JObject> _updateDocs;
        
        public ProviderUpdateContext(ISearchIndex index)
        {
            _index = index;
            _updateDocs = new List<JObject>();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Optimize()
        {
            throw new NotImplementedException();
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
            _updateDocs.Add(doc);
        }

        public void UpdateDocument(object itemToUpdate, object criteriaForUpdate, params IExecutionContext[] executionContexts)
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
    }
}
