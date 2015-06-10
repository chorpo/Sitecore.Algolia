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

        public async Task<JObject> SaveObjectsAsync(IEnumerable<JObject> objects)
        {
            if (objects == null) throw new ArgumentNullException("objects");
            return await _index.SaveObjectsAsync(objects);
        }

        public async Task<JObject> AddObjectAsync(object content, string objectId = null)
        {
            var result = await _index.AddObjectAsync(content, objectId);
            return result;
        }

        public async Task<JObject> DeleteObjectsAsync(IEnumerable<String> objects)
        {
            return await _index.DeleteObjectsAsync(objects);
        }

        public async Task WaitTaskAsync(string taskID)
        {
            await _index.WaitTaskAsync(taskID);
        }

        public async Task<JObject> SearchAsync(Query q)
        {
            return await _index.SearchAsync(q);
        }

        public async Task<JObject> ClearIndexAsync()
        {
            return await _index.ClearIndexAsync();
        }
    }
}
