using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Algolia.Search;
using Newtonsoft.Json.Linq;

namespace Algolia.SitecoreProvider.Abstract
{
    public interface IAlgoliaRepository
    {
        Task<JObject> SaveObjectsAsyn(IEnumerable<JObject> objects);
        Task<JObject> AddObjectAsync(object content, string objectId = null);
        Task<JObject> DeleteObjectsAsync(IEnumerable<String> objects);
        Task WaitTaskAsync(string taskID);
        Task<JObject> SearchAsync(Query q);
    }
}