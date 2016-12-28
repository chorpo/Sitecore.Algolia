using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using Score.ContentSearch.Algolia.ComputedFields;
using Score.ContentSearch.Algolia.Tests.Builders;
using Sitecore.ContentSearch;
using Sitecore.Data;
using Sitecore.FakeDb;

namespace Score.ContentSearch.Algolia.Tests.ComputedFields
{
    [TestFixture]
    public class ReferenceFieldTests
    {
        [TestCase("droplink")]
        [TestCase("droptree")]
        [TestCase("grouped droplink")]
        [TestCase("tree")]
        public void ShouldReturnName(string fieldType)
        {
            //Arrange
            using (var db = new Db { new ItemBuilder().AddSubItem().WithReference(fieldType).Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);

                var sut = new ReferenceField
                {
                    FieldName = "Reference"
                };

                //Act
                var actual = (IEnumerable<string>)sut.ComputeFieldValue(indexable);

                //Assert
                actual.Should().BeEquivalentTo("subitem");
            }
        }

        [Test]
        public void ShouldReturnDisplayName()
        {
            //Arrange
            using (var db = new Db { new ItemBuilder().AddSubItem().WithReference("droplink")
                .WithSubitemDisplayName("display").Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);

                var sut = new ReferenceField
                {
                    FieldName = "Reference"
                };

                //Act
                var actual = (IEnumerable<string>)sut.ComputeFieldValue(indexable);

                //Assert
                actual.Should().BeEquivalentTo("display");
            }
        }

        [Test]
        public void ShouldReturnCustomTitle()
        {
            //Arrange
            using (var db = new Db { new ItemBuilder().AddSubItemWithField("titleField", "titleValue")
                .WithReference("droplink").Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(@"<field><target referenceFieldName=""titleField""/></field>");
                var xmlNode = xmlDoc.FirstChild;

                var sut = new ReferenceField(xmlNode)
                {
                    FieldName = "Reference"
                };

                //Act
                var actual = (IEnumerable<string>)sut.ComputeFieldValue(indexable);

                //Assert
                actual.Should().BeEquivalentTo("titleValue");
            }
        }

        [Test]
        public void BrokenLinkShouldNotFail()
        {
            //Arrange
            using (var db = new Db { new ItemBuilder()
                .WithReference(new List<ID> {ID.NewID}).Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);

                var sut = new ReferenceField
                {
                    FieldName = "Reference",
                };

                //Act
                var actual = (IEnumerable<string>)sut.ComputeFieldValue(indexable);

                //Assert
                actual.Should().BeEquivalentTo();
            }
        }


        [Test]
        public void NoValueShouldNotFail()
        {
            //Arrange
            using (var db = new Db { new ItemBuilder()
                .WithReference(new List<ID>()).Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);

                var sut = new ReferenceField
                {
                    FieldName = "Reference",
                };

                //Act
                var actual = (IEnumerable<string>)sut.ComputeFieldValue(indexable);

                //Assert
                actual.Should().BeEquivalentTo();
            }
        }

        [TestCase("Broken")]
        [TestCase("")]
        [TestCase(null)]
        public void WrongFieldNameShouldNotfail(string fieldName)
        {
            //Arrange
            using (var db = new Db { new ItemBuilder().AddSubItem()
                .WithReference("droplink").Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var indexable = new SitecoreIndexableItem(item);

                var sut = new ReferenceField
                {
                    FieldName = fieldName,
                };

                //Act
                var actual = sut.ComputeFieldValue(indexable);

                //Assert
                actual.Should().Be("");
            }
        }
    }
}
