using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.Data.Items;

namespace Algolia.SitecoreProvider
{
    public class IndexOperations : IIndexOperations
    {
        public void Update(IIndexable indexable, IProviderUpdateContext context,
            ProviderIndexConfiguration indexConfiguration)
        {
            var doc = GetDocument(indexable);
            context.UpdateDocument(doc, null, (IExecutionContext) null);
        }

        public void Delete(IIndexable indexable, IProviderUpdateContext context)
        {
            throw new NotImplementedException();
        }

        public void Delete(IIndexableId id, IProviderUpdateContext context)
        {
            throw new NotImplementedException();
        }

        public void Delete(IIndexableUniqueId indexableUniqueId, IProviderUpdateContext context)
        {
            throw new NotImplementedException();
        }

        public void Add(IIndexable indexable, IProviderUpdateContext context,
            ProviderIndexConfiguration indexConfiguration)
        {
            throw new NotImplementedException();
        }

        protected virtual JObject GetDocument(IIndexable indexable)
        {
            var item = (Item) (indexable as SitecoreIndexableItem);

            //var tags = new List<string>();
            //tags.Add("image");

            //if (!string.IsNullOrEmpty(item.GpId))
            //    tags.Add("gpId_" + item.GpId);

            //tags.Add("owner_" + item.UserId);
            //tags.Add("tripId_" + item.TripId);
            //foreach (var connectionId in item.UsersCanSee)
            //{
            //    tags.Add("connection_" + connectionId);
            //}

            dynamic w = new ExpandoObject();

            w.name = item.Name;
            w.path = item.Paths.Path;

            return SerializeExpandoObject(w);
        }

        private static JObject SerializeExpandoObject(dynamic data)
        {
            var dict = (IDictionary<string, object>) data;
            for (int i = dict.Keys.Count - 1; i >= 0; i--)
            {
                var key = dict.Keys.ElementAt(i);
                if (dict[key] == null)
                {
                    ((IDictionary<String, Object>) data).Remove(key);
                }
            }
            var str = JsonConvert.SerializeObject(data,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    StringEscapeHandling = StringEscapeHandling.Default,
                });
            var jobj = JObject.Parse(str);
            return jobj;
        }
    }
}

