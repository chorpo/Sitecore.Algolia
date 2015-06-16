using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.Abstract;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaTagsProcessor: ITagsProcessor
    {
        public const string TagsFieldName = "_tags";

        private readonly IEnumerable<AlgoliaTagConfig> _tagsConfig;

        public AlgoliaTagsProcessor(IEnumerable<AlgoliaTagConfig> tagsConfig)
        {
            if (tagsConfig == null) 
                throw new ArgumentNullException("tagsConfig");
            _tagsConfig = tagsConfig;
        }


        public void ProcessDocument(JObject doc)
        {
            foreach (var algoliaTagConfig in _tagsConfig)
            {
                AddTag(doc, algoliaTagConfig);
            }
        }

        private void AddTag(JObject doc, AlgoliaTagConfig tagConfig)
        {
            var fieldValue = (string)doc[tagConfig.FieldName];

            if (fieldValue == null)
                return;

            if (tagConfig.HideField)
            {
                doc.Remove(tagConfig.FieldName);
            }

            if (!string.IsNullOrWhiteSpace(tagConfig.TagPreffix))
            {
                fieldValue = tagConfig.TagPreffix + fieldValue;
            }

            var tags = (JArray)doc[TagsFieldName] ?? new JArray();

            tags.Add(fieldValue);

            doc[TagsFieldName] = tags;
        }
    }
}
