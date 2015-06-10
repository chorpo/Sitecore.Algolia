using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Algolia.Search
{
    /// <summary>
    /// Methods bot implemented in 4.0
    /// </summary>
    public static class JObjectEx
    {
        public static JToken GetValue(this JObject obj, string propertyName, StringComparison comparison)
        {
            if (propertyName == null)
            {
                return null;
            }
            JProperty jProperty = obj.Property(propertyName);
            if (jProperty != null)
            {
                return jProperty.Value;
            }
            if (comparison != StringComparison.Ordinal)
            {
                using (IEnumerator<JToken> enumerator = obj.Properties().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        JProperty jProperty2 = (JProperty)enumerator.Current;
                        if (string.Equals(jProperty2.Name, propertyName, comparison))
                        {
                            return jProperty2.Value;
                        }
                    }
                }
            }
            return null;
        }

        public static JToken GetValue(this JObject obj, string propertyName)
        {
            return obj.GetValue(propertyName, StringComparison.Ordinal);
        }

    }
}
