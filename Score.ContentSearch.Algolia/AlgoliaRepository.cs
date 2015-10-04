using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algolia.Search;
using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore.Common;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaRepository : IAlgoliaRepository
    {
        private readonly Index _index;
        private int ApiChunkSize = 1000;

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

        public async Task<int> DeleteAllObjByTag(string tag)
        {
            var query = new Query();
            query.SetTagFilters(tag);
            query.SetNbHitsPerPage(ApiChunkSize);
            query.SetAttributesToRetrieve(new List<string> { "objectID" });

            int processed = 0;
            ICollection<string> hits = await GetElements(query);
            while (hits.Any())
            {
                var deletionResponse = await _index.DeleteObjectsAsync(hits);
                var taskId = (string)deletionResponse["taskID"];
                await _index.WaitTaskAsync(taskId);
                processed += hits.Count;
                hits = await GetElements(query);
            }

            return processed;
        }

        private async Task<ICollection<string>> GetElements(Query query)
        {
            var data = await _index.SearchAsync(query);
            var hits = (JArray)data["hits"];

            var objectIds = hits.Select(hit => (string)hit["objectID"]).ToList();
            return objectIds;
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
