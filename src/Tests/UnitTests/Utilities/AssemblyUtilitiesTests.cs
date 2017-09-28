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
            getLogo.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void GetAssemblyTitleForAssemblyWithNoAttributes()
        {
            var assembly = Substitute.For<ICustomAttributeProvider>();
            var title = AssemblyUtilities.GetAssemblyTitle(assembly, "MyAssembly");
            title.Should().NotBeNullOrWhiteSpace();
            title.Should().Be("MyAssembly");
        }

        [TestMethod]
        public void GetAssemblyTitleForAssemblyWithOnlyCompany()
        {
            var assembly = Substitute.For<ICustomAttributeProvider>();
            assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true).Returns(new object[] { new AssemblyCompanyAttribute("MyCompany") });

            var title = AssemblyUtilities.GetAssemblyTitle(assembly, "MyAssembly");
            title.Should().Be("MyCompany MyAssembly");
        }

        [TestMethod]
        public void GetAssemblyTitleForAssemblyWithCompanyAndProductName()
        {
            var assembly = Substitute.For<ICustomAttributeProvider>();
            assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true).Returns(new object[] { new AssemblyCompanyAttribute("MyCompany") });
            assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true).Returns(new object[] { new AssemblyProductAttribute("MyCompany MyProduct") });

            var title = AssemblyUtilities.GetAssemblyTitle(assembly, "MyAssembly");
            title.Should().Be("MyCompany MyProduct");
        }
    }
}
