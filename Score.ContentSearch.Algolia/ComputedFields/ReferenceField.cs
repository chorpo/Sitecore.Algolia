using System.Collections.Generic;
using System.Xml;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Score.ContentSearch.Algolia.ComputedFields
{
    public class ReferenceField : IComputedIndexField
    {
 
        public ReferenceField()
            : this((XmlNode) null)
        {
        }

        public ReferenceField(XmlNode configurationNode)
        {
            Initialize(configurationNode);
        }

        private void Initialize(XmlNode configurationNode)
        {
            if (configurationNode?.ChildNodes.Count > 0)
            {
                var targetNode = configurationNode.SelectSingleNode("target");
                if (targetNode?.Attributes?["referenceFieldName"] != null)
                {
                    ReferenceFieldName = targetNode.Attributes["referenceFieldName"].Value;
                }
            }
        }

        public object ComputeFieldValue(IIndexable indexable)
        {
            var item = (SitecoreIndexableItem) indexable;

            if (item?.Item == null)
            {
                CrawlingLog.Log.Error("ReferenceField:  indexable is not provided");
                return string.Empty;
            }


            if (string.IsNullOrEmpty(FieldName))
            {
                CrawlingLog.Log.Error("ReferenceField: FieldName is not provided");
                return string.Empty;
            }

            MultilistField field = item.Item.Fields[FieldName];
            if (field == null)
            {
                CrawlingLog.Log.Debug($"ReferenceField: cannot find filed '{FieldName}'");
                return string.Empty;
            }

            var values = new List<string>();

            foreach (Item reference in field.GetItems())
            {
                string value = null;
                if (!string.IsNullOrEmpty(ReferenceFieldName))
                {
                    value = reference[ReferenceFieldName];
                }

                if (string.IsNullOrEmpty(value))
                {
                    value = reference.DisplayName;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    values.Add(value);
                }
            }

            return values;
        }

        public string FieldName { get; set; }
        public string ReturnType { get; set; }

        public string ReferenceFieldName { get; private set; }
    }

}
