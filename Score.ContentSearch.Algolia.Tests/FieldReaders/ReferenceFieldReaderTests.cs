using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Score.ContentSearch.Algolia.FieldReaders;
using Score.ContentSearch.Algolia.Tests.Builders;
using Sitecore.ContentSearch;
using Sitecore.FakeDb;

namespace Score.ContentSearch.Algolia.Tests.FieldReaders
{
    public class ReferenceFieldReaderTests
    {
        [TestCase("droplink")]
        [TestCase("droptree")]
        [TestCase("grouped droplink")]
        [TestCase("tree")]
        public void ShouldReturnReferenceName(string fieldType)
        {
            //Arrange
            using (var db = new Db { new ItemBuilder().WithReference(fieldType).AddSubItem().Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var field = item.Fields[ItemBuilder.ReferenceFieldName];
                var args = new SitecoreItemDataField(field);
                var sut = new ReferenceFieldReader();

                //Act
                var actual = sut.GetFieldValue(args);

                //Assert
                actual.Should().BeOfType<string>();
                var result = actual as string;
                result.Should().Be("subitem");
            }
        }

        [TestCase("number")]
        [TestCase("droplist")]
        public void WrongTypeShouldReturnNull(string fieldType)
        {
            //Arrange
            using (var db = new Db { new ItemBuilder().WithReference(fieldType).AddSubItem().Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var field = item.Fields[ItemBuilder.ReferenceFieldName];
                var args = new SitecoreItemDataField(field);
                var sut = new ReferenceFieldReader();

                //Act
                var actual = sut.GetFieldValue(args);

                //Assert
                actual.Should().BeNull();
            }
        }

        [Test]
        public void NoReferenceShouldNotFail()
        {
            //Arrange
            using (var db = new Db { new ItemBuilder().WithReference("droplink").Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var field = item.Fields[ItemBuilder.ReferenceFieldName];
                var args = new SitecoreItemDataField(field);
                var sut = new ReferenceFieldReader();

                //Act
                var actual = sut.GetFieldValue(args);

                //Assert
                actual.Should().BeNull();
            }
        }

        [Test]
        public void DisplayNameShouldBeUsed()
        {
            //Arrange
            using (var db = new Db { new ItemBuilder().WithReference("droplink")
                .AddSubItem().WithSubitemDisplayName("display").Build() })
            {
                var item = db.GetItem("/sitecore/content/source");
                var field = item.Fields[ItemBuilder.ReferenceFieldName];
                var args = new SitecoreItemDataField(field);
                var sut = new ReferenceFieldReader();

                //Act
                var actual = sut.GetFieldValue(args);

                //Assert
                actual.Should().BeOfType<string>();
                var result = actual as string;
                result.Should().Be("display");
            }
        }
    }
}
