using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algolia.Search;
using Algolia.SitecoreProvider.Abstract;
using Newtonsoft.Json.Linq;

namespace Algolia.SitecoreProvider
{
    public class AlgoliaRepository : IAlgoliaRepository
    {
        private readonly Index _index;

        public AlgoliaRepository(IAlgoliaConfig algoliaConfig)
        {
            AlgoliaClient algoliaClient = new AlgoliaClient(algoliaConfig.ApplicationId, algoliaConfig.FullApiKey);
            _index = algoliaClient.InitIndex(algoliaConfig.IndexName);
        }

        public async Task<JObject> SaveObjectsAsyn(IEnumerable<JObject> objects)
        {
            if (objects == null) throw new ArgumentNullException("objects");
            return await _index.SaveObjects(objects);
        }

        public async Task<JObject> AddObjectAsync(object content, string objectId = null)
        {
            var result = await _index.AddObject(content, objectId);
            return result;
        }

        public async Task<JObject> DeleteObjectsAsync(IEnumerable<String> objects)
        {
            return await _index.DeleteObjects(objects);
        }

        public async Task WaitTaskAsync(string taskID)
        {
            await _index.WaitTask(taskID);
        }

        public async Task<JObject> SearchAsync(Query q)
        {
            return await _index.Search(q);
        }

        public async Task<JObject> ClearIndexAsync()
        {
            return await _index.ClearIndex();
        }
    }
}
