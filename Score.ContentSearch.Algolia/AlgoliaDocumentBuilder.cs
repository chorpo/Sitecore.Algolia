using System;
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
        }

        #region AbstractDocumentBuilder

        public override void AddField(string fieldName, object fieldValue, bool append = false)
        {
            //reader can return JObject for complex data 
            //builder should merge that data into document
            if (fieldValue is JObject)
            {
                var jvalue = fieldValue as JObject;
                Document.Merge(jvalue);
                return;
            }
            
            //otherwise - add new field (for simple types data)

            var stringValue = fieldValue as string;

            if (stringValue != null)
            {
                if (string.IsNullOrWhiteSpace(stringValue))
                    return;
                fieldValue = stringValue.Trim();
            }

            Document[fieldName] = new JValue(fieldValue);
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
