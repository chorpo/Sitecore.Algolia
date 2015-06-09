using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algolia.Search;
using Algolia.SitecoreProvider.Abstract;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Maintenance;

namespace Algolia.SitecoreProvider
{
    public class AlgoliaSearchIndexSummary : ISearchIndexSummary
    {
        private readonly IAlgoliaRepository _repository;
        private readonly IIndexPropertyStore _propertyStore;

        public AlgoliaSearchIndexSummary( 
            IAlgoliaRepository repository,
            IIndexPropertyStore propertyStore)
        {
            _repository = repository;
            _propertyStore = propertyStore;
        }

        public long NumberOfDocuments
        {
            get
            {
                IsHealthy = false;
                var response = _repository.SearchAsync(new Query("")).Result;
                var result = (long) response["nbHits"];
                //Index is Healthy if I can pull the data
                IsHealthy = true;
                return result;
            }
        }

        public bool IsHealthy { get; private set; }

        public DateTime LastUpdated
        {
            get
            {
                DateTime d;
                DateTime.TryParse(_propertyStore.Get(IndexProperties.LastUpdatedKey), out d);
                return d;
            }
            set
            {
                _propertyStore.Set(IndexProperties.LastUpdatedKey, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool IsOptimized { get; private set; }
        public bool HasDeletions { get; private set; }
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
