using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.FieldsConfiguration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaDocumentBuilder : AbstractDocumentBuilder<JObject>
    {
        public AlgoliaDocumentBuilder(IIndexable indexable, IProviderUpdateContext context) : base(indexable, context)
        {

        }

        #region AbstractDocumentBuilder

        public override void AddSpecialFields()
        {
            var item = (Item)(this.Indexable as SitecoreIndexableItem);
            this.AddSpecialField("objectID", item.Language.Name + "_" + item.ID.ToGuid(), false);
            this.AddSpecialField("_id", this.Indexable.Id.ToString(), false);


            //below is base.AddSpecialFields()
            //we do not call base method because we want to keep only business data in index 

            //this.AddSpecialField("_uniqueid", this.Indexable.UniqueId.Value.ToString(), false);
            //this.AddSpecialField("_datasource", (object)this.Indexable.DataSource.ToLowerInvariant(), false);
            //this.AddSpecialField("_indexname", (object)this.Index.Name.ToLowerInvariant(), false);
            IIndexableBuiltinFields indexableBuiltinFields = this.Indexable as IIndexableBuiltinFields;
            if (indexableBuiltinFields == null)
                return;
            //this.AddSpecialField("_database", (object)indexableBuiltinFields.Database, false);
            this.AddSpecialField("_language", (object)indexableBuiltinFields.Language, false);
            //this.AddSpecialField("_template", indexableBuiltinFields.TemplateId, false);
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
            //this.AddSpecialField("_templatename", (object)indexableBuiltinFields.TemplateName, false);
            //this.AddSpecialField("_created", (object)indexableBuiltinFields.CreatedDate, false);
            //this.AddSpecialField("_updated", (object)indexableBuiltinFields.UpdatedDate, false);
            //this.AddSpecialField("_path", (object)indexableBuiltinFields.Paths, false);
            //this.AddSpecialField("_content", (object)indexableBuiltinFields.Name, false);
            //this.AddSpecialField("_content", (object)indexableBuiltinFields.DisplayName, false);
            //if (this.Options.Tags == null || this.Options.Tags.Length <= 0)
            //    return;
            //this.AddSpecialField("_tags", (object)this.Options.Tags, false);
        }

        #region AddField

        public override void AddField(IIndexableDataField field)
        {
            var fieldConfig =
                base.Index.Configuration.FieldMap.GetFieldConfiguration(field) as SimpleFieldsConfiguration;
            if (fieldConfig == null || !ShouldAddField(field, fieldConfig))
            {
                return;
            }

            var reader = base.Index.Configuration.FieldReaders.GetFieldReader(field);
            var value = reader.GetFieldValue(field);

            if (value == null)
                return;

            AddField(field.Name, value);
        }

        public override void AddField(string fieldName, object fieldValue, bool append = false)
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
                fieldValue = stringValue.Trim();
            }

            Document[fieldName] = new JValue(fieldValue);
            return true;
        }

        private bool AddFieldAsDictionary(string fieldName, object fieldValue, bool append = false)
        {
            var dictionary = fieldValue as IDictionary<string, object>;

            if (dictionary == null) return false;

            foreach (var element in dictionary)
            {
                Document[element.Key] = new JValue(element.Value);
            }               
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="append"></param>
        /// <returns>true if added</returns>
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
                Document.Add(fieldName, array);
                return true;
            }

            return false;
        }

        private bool AddFieldAsJObject(string fieldName, object fieldValue, bool append = false)
        {
            //reader can return JObject for complex data 
            //builder should merge that data into document
            if (fieldValue is JObject)
            {
                var jvalue = fieldValue as JObject;

                //Available in 6.0
                //Document.Merge(jvalue);

                if (jvalue.Count == 1)
                {
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

        #endregion
        
        public override void AddBoost()
        {
            //Algolia manages boost in GUI
        }

        public override void AddComputedIndexFields()
        {
            foreach (IComputedIndexField current in base.Options.ComputedIndexFields)
            {
                object obj;
                try
                {
                    obj = current.ComputeFieldValue(base.Indexable);
                }
                catch (Exception exception)
                {
                    CrawlingLog.Log.Warn(
                        string.Format("Could not compute value for ComputedIndexField: {0} for indexable: {1}",
                            current.FieldName, base.Indexable.UniqueId), exception);
                    if (base.Settings.StopOnCrawlFieldError())
                    {
                        throw;
                    }
                    continue;
                }
                


                //Enumerables are supported in AddField and should be added as Array

                //if (obj is IEnumerable && !(obj is string))
                //{
                //    IEnumerator enumerator2 = (obj as IEnumerable).GetEnumerator();
                //    try
                //    {
                //        while (enumerator2.MoveNext())
                //        {
                //            object current2 = enumerator2.Current;

                //            this.AddField(current.FieldName, current2, false);

                //        }
                //        continue;
                //    }
                //    finally
                //    {
                //        IDisposable disposable = enumerator2 as IDisposable;
                //        if (disposable != null)
                //        {
                //            disposable.Dispose();
                //        }
                //    }
                //}

                this.AddField(current.FieldName, obj, false);
            }
        }

        public override void AddProviderCustomFields()
        {
            //so far nothing provider specific
        }

        #endregion
        
        private bool ShouldAddField(IIndexableDataField field, SimpleFieldsConfiguration config)
        {
            if (!base.Index.Configuration.IndexAllFields)
            {
                if (config == null || (config.FieldName == null && config.FieldTypeName == null))
                {
                    return false;
                }
                if (field.Value == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
