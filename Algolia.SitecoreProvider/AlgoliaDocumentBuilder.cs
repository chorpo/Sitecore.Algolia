using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algolia.SitecoreProvider.FieldsConfiguration;
using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Extensions;
using Sitecore.Data.Items;

namespace Algolia.SitecoreProvider
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
            throw new NotImplementedException();
        }

        public override void AddField(IIndexableDataField field)
        {
            var fieldConfig =
                base.Index.Configuration.FieldMap.GetFieldConfiguration(field) as SimpleFieldsConfiguration;
            if (fieldConfig == null || !ShouldAddField(field, fieldConfig))
            {
                return;
            }

            if (field.TypeKey == "number")
                Document[field.Name] = double.Parse((string) field.Value);
            else
                Document[field.Name] = field.Value.ToString();
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
