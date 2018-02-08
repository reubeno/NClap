using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Help;

namespace NClap.Tests.Help
{
    [TestClass]
    public class LogoFactoryTests
    {
        [TestMethod]
        public void TestThatConstructorThrowsOnNullAssembly()
        {
            Action a = () => new LogoFactory(null);
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatTitleIsRetrievableWhenAttributePresent() =>
            TestThatVariableIsRetrievableWhenAttributeIsPresent("Title", (a, value) => a.Title = value);

        [TestMethod]
        public void TestThatVersionIsRetrievableWhenAttributePresent() =>
            TestThatVariableIsRetrievableWhenAttributeIsPresent("Version", (a, value) => a.Version = value);

        [TestMethod]
        public void TestThatCompanyIsRetrievableWhenAttributePresent() =>
            TestThatVariableIsRetrievableWhenAttributeIsPresent("Company", (a, value) => a.Company = value);

        [TestMethod]
        public void TestThatConfigurationIsRetrievableWhenAttributePresent() =>
            TestThatVariableIsRetrievableWhenAttributeIsPresent("Configuration", (a, value) => a.Configuration = value);

        [TestMethod]
        public void TestThatCopyrightIsRetrievableWhenAttributePresent() =>
            TestThatVariableIsRetrievableWhenAttributeIsPresent("Copyright", (a, value) => a.Copyright = value);

        [TestMethod]
        public void TestThatCultureIsRetrievableWhenAttributePresent() =>
            TestThatVariableIsRetrievableWhenAttributeIsPresent("Culture", (a, value) => a.Culture = value);

        [TestMethod]
        public void TestThatDescriptionIsRetrievableWhenAttributePresent() =>
            TestThatVariableIsRetrievableWhenAttributeIsPresent("Description", (a, value) => a.Description = value);

        [TestMethod]
        public void TestThatFileVersionIsRetrievableWhenAttributePresent() =>
            TestThatVariableIsRetrievableWhenAttributeIsPresent("FileVersion", (a, value) => a.FileVersion = value);
        [TestMethod]
        public void TestThatInformationalVersionIsRetrievableWhenAttributePresent() =>
            TestThatVariableIsRetrievableWhenAttributeIsPresent("InformationalVersion", (a, value) => a.InformationalVersion = value);

        [TestMethod]
        public void TestThatProductIsRetrievableWhenAttributePresent() =>
            TestThatVariableIsRetrievableWhenAttributeIsPresent("Product", (a, value) => a.Product = value);

        [TestMethod]
        public void TestThatTrademarkIsRetrievableWhenAttributePresent() =>
            TestThatVariableIsRetrievableWhenAttributeIsPresent("Trademark", (a, value) => a.Trademark = value);

        [TestMethod]
        public void TestThatTitleIsRetrievableEvenWhenAttributeIsNotPresent()
        {
            const string anyString = "SomeMetadata";
            var assembly = new FakeAssembly
            {
                Name = anyString
            };

            var factory = new LogoFactory(assembly);
            factory.TryGetVariable("Title", out string value).Should().BeTrue();
            value.Should().Be(anyString);
        }

        [TestMethod]
        public void TestThatVersionIsRetrievableEvenWhenAttributeIsNotPresent()
        {
            var assembly = new FakeAssembly
            {
                Location = Assembly.GetExecutingAssembly().Location
            };

            var factory = new LogoFactory(assembly);
            factory.TryGetVariable("Version", out string value).Should().BeTrue();
            value.Should().MatchRegex(@"^\d+\.\d+\.\d+\.\d+$");
        }

        [TestMethod]
        public void TestThatUnknownVariableIsNotRetrievable()
        {
            var assembly = new FakeAssembly();
            var factory = new LogoFactory(assembly);

            factory.TryGetVariable("UNIMPLEMENTED", out string value).Should().BeFalse();
            value.Should().BeNull();
        }

        [TestMethod]
        public void TestThatCopyrightSymbolIsReplaced()
        {
            const string anyStringContainingCopyrightSymbol = "Something © Else";
            var assembly = new FakeAssembly
            {
                Copyright = anyStringContainingCopyrightSymbol
            };

            var factory = new LogoFactory(assembly);
            factory.TryGetVariable("Copyright", out string value).Should().BeTrue();
            value.Should().Be("Something (C) Else");
        }

        private void TestThatVariableIsRetrievableWhenAttributeIsPresent(string variableName, Action<FakeAssembly, string> setterFunc)
        {
            const string anyString = "SomeMetadata";

            var assembly = new FakeAssembly();
            setterFunc(assembly, anyString);

            var factory = new LogoFactory(assembly);
            factory.TryGetVariable(variableName, out string actualValue).Should().BeTrue();
            actualValue.Should().Be(anyString);
        }
    }
}
