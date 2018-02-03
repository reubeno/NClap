using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Tests.Utilities;

namespace NClap.Tests
{
    [TestClass]
    public class CommandLineParserOptionsTests
    {
        [TestMethod]
        public void TestThatDefaultOptionsCloneCorrectly()
        {
            var options = new CommandLineParserOptions();
            DeepCloneTests.CloneShouldYieldADistinctButEquivalentObject(options);
        }

        [TestMethod]
        public void TestThatRequiredPropertiesArePresentInDefaultOptions()
        {
            var options = new CommandLineParserOptions();
            options.HelpOptions.Should().NotBeNull();
            options.Reporter.Should().NotBeNull();
            options.FileSystemReader.Should().NotBeNull();
        }

        [TestMethod]
        public void TestThatRequiredPropertiesArePresentInQuietOptions()
        {
            var options = CommandLineParserOptions.Quiet();
            options.HelpOptions.Should().NotBeNull();
            options.Reporter.Should().NotBeNull();
            options.FileSystemReader.Should().NotBeNull();
        }
    }
}
