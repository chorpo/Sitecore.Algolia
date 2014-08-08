using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Algolia.SitecoreProvider
{
    public class AlgoliaItemSerializer
    {
        public JObject SerializeExpandoObject(dynamic data)
        {
            var dict = (IDictionary<string, object>)data;
            for (int i = dict.Keys.Count - 1; i >= 0; i--)
            {
                var key = dict.Keys.ElementAt(i);
                if (dict[key] == null)
                {
                    ((IDictionary<String, Object>)data).Remove(key);
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
