using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Score.ContentSearch.Algolia.Tests
{
    [TestFixture]
    public class FrameworkTests
    {
        [Test]
        public void ShouldTargetFrameworkVersion()
        {
            //Arrange
            var assembly = Assembly.GetAssembly(typeof (AlgoliaSearchIndex));

            //Act
            var version = assembly.ImageRuntimeVersion;
            var attributes = assembly.GetCustomAttributes(typeof (TargetFrameworkAttribute));
            var attribute = attributes.FirstOrDefault() as TargetFrameworkAttribute;

            //Assert
            version.Should().Be("v4.0.30319");

            var expected = "4.5";
#if (SITECORE82)
            expected = "4.5.2";
#endif
            attribute.FrameworkName.Should().Be($".NETFramework,Version=v{expected}");
        }
    }
}
