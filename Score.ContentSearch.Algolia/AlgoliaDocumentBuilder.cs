using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using Score.ContentSearch.Algolia.FieldsConfiguration;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;

namespace Score.ContentSearch.Algolia
{
    public class AlgoliaDocumentBuilder : AbstractDocumentBuilder<JObject>
    {
        public AlgoliaDocumentBuilder(IIndexable indexable, IProviderUpdateContext context) : base(indexable, context)
        {
            var item = (Item)(indexable as SitecoreIndexableItem);
            Document["name"] = item.Name;
            Document["path"] = item.Paths.Path;
            Document["objectID"] = item.ID.ToGuid().ToString();
            Document["language"] = item.Language.Name;
        }

        #region AbstractDocumentBuilder

        public override void AddField(string fieldName, object fieldValue, bool append = false)
        {
            //reader can return JObject for complex data 
            //builder should merge that data into document
            if (AddAsJObject(fieldName, fieldValue, append))
                return;

            //collections to be added as Array
            if (AddAsEnumarable(fieldName, fieldValue, append))
                return;

            //otherwise - add new field (for simple types data)
            AddAsPlainField(fieldName, fieldValue, append);
        }

        private bool AddAsPlainField(string fieldName, object fieldValue, bool append = false)
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="append"></param>
        /// <returns>true if added</returns>
        private bool AddAsEnumarable(string fieldName, object fieldValue, bool append = false)
        {
            if (fieldValue is string)
                return false;

            var enumerable = fieldValue as IEnumerable;

            if (enumerable != null)
            {
                var array = new JArray(enumerable);
                Document.Add(fieldName, array);
                return true;
            }

            return false;
        }

        private bool AddAsJObject(string fieldName, object fieldValue, bool append = false)
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

        public override void AddBoost()
        {
            throw new NotImplementedException();
        }

        public override void AddComputedIndexFields()
        {
            throw new NotImplementedException();
        }

        public override void AddProviderCustomFields()
        {
            throw new NotImplementedException();
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
