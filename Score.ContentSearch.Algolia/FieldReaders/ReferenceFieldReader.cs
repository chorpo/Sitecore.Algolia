using Sitecore;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.FieldReaders;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Score.ContentSearch.Algolia.FieldReaders
{
    public class ReferenceFieldReader : FieldReader
    {
        public override object GetFieldValue(IIndexableDataField indexableField)
        {
            Field field = (indexableField as SitecoreItemDataField);

            var strongTypeField = FieldTypeManager.GetField(field);

            if (strongTypeField is LookupField)
                return GetRefernceValue(new LookupField(field).TargetItem);
            if (strongTypeField is ReferenceField)
                return GetRefernceValue(new ReferenceField(field).TargetItem);

            return null;
        }

        private string GetRefernceValue(Item target)
        {
            if (target == null)
                return null;

            string str = target[FieldIDs.DisplayName];
            if (str.Length > 0)
                return str;
            return target.Name;
        }
    }
}
