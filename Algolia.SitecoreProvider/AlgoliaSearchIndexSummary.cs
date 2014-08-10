using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.ContentSearch;

namespace Algolia.SitecoreProvider
{
    public class AlgoliaSearchIndexSummary : ISearchIndexSummary
    {
        public long NumberOfDocuments { get; private set; }
        public bool IsOptimized { get; private set; }
        public bool HasDeletions { get; private set; }
        public bool IsHealthy { get; private set; }
        public DateTime LastUpdated { get; set; }
        public int NumberOfFields { get; private set; }
        public long NumberOfTerms { get; private set; }
        public bool IsClean { get; private set; }
        public string Directory { get; private set; }
        public bool IsMissingSegment { get; private set; }
        public int NumberOfBadSegments { get; private set; }
        public bool OutOfDateIndex { get; private set; }
        public IDictionary<string, string> UserData { get; private set; }
        public long? LastUpdatedTimestamp { get; set; }
    }
}
