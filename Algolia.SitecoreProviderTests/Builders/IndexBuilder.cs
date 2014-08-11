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
                FieldMap = new FieldMap()
            };
            _index.Configuration = configuration;
        }

        public IndexBuilder WithSimpleFieldTypeMap(string typeKey)
        {
            var fieldConfig = new SimpleFieldsConfiguration(string.Empty, string.Empty, typeKey,
                new Dictionary<string, string>(), new XmlDocument());
            //string xmlContent = "<foo></foo>";
            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml(xmlContent);
            //XmlNode newNode = doc.DocumentElement;

            var fieldMap = _index.Configuration.FieldMap as FieldMap;

            fieldMap.Add(fieldConfig);
            return this;
        }
        
        public ISearchIndex Build()
        {
            return _index;
        }
    }
}
