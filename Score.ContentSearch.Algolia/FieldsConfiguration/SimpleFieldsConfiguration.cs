using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.ContentSearch;

namespace Score.ContentSearch.Algolia.FieldsConfiguration
{
    /// <summary>
    /// Configuration for text and numeric fields that does not require any extra processing
    /// fields like Integer, Single-Line Text, Multi-Line Text
    /// </summary>
    public class SimpleFieldsConfiguration : AbstractSearchFieldConfiguration
    {
        public SimpleFieldsConfiguration()
        {
            
        }

        public SimpleFieldsConfiguration(
            string name, string fieldID, string fieldTypeName, 
            IDictionary<string, string> attributes, XmlNode configNode
            )
            : base(name, fieldID, fieldTypeName, attributes, configNode)
        {
            this.FieldName = name;
            this.FieldTypeName = fieldTypeName;
        }
    }
}
