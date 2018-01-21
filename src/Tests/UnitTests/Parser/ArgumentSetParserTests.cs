using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Parser;

namespace NClap.Tests.Parser
{
    [TestClass]
    public class ArgumentSetParserTests
    {
        [TestMethod]
        public void TestThatConstructorThrowsOnNullArgumentSet()
        {
            Action a = () => new ArgumentSetParser(null, new CommandLineParserOptions());
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatConstructorAllowNullOptions()
        {
            var argSet = new ArgumentSetDefinition();
            Action a = () => new ArgumentSetParser(argSet, null);
            a.Should().NotThrow();
        }

        [TestMethod]
        public void TestThatConstructorAllowsOptionsWithNullProperties()
        {
            var argSet = new ArgumentSetDefinition();
            var options = new CommandLineParserOptions
            {
                Context = null,
                FileSystemReader = null,
                HelpOptions = null,
                Reporter = null
            };

            Action a = () => new ArgumentSetParser(argSet, options);
            a.Should().NotThrow();
        }
    }
}
