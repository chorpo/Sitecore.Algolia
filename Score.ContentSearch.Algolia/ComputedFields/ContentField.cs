using System.Collections.Generic;
using System.Linq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Search.Crawlers.FieldCrawlers;

namespace Score.ContentSearch.Algolia.ComputedFields
{
    /// <summary>
    /// Collects Data From All Rich Text fields in Page Components
    /// </summary>
    public class ContentField : IComputedIndexField
    {
        private readonly string[] _textFieldTypes = { "rich text" };

        public object ComputeFieldValue(IIndexable indexable)
        {
            var item = (SitecoreIndexableItem)indexable;

            var result = new List<string>();

            var database = item.Item.Database;

            // Get all renderings
            var renderings = item.Item.Visualization.GetRenderings(DeviceItem.ResolveDevice(database), false);
            foreach (var reference in renderings)
            {
                // Get the source item
                if (reference.RenderingItem == null)
                    continue;

                var datasource = reference.Settings.DataSource;

                if (string.IsNullOrEmpty(datasource))
                    continue;

                var sourceId = ID.Parse(datasource);

                var source = database.GetItem(sourceId, item.Item.Language);

                if (source == null)
                    continue;

                // Go through all fields
                foreach (Field field in source.Fields)
                {
                    var value = GetFieldValue(field);

                    if (!string.IsNullOrEmpty(value))
                        result.Add(value);
                }
            }

            if (result.Any())
                return result;

            return null;
        }

        public string FieldName { get; set; }
        public string ReturnType { get; set; }

        private string GetFieldValue(Field field)
        {
            if (!IsTextField(field))
                return string.Empty;

            FieldCrawlerBase fieldCrawler = FieldCrawlerFactory.GetFieldCrawler(field);
            return fieldCrawler != null ? fieldCrawler.GetValue() : string.Empty;
        }

        #region Sitecore.Search.Crawlers.DatabaseCrawler

        /// <summary>
        ///     Determines whether [is text field] [the specified field].
        /// </summary>
        /// <param name="field">
        ///     The field.
        /// </param>
        /// <returns>
        ///     <c>true</c> if [is text field] [the specified field]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsTextField(Field field)
        {
            Assert.ArgumentNotNull(field, "field");
            if (!_textFieldTypes.Contains(field.TypeKey))
            {
                return false;
            }
            TemplateField templateField = field.GetTemplateField();
            return templateField == null || !templateField.ExcludeFromTextSearch;
        }

        #endregion

    }
}
