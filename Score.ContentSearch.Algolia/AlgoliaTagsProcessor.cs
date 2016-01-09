using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore.ContentSearch.Utilities;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaTagsProcessor: ITagsProcessor
    {
        public const string TagsFieldName = "_tags";

        private readonly ICollection<AlgoliaTagConfig> _tagsConfig;

        public AlgoliaTagsProcessor(ICollection<AlgoliaTagConfig> tagsConfig)
        {
            if (tagsConfig == null) 
                throw new ArgumentNullException("tagsConfig");
            _tagsConfig = tagsConfig;
        }

        #region For Sitecore Config instantiation

        public AlgoliaTagsProcessor()
        {
            _tagsConfig = new List<AlgoliaTagConfig>();
        }

        public void AddTagConfig(AlgoliaTagConfig tag)
        {
            if (tag == null) throw new ArgumentNullException("tag");
            _tagsConfig.Add(tag);
        }

        #endregion
        
        public void ProcessDocument(JObject doc)
        {
            foreach (var algoliaTagConfig in _tagsConfig)
            {
                AddTag(doc, algoliaTagConfig);
            }
        }

        private void AddTag(JObject doc, AlgoliaTagConfig tagConfig)
        {
            var fieldValue = doc[tagConfig.FieldName];

            if (fieldValue == null)
                return;

            if (tagConfig.HideField)
            {
                doc.Remove(tagConfig.FieldName);
            }

            AddTagValue(doc, tagConfig, fieldValue);
        }

        private void AddTagValue(JObject doc, AlgoliaTagConfig tagConfig, JToken fieldValue)
        {
            var tagValues = new List<string>();

            var fieldValues = fieldValue as JArray;

            if (fieldValues != null)
            {
                List<string> values = tagValues;
                fieldValues.ForEach(t => values.Add((string)t));
            }
            else
            {
                tagValues.Add((string)fieldValue);
            }

            if (!string.IsNullOrWhiteSpace(tagConfig.TagPreffix))
            {
                tagValues = tagValues.Select(t => tagConfig.TagPreffix + t).ToList();               
            }

            tagValues = tagValues.Distinct().ToList();

            var jtags = (JArray)doc[TagsFieldName] ?? new JArray();
            var tags = jtags.ToObject<string[]>();

            tags = tags.Union(tagValues).ToArray();

            doc[TagsFieldName] = new JArray(tags);
        }
    }
}
