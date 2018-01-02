using System;
using System.Reflection;
using NClap.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using NSubstitute;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class AssemblyUtilitiesTests
    {
        [TestMethod]
        public void GetLogoWorks()
        {
            var logo = AssemblyUtilities.GetLogo();
            logo.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public void GetLogoThrowsOnInvalidArgument()
        {
            Action getLogo = () => AssemblyUtilities.GetLogo(null);
            getLogo.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetAssemblyTitleForAssemblyWithNoAttributes()
        {
            var assembly = GetFakeAssembly();
            var title = assembly.GetAssemblyTitle("MyAssembly");
            title.Should().NotBeNullOrWhiteSpace();
            title.Should().Be("MyAssembly");
        }

        [TestMethod]
        public void GetAssemblyTitleForAssemblyWithOnlyCompany()
        {
            var assembly = GetFakeAssembly(company: "MyCompany");

            var title = assembly.GetAssemblyTitle("MyAssembly");
            title.Should().Be("MyCompany MyAssembly");
        }

        [TestMethod]
        public void GetAssemblyTitleForAssemblyWithCompanyAndProductName()
        {
            var assembly = GetFakeAssembly(company: "MyCompany", product: "MyProduct");

            var title = assembly.GetAssemblyTitle("MyAssembly");
            title.Should().Be("MyCompany MyProduct");
        }

        [TestMethod]
        public void GetAssemblyTitleForAssemblyWithRedundantCompanyAndProductName()
        {
            var assembly = GetFakeAssembly(company: "MyCompany", product: "MyCompany MyProduct");

            var title = assembly.GetAssemblyTitle("MyAssembly");
            title.Should().Be("MyCompany MyProduct");
        }

        private ICustomAttributeProvider GetFakeAssembly(string company = null, string product = null, string title = null)
        {
            var assembly = Substitute.For<ICustomAttributeProvider>();

            if (company != null)
            {
                assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true).Returns(new object[] { new AssemblyCompanyAttribute(company) });
            }

            if (product != null)
            {
                assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true).Returns(new object[] { new AssemblyProductAttribute(product) });
            }

            if (title != null)
            {
                assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true).Returns(new object[] { new AssemblyTitleAttribute(title) });
            }

            return assembly;
        }
    }
}
