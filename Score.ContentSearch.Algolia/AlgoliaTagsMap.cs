using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.Abstract;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaTagsMap: ITagsMap
    {
        public void ProcessDocument(JObject doc)
        {
            AddTag(doc, "_id");
        }

        private void AddTag(JObject doc, string fieldName)
        {
            var fieldValue = doc[fieldName];

            if (fieldName == null)
                return;

            var tags = (JArray)doc["_tags"];
            if (tags == null)
                tags = new JArray();

            tags.Add(fieldValue);

            doc["_tags"] = tags;
        }
    }
}
