using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Algolia.SitecoreProvider;
using Algolia.SitecoreProvider.Abstract;
using Algolia.SitecoreProvider.FieldsConfiguration;
using Moq;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.FieldReaders;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.Shell.Framework.Commands.TemplateBuilder;

namespace Algolia.SitecoreProviderTests.Builders
{
    internal class IndexBuilder
    {
        private readonly ISearchIndex _index;

        public IndexBuilder()
        {
            var algoliaRepository = new Mock<IAlgoliaRepository>();
            _index = new AlgoliaBaseIndex("index_name", algoliaRepository.Object, new NullPropertyStore());
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

        public IndexBuilder WithDefaultFieldReader(string fieldTypeName)
        {
            AddStandardFieldReader(fieldTypeName, "DefaultFieldReader");
            return this;
        }

        public IndexBuilder WithNumericFieldReader(string fieldTypeName)
        {
            var fieldTypes = fieldTypeName.Split('|');
            string readerType = "Algolia.SitecoreProvider.FieldReaders.NumberFieldReader, Algolia.SitecoreProvider";
            _index.Configuration.FieldReaders.AddFieldReaderByFieldTypeName(readerType, fieldTypes);
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
    }
}
