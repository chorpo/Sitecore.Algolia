using System;
using System.Collections.Generic;
using System.Globalization;
using Algolia.Search;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.Diagnostics;
using Newtonsoft.Json;
using Score.ContentSearch.Algolia.Dto;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaSearchIndexSummary : ISearchIndexSummary
    {
        private readonly IAlgoliaRepository _repository;
        private readonly IIndexPropertyStore _propertyStore;

        public AlgoliaSearchIndexSummary(
            IAlgoliaRepository repository,
            IIndexPropertyStore propertyStore)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (propertyStore == null) throw new ArgumentNullException(nameof(propertyStore));

            _repository = repository;
            _propertyStore = propertyStore;
        }

        public long NumberOfDocuments
        {
            get
            {
                IsHealthy = false;
                var info = _repository.GetIndexInfo();
                var result = info.Entries;
                //Index is Healthy if I can pull the data
                IsHealthy = !info.PendingTask;
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

        public bool IsOptimized => true;
        public bool HasDeletions { get; private set; }
        public int NumberOfFields { get; private set; }
        public long NumberOfTerms => -1L;
        public bool IsClean { get; private set; }
        public string Directory { get; private set; }
        public bool IsMissingSegment => false;
        public int NumberOfBadSegments => 0;
        public bool OutOfDateIndex { get; private set; }
        public IDictionary<string, string> UserData { get; private set; }
        public long? LastUpdatedTimestamp { get; set; }

#if SITECORE81
        private IIndexableInfo lastIndexedEntry;

        public IIndexableInfo LastIndexedEntry
        {
            get
            {
                this.lastIndexedEntry = (JsonConvert.DeserializeObject<IndexableInfo>(_propertyStore.Get(IndexProperties.LastIndexedEntry)) ?? new IndexableInfo());
                return this.lastIndexedEntry;
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.lastIndexedEntry = value;
                _propertyStore.Set(IndexProperties.LastIndexedEntry, JsonConvert.SerializeObject(this.lastIndexedEntry));
            }
        }
#endif

    }
}
