using System.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.FieldReaders;
using Sitecore.Data.Fields;

namespace Algolia.SitecoreProvider.FieldReaders
{
    public class NumberFieldReader: FieldReader
    {
            public override object GetFieldValue(IIndexableDataField indexableField)
            {
                if (!(indexableField is SitecoreItemDataField))
                {
                    return indexableField.Value;
                }
                Field field = indexableField as SitecoreItemDataField;
                if (!string.IsNullOrEmpty(field.Value))
                {
                    string text = field.Value;

                    int intResult;

                    if (int.TryParse(text, out intResult))
                    {
                        return intResult;
                    }

                    if (text.Count((char x) => x == ',') == 1)
                    {
                        text = text.Replace(',', '.');
                    }
                    double num;
                    if (double.TryParse(text, System.Globalization.NumberStyles.Float, 
                        System.Globalization.CultureInfo.InvariantCulture, out num))
                    {
                        return num;
                    }
                }
                return 0;
            }
    }
}
