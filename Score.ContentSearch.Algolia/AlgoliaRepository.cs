using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Algolia.Search;
using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.Abstract;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaRepository : IAlgoliaRepository
    {
        private readonly Index _index;

        public AlgoliaRepository(IAlgoliaConfig algoliaConfig)
        {
            AlgoliaClient algoliaClient = new AlgoliaClient(algoliaConfig.ApplicationId, algoliaConfig.FullApiKey);
            _index = algoliaClient.InitIndex(algoliaConfig.IndexName);
        }

        public Task<JObject> SaveObjectsAsync(IEnumerable<JObject> objects)
        {
            if (objects == null) throw new ArgumentNullException("objects");
            return _index.SaveObjectsAsync(objects);
        }

        public Task<JObject> AddObjectAsync(object content, string objectId = null)
        {
            var result = _index.AddObjectAsync(content, objectId);
            return result;
        }

        public Task<JObject> DeleteObjectsAsync(IEnumerable<String> objects)
        {
            return _index.DeleteObjectsAsync(objects);
        }

        public Task WaitTaskAsync(string taskID)
        {
            return _index.WaitTaskAsync(taskID);
        }

        public Task<JObject> SearchAsync(Query q)
        {
            return _index.SearchAsync(q);
        }

        public Task<JObject> ClearIndexAsync()
        {
            return _index.ClearIndexAsync();
        }
    }
}
