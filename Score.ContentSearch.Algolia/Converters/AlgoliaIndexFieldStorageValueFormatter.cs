using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Converters;
using Sitecore.Diagnostics;
using System;
using System.ComponentModel;
using System.Xml;

namespace Score.ContentSearch.Algolia.Converters
{
    public class AlgoliaIndexFieldStorageValueFormatter : IndexFieldStorageValueFormatter, ISearchIndexInitializable
    {
        private ISearchIndex _searchIndex;

        public AlgoliaIndexFieldStorageValueFormatter()
        {
            this.EnumerableConverter = (TypeConverter)new IndexFieldEnumerableConverter((IndexFieldStorageValueFormatter)this);
        }

        public override object FormatValueForIndexStorage(object value, string fieldName)
        {
            Assert.IsNotNullOrEmpty(fieldName, nameof(fieldName));

            throw new NotImplementedException();
        }

        public override object ReadFromIndexStorage(object indexValue, string fieldName, Type destinationType)
        {
            if (indexValue == null)
                return (object)null;
            if (destinationType == (Type)null)
                throw new ArgumentNullException(nameof(destinationType));
            if (indexValue.GetType() == destinationType)
                return indexValue;

            throw new NotImplementedException();
        }

        public override void AddConverter(XmlNode configNode)
        {
            throw new NotImplementedException();
        }

        public new void Initialize(ISearchIndex searchIndex)
        {
            this._searchIndex = searchIndex;
            base.Initialize(searchIndex);
        }

        private static bool TryConvertToPrimitiveType(object value, Type expectedType, out object result)
        {
            throw new NotImplementedException();
        }

        private TypeConverter GetConverter(Type type)
        {
            throw new NotImplementedException();
        }

        private object ConvertToType(object value, Type expectedType, ITypeDescriptorContext context)
        {
            throw new NotImplementedException();
        }

        private bool TryConvertToEnumerable(object value, Type expectedType, ITypeDescriptorContext context, out object result)
        {
            throw new NotImplementedException();
        }

        private object ReadFromIndexStorageBase(object indexValue, string fieldName, Type destinationType)
        {
            if (indexValue == null)
                return (object)null;
            if (destinationType == (Type)null)
                throw new ArgumentNullException(nameof(destinationType));
            if (destinationType.IsInstanceOfType(indexValue))
                return indexValue;

            throw new NotImplementedException();
        }
    }
}
