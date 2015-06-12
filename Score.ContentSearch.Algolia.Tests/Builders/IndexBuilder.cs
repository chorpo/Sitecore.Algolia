using System.Collections.Generic;
using System.Xml;
using Moq;
using Score.ContentSearch.Algolia.Abstract;
using Score.ContentSearch.Algolia.ComputedFields;
using Score.ContentSearch.Algolia.FieldsConfiguration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.FieldReaders;
using Sitecore.ContentSearch.Maintenance;

namespace Score.ContentSearch.Algolia.Tests.Builders
{
    internal class IndexBuilder
    {
        private readonly ISearchIndex _index;

        public IndexBuilder()
        {
            var algoliaRepository = new Mock<IAlgoliaRepository>();
            _index = new AlgoliaBaseIndex("index_name", algoliaRepository.Object);
            var configuration = new ProviderIndexConfiguration
            {
                DocumentOptions = new DocumentBuilderOptions(),
                FieldMap = new FieldMap(),
                FieldReaders = new FieldReaderMap(),
            };
            _index.Configuration = configuration;
        }

        public IndexBuilder WithSimpleFieldTypeMap(string typeKey)
        {
            var fieldConfig = new SimpleFieldsConfiguration(string.Empty, string.Empty, typeKey,
                new Dictionary<string, string>(), new XmlDocument());
            var fieldMap = _index.Configuration.FieldMap as FieldMap;

            fieldMap.Add(fieldConfig);
            return this;
        }

        public IndexBuilder WithParentsComputedField(string fieldName)
        {
            var field = new ParentsField {FieldName = fieldName};
            _index.Configuration.DocumentOptions.ComputedIndexFields.Add(field);
            return this;
        }

        public IndexBuilder WithDefaultFieldReader(string fieldTypeName)
        {
            AddStandardFieldReader(fieldTypeName, "DefaultFieldReader");
            return this;
        }

        public IndexBuilder WithNumericFieldReader(string fieldTypeName)
        {
            AddCustomFieldReader(fieldTypeName, "NumberFieldReader");
            return this;
        }

        public IndexBuilder WithDateFieldReader(string fieldTypeName)
        {
            AddCustomFieldReader(fieldTypeName, "DateFieldReader");
            return this;
        }

        public ISearchIndex Build()
        {
            return _index;
        }

        private void AddStandardFieldReader(string fieldTypeName, string fieldReaderType)
        {
            var fieldTypes = fieldTypeName.Split('|');
            string readerType = string.Format("Sitecore.ContentSearch.FieldReaders.{0}, Sitecore.ContentSearch",
                fieldReaderType);
            _index.Configuration.FieldReaders.AddFieldReaderByFieldTypeName(readerType, fieldTypes);
        }

        private void AddCustomFieldReader(string fieldTypeName, string fieldReaderType)
        {
            var fieldTypes = fieldTypeName.Split('|');
            string readerType = string.Format("Score.ContentSearch.Algolia.FieldReaders.{0}, Score.ContentSearch.Algolia",
                fieldReaderType);
            _index.Configuration.FieldReaders.AddFieldReaderByFieldTypeName(readerType, fieldTypes);
        }
    }
}
