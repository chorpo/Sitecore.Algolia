using System;
using System.Collections.Generic;
using System.Globalization;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.Diagnostics;
using Newtonsoft.Json;

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
                if (_propertyStore == null)
                {
                    return DateTime.MinValue;
                }

                var isoDate = _propertyStore.Get(IndexProperties.LastUpdatedKey);

                if (isoDate.Length <= 0)
                {
                    return DateUtil.IsoDateToDateTime(isoDate, DateTime.MinValue);
                }

                return DateUtil.IsoDateToDateTime(isoDate, DateTime.MinValue, true);
            }
            set
            {
                _propertyStore?.Set(IndexProperties.LastUpdatedKey, DateUtil.ToIsoDate(value, true, true));
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

        public long? LastUpdatedTimestamp
        {
            get
            {
                var s = _propertyStore.Get(IndexProperties.LastUpdatedTimestamp);
                if (string.IsNullOrEmpty(s))
                {
                    return new long?();
                }
                return long.Parse(s, CultureInfo.InvariantCulture);
            }
            set
            {
                var str = value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                _propertyStore.Set(IndexProperties.LastUpdatedTimestamp, str);
            }
        }


        private IndexableInfo lastIndexedEntry;

        public IndexableInfo LastIndexedEntry
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

    }
}
