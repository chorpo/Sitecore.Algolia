using Newtonsoft.Json.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.FieldReaders;
using Sitecore.Data.Fields;

namespace Score.ContentSearch.Algolia.FieldReaders
{
    public class GeoLocationFieldReader : FieldReader
    {
        public override object GetFieldValue(IIndexableDataField indexableField)
        {
            Field field = indexableField as SitecoreItemDataField;
            if (string.IsNullOrWhiteSpace(field?.Value))
                return null;

            var values = field.Value.Split(',');

            if (values.Length < 2)
                return null;

            double lat, lng;

            if (double.TryParse(values[0], out lat)
                && double.TryParse(values[1], out lng))
            {
                var location = new JObject
                {
                    ["lat"] = lat,
                    ["lng"] = lng
                };

                var result = new JObject {["_geoloc"] = location};

                return result;
            }

            return null;
        }
    }
}
