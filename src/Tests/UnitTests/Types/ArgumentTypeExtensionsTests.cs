using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Types;

namespace NClap.Tests.Types
{
    [TestClass]
    public class ArgumentTypeExtensionsTests
    {
        class TestType : IStringParser, IObjectFormatter, IStringCompleter
        {
            public bool TryParse(ArgumentParseContext context, string stringToParse, out object value)
            {
                if (!stringToParse.StartsWith("|") || !stringToParse.EndsWith("|") || stringToParse.Length < 2)
                {
                    value = null;
                    return false;
                }

                value = stringToParse.Substring(1, stringToParse.Length - 2);
                return true;
            }

            public string Format(object value) => $"|{value}|";

            public IEnumerable<string> GetCompletions(ArgumentCompletionContext context, string valueToComplete) =>
                new[] { "|a|", "|ABC|" };
        }

        class TestArguments
        {
            [PositionalArgument(ArgumentFlags.AtMostOnce, Parser = typeof(TestType), Formatter = typeof(TestType),
                Completer = typeof(TestType))]
            public string Value;
        }

        [TestMethod]
        public void CustomParser()
        {
            CommandLineParser.TryParse(new[] { "|foo|" }, out TestArguments args).Should().BeTrue();
            args.Value.Should().Be("foo");
        }

        [TestMethod]
        public void CustomFormatter()
        {
            var args = new TestArguments { Value = "foo" };
            CommandLineParser.Format(args).Should().Equal("|foo|");
        }

        [TestMethod]
        public void CustomCompleter()
        {
            var completions = CommandLineParser.GetCompletions(typeof(TestArguments), new[] { "|a|" }, 0);
            completions.Should().Equal("|a|", "|ABC|");
        }
    }
}
