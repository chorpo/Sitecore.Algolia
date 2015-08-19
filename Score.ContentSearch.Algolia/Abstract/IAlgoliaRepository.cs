using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Score.ContentSearch.Algolia.Abstract
{
    public interface IAlgoliaRepository
    {
        Task<JObject> SaveObjectsAsync(IEnumerable<JObject> objects);
        Task<JObject> AddObjectAsync(object content, string objectId = null);
        Task<JObject> DeleteObjectsAsync(IEnumerable<String> objects);
        Task WaitTaskAsync(string taskID);
        Task<JObject> SearchAsync(global::Algolia.Search.Query q);
        Task<JObject> ClearIndexAsync();
    }
}