﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.Abstract;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.Data.Items;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaDocumentBuilder : AbstractDocumentBuilder<JObject>, IIndexCustomOptions
    {
        private readonly ITagsProcessor _tagsProcessor;

        public AlgoliaDocumentBuilder(IIndexable indexable, IProviderUpdateContext context) : base(indexable, context)
        {
            var config = context.Index.Configuration as AlgoliaIndexConfiguration;

            if (config != null)
            {
                _tagsProcessor = config.TagsProcessor;
            }
        }

        protected override void AddSpecialFields()
        {
            var item = (Item)(this.Indexable as SitecoreIndexableItem);
            this.AddSpecialField("objectID", item.Language.Name + "_" + item.ID.ToGuid(), false);
            this.AddSpecialField("_id", this.Indexable.Id.ToString(), false);


            //lines below are copied from base.AddSpecialFields()
            //we do not call base method because we want to keep only business data in index 

            //this.AddSpecialField("_uniqueid", this.Indexable.UniqueId.Value.ToString(), false);
            //this.AddSpecialField("_datasource", (object)this.Indexable.DataSource.ToLowerInvariant(), false);
            //this.AddSpecialField("_indexname", (object)this.Index.Name.ToLowerInvariant(), false);
            IIndexableBuiltinFields indexableBuiltinFields = this.Indexable as IIndexableBuiltinFields;
            if (indexableBuiltinFields == null)
                return;
            //this.AddSpecialField("_database", (object)indexableBuiltinFields.Database, false);
            this.AddSpecialField("_language", (object)indexableBuiltinFields.Language, false);

            if (IncludeTemplateId)
            {
                this.AddSpecialField("_template", indexableBuiltinFields.TemplateId.ToString(), false);
            }
            //this.AddSpecialField("_parent", indexableBuiltinFields.Parent, false);
            //if (indexableBuiltinFields.IsLatestVersion)
            //    this.AddSpecialField("_latestversion", (object)true, false);
            //this.AddSpecialField("_version", (object)indexableBuiltinFields.Version, false);
            //this.AddSpecialField("_group", indexableBuiltinFields.Group, false);
            //if (indexableBuiltinFields.IsClone)
            //    this.AddSpecialField("_isclone", (object)true, false);
            this.AddSpecialField("_fullpath", (object)indexableBuiltinFields.FullPath, false);
            if (this.Options.ExcludeAllSpecialFields)
                return;
            this.AddSpecialField("_name", (object)indexableBuiltinFields.Name, false);
            //this.AddSpecialField("_displayname", (object)indexableBuiltinFields.DisplayName, false);
            //this.AddSpecialField("_creator", (object)indexableBuiltinFields.CreatedBy, false);
            //this.AddSpecialField("_editor", (object)indexableBuiltinFields.UpdatedBy, false);
            this.AddSpecialField("_templatename", (object)indexableBuiltinFields.TemplateName, false);
            //this.AddSpecialField("_created", (object)indexableBuiltinFields.CreatedDate, false);
            //this.AddSpecialField("_updated", (object)indexableBuiltinFields.UpdatedDate, false);
            //this.AddSpecialField("_path", (object)indexableBuiltinFields.Paths, false);
            //this.AddSpecialField("_content", (object)indexableBuiltinFields.Name, false);
            //this.AddSpecialField("_content", (object)indexableBuiltinFields.DisplayName, false);
            //if (this.Options.Tags == null || this.Options.Tags.Length <= 0)
            //    return;
            this.AddField("_tags", new List<string> { "id_" + this.Indexable.Id });
        }

        protected override void AddField(IIndexableDataField field)
        {
            if (!ShouldAddField(field))
            {
                return;
            }

            var reader = base.Index.Configuration.FieldReaders.GetFieldReader(field);
            var value = reader.GetFieldValue(field);

            if (value == null)
                return;

            AddField(field.Name, value);
        }

        protected override void AddField(string fieldName, object fieldValue, bool append = false)
        {
            //Empty values should be skipped
            if (AddFieldAsEmpty(fieldName, fieldValue, append))
                return;

            //reader can return JObject for complex data 
            //builder should merge that data into document
            if (AddFieldAsJObject(fieldName, fieldValue, append))
                return;

            //add every element of dictionary as field
            if (AddFieldAsDictionary(fieldName, fieldValue, append))
                return;

            //collections to be added as Array
            if (AddFieldAsEnumarable(fieldName, fieldValue, append))
                return;

            //otherwise - add new field (for simple types data)
            AddFieldAsPlainField(fieldName, fieldValue, append);
        }

        private bool AddFieldAsEmpty(string fieldName, object fieldValue, bool append = false)
        {
            if (fieldValue == null)
                return true;

            return false;
        }

        private bool AddFieldAsPlainField(string fieldName, object fieldValue, bool append = false)
        {
            var stringValue = fieldValue as string;

            if (stringValue != null)
            {
                if (string.IsNullOrWhiteSpace(stringValue))
                    //not added but next processor should be skipped
                    return true;
                stringValue = stringValue.Trim();

                if (MaxFieldLength > 0 && stringValue.Length > MaxFieldLength)
                {
                    stringValue = stringValue.Substring(0, MaxFieldLength) + "...";
                    CrawlingLog.Log.Debug(
                        $"Cut field value for {Indexable.Id}.{fieldName} to {MaxFieldLength} characters");
                }

                fieldValue = stringValue;
            }

            Document[fieldName] = new JValue(fieldValue);
            return true;
        }

        private bool AddFieldAsDictionary(string fieldName, object fieldValue, bool append = false)
        {
            var dictionary = fieldValue as IDictionary;

            if (dictionary == null) return false;

            foreach (DictionaryEntry element in dictionary)
            {
                AddField(element.Key.ToString(), element.Value);
            }
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="append"></param>
        /// <returns>true if value processed and no extra work required</returns>
        private bool AddFieldAsEnumarable(string fieldName, object fieldValue, bool append = false)
        {
            if (fieldValue is string)
                return false;

            var enumerable = fieldValue as IEnumerable;

            if (enumerable != null)
            {
                var array = new JArray(enumerable);
                if (!array.Any())
                    return true;

                if (Document.Property(fieldName) != null)
                {
                    CrawlingLog.Log.Error($"Skipped duplicated field '{fieldName}' in document '{Indexable.Id}'");
                    return true;
                }
                Document.Add(fieldName, array);
                return true;
            }

            return false;
        }

        private bool AddFieldAsJObject(string fieldName, object fieldValue, bool append = false)
        {
            //reader can return JObject for complex data 
            //builder should merge that data into document
            var jvalue = fieldValue as JObject;
            if (jvalue != null)
            {
                //Available in 6.0 but cannot be used with Sitecore 7
                //Document.Merge(jvalue);

                if (jvalue.Count == 1)
                {
                    var property = jvalue.First as JProperty;
                    if (property != null &&  Document.Property(property.Name) != null)
                    {
                        CrawlingLog.Log.Error($"Skipped duplicated property '{property.Name}' in document '{Indexable.Id}'");
                        return true;
                    }

                    Document.Add(jvalue.First);
                }
                else
                {
                    Document.Add(jvalue);
                }
                return true;
            }
            return false;
        }

        protected override void AddBoost()
        {
            //Algolia manages boost in GUI
        }

        protected override void AddComputedIndexField(IComputedIndexField computedIndexField, object fieldValue)
        {
            foreach (IComputedIndexField current in Options.ComputedIndexFields)
            {
                var siteSpecificField = current as ISiteSpecificField;
                if (siteSpecificField != null)
                {
                    var algoliaIndex = Index as AlgoliaSearchIndex;

                    if (algoliaIndex != null)
                    {
                        siteSpecificField.Site = algoliaIndex.Site;
                    }
                }

                object computedFieldValue;
                try
                {
                    computedFieldValue = current.ComputeFieldValue(Indexable);
                }
                catch (Exception exception)
                {
                    CrawlingLog.Log.Warn(
                        $"Could not compute value for ComputedIndexField: {current.FieldName} for indexable: {base.Indexable.UniqueId}",
                        exception);
                    if (base.Settings.StopOnCrawlFieldError())
                    {
                        throw;
                    }
                    continue;
                }

                this.AddField(current.FieldName, computedFieldValue, false);
            }
        }

        public virtual void GenerateTags()
        {
            if (_tagsProcessor == null)
                return;

            _tagsProcessor.ProcessDocument(Document);
        }

        private bool ShouldAddField(IIndexableDataField field)
        {
            if (!base.Index.Configuration.DocumentOptions.IndexAllFields)
            {
                if (field.Value == null)
                {
                    return false;
                }
            }
            return true;
        }

        public int MaxFieldLength { get; set; }
        public bool IncludeTemplateId { get; set; }
    }
}
