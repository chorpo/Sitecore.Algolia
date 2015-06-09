using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;

namespace Score.ContentSearch.Algolia.Tests.Configuration
{
    [TestFixture]
    public class AlgoliaSearchIndexTests
    {
        [Test]
        public void ShouldCreateFromConfig()
        {
            //Arrange
            string xmlPath = @"Configuration\Algolia.Search.config";
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(xmlPath);
            //var firstElement = (System.Xml.XmlElement)xmlDoc.DocumentElement.FirstChild;

            XmlElement root = xmlDoc.DocumentElement;
            var configNode = root.SelectSingleNode("//configuration//sitecore//contentSearch//configuration//indexes");

            configNode.Should().NotBeNull();

            //Act
            var index = Sitecore.Configuration.Factory.CreateObject<AlgoliaSearchIndex>(configNode);


            //Assert
        }
    }
}
