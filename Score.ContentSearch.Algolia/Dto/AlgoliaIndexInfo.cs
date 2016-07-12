using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Score.ContentSearch.Algolia.Dto
{
    /// <summary>
    /// Dto for https://www.algolia.com/doc/rest#list-indexes
    /// </summary>
    public class AlgoliaIndexInfo
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long Entries { get; set; }
        public bool PendingTask { get; set; }
        public int LastBuildTimeS { get; set; }
        public long DataSize { get; set; }


        public static AlgoliaIndexInfo LoadFromJson(JObject data, string indexName)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var indexInfo = (from info in data["items"]
                where string.Equals((string)info["name"], indexName, StringComparison.InvariantCultureIgnoreCase)
                select info).FirstOrDefault();

            if (indexInfo == null)
                return new AlgoliaIndexInfo();

            DateTime createdAt = (DateTime)indexInfo["createdAt"];
            DateTime updatedAt = (DateTime)indexInfo["updatedAt"];
            return new AlgoliaIndexInfo
            {
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                Entries = (long)indexInfo["entries"],
                PendingTask = (bool)indexInfo["pendingTask"],
                LastBuildTimeS = (int)indexInfo["lastBuildTimeS"],
                DataSize = (long)indexInfo["dataSize"],
            };
        }
    }
}
