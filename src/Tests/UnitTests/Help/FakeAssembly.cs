using System.Reflection;
using NSubstitute;

namespace NClap.Tests.Help
{
    internal class FakeAssembly
    {
        internal class FakeAssemblyImpl : Assembly
        {
        }

        public static implicit operator Assembly(FakeAssembly fake) => fake.Object;

        public Assembly Object { get; } = Substitute.For<FakeAssemblyImpl>();

        public string Name
        {
            set => Object.GetName().Returns(new AssemblyName(value));
        }

        public string Location
        {
            set => Object.Location.Returns(value);
        }

        public string Company
        {
            set => Object.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true).Returns(new object[] { new AssemblyCompanyAttribute(value) });
        }

        public string Configuration
        {
            set => Object.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), true).Returns(new object[] { new AssemblyConfigurationAttribute(value) });
        }

        public string Copyright
        {
            set => Object.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true).Returns(new object[] { new AssemblyCopyrightAttribute(value) });
        }

        public string Culture
        {
            set => Object.GetCustomAttributes(typeof(AssemblyCultureAttribute), true).Returns(new object[] { new AssemblyCultureAttribute(value) });
        }

        public string Description
        {
            set => Object.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true).Returns(new object[] { new AssemblyDescriptionAttribute(value) });
        }

        public string FileVersion
        {
            set => Object.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true).Returns(new object[] { new AssemblyFileVersionAttribute(value) });
        }

        public string InformationalVersion
        {
            set => Object.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true).Returns(new object[] { new AssemblyInformationalVersionAttribute(value) });
        }

        public string Product
        {
            set => Object.GetCustomAttributes(typeof(AssemblyProductAttribute), true).Returns(new object[] { new AssemblyProductAttribute(value) });
        }

        public string Title
        {
            set => Object.GetCustomAttributes(typeof(AssemblyTitleAttribute), true).Returns(new object[] { new AssemblyTitleAttribute(value) });
        }

        public string Trademark
        {
            set => Object.GetCustomAttributes(typeof(AssemblyTrademarkAttribute), true).Returns(new object[] { new AssemblyTrademarkAttribute(value) });
        }

        public string Version
        {
            set => Object.GetCustomAttributes(typeof(AssemblyVersionAttribute), true).Returns(new object[] { new AssemblyVersionAttribute(value) });
        }
    }
}
