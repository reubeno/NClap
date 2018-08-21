using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NSubstitute;

namespace NClap.Tests.Parser
{
    [TestClass]
    public class CompletionTests
    {
        private enum SimpleEnum
        {
            Nothing,
            Something
        }

        [ArgumentSet(
            Style = ArgumentSetStyle.WindowsCommandLine,
            AllowNamedArgumentValueAsSucceedingToken = true,
            AnswerFileArgumentPrefix = "@")]
        private class SimpleArgs
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "d")]
            public string Baz { get; set; }

            [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "c")]
            public int Bar { get; set; }

            [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "s", DefaultValue = true)]
            public bool SomeFlag { get; set; }

            [PositionalArgument(ArgumentFlags.Required)]
            public bool PositionalFlag { get; set; }

            [NamedArgument(ArgumentFlags.Optional, ShortName = "senum")]
            public SimpleEnum SomeEnum { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine, AnswerFileArgumentPrefix = null)]
        private class ArgsWithNoAnswerFile
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public bool Flag { get; set; }
        }

        [TestMethod]
        public void TestThatGetCompletionsThrowsOnNullType()
        {
            Action a = () => CommandLineParser.GetCompletions(null, Array.Empty<string>(), 0);
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatGetCompletionsThrowsOnNullTokens()
        {
            Action a = () => CommandLineParser.GetCompletions(typeof(SimpleArgs), (IEnumerable<string>)null, 0);
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatGetCompletionsThrowsOnBogusIndex()
        {
            Action a = () => CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/" }, 2).ToList();
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestCanCompleteAnswerFilePaths()
        {
            var reader = Substitute.For<IFileSystemReader>();
            reader.EnumerateFileSystemEntries(Arg.Any<string>(), Arg.Any<string>()).Returns(new[]
            {
                Path.Combine(".", "FooBar"), Path.Combine(".", "Foo.txt")
            });

            var options = new CommandLineParserOptions { FileSystemReader = reader };
            var completions = CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "@" + Path.Combine(".", "Foo") }, 0, options).ToList();

            completions.Should().Equal("@" + Path.Combine(".", "FooBar"), "@" + Path.Combine(".", "Foo.txt"));
            reader.Received().EnumerateFileSystemEntries(".", "Foo*");
        }

        [TestMethod]
        public void TestCannotCompleteAnswerFilePathWithNoPrefixAvailable()
        {
            var reader = Substitute.For<IFileSystemReader>();
            reader.EnumerateFileSystemEntries(Arg.Any<string>(), Arg.Any<string>()).Returns(new[]
            {
                Path.Combine(".", "FooBar"), Path.Combine(".", "Foo.txt")
            });

            var options = new CommandLineParserOptions { FileSystemReader = reader };
            var completions = CommandLineParser.GetCompletions(typeof(ArgsWithNoAnswerFile), new[] { Path.Combine(".", "Foo") }, 0, options).ToList();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void TestCanCompleteArgNamesPrefixedWithHyphen() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "-" }, 0).ToList()
                .Should().Contain("-Bar", "-Baz", "-SomeFlag");

        [TestMethod]
        public void TestCanCompleteArgNamesPrefixedWithSlash() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/" }, 0).ToList()
                .Should().Contain("/Bar", "/Baz", "/SomeFlag");

        [TestMethod]
        public void TestCanCompleteArgNamesWithPartialPrefix() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/ba" }, 0).ToList()
                .Should().Equal("/Bar", "/Baz");

        [TestMethod]
        public void TestCanCompleteArgNameCapitalizationWithFullMatch() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/bar" }, 0).ToList()
                .Should().Equal("/Bar");

        [TestMethod]
        public void TestThatArgNameWithStrictPrefixMatchDoesNotComplete() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/bars" }, 0).ToList()
                .Should().BeEmpty();

        [TestMethod]
        public void TestCanCompleteArgNameEvenIfItHasAlreadyAppeared() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/Bar=foo", "/Ba" }, 1).ToList()
                .Should().Equal("/Bar", "/Baz");

        [TestMethod]
        public void TestThatCompleteBoolArgCompletesToItself() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag" }, 0).ToList()
                .Should().Equal("/SomeFlag");

        [TestMethod]
        public void TestThatBoolArgWithEqualsCompletesToFalseAndTrueCompletions() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag=" }, 0).ToList()
                .Should().Equal("/SomeFlag=False", "/SomeFlag=True");

        [TestMethod]
        public void TestThatBoolArgWithPrefixOfFalseCompletes() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag=f" }, 0).ToList()
                .Should().Equal("/SomeFlag=False");

        [TestMethod]
        public void TestThatBoolArgWithPlusHasNoCompletions() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag+" }, 0).ToList()
                .Should().BeEmpty();

        [TestMethod]
        public void TestThatBoolArgWithMinusHasNoCompletions() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag-" }, 0).ToList()
                .Should().BeEmpty();

        [TestMethod]
        public void TestThatUnknownArgHasNoCompletions() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/NotAFlag=f" }, 0).ToList()
                .Should().BeEmpty();

        [TestMethod]
        public void TestCanCompleteBoolPositionalArgument()
        {
            CommandLineParser.GetCompletions(typeof(SimpleArgs), Array.Empty<string>(), 0).ToList()
                .Should().Equal("False", "True");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { string.Empty }, 0).ToList()
                .Should().Equal("False", "True");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "f" }, 0).ToList()
                .Should().Equal("False");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "x" }, 0).ToList()
                .Should().BeEmpty();

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "False" }, 1).ToList()
                .Should().BeEmpty();

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag=true", "FA", "/Baz" }, 1).ToList()
                .Should().Equal("False");
        }

        [TestMethod]
        public void TestThatNamedArgumentCanGenerateCompletionsInSameOrSucceedingToken()
        {
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeEnum" }, 0).ToList()
                .Should().Equal("/SomeEnum");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeEnum:" }, 0).ToList()
                .Should().Equal("/SomeEnum:Nothing", "/SomeEnum:Something");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeEnum" }, 1).ToList()
                .Should().Equal("Nothing", "Something");
        }

        [TestMethod]
        public void TestCanGenerateCompletionWithShortName() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/sen" }, 0).ToList()
                .Should().Equal("/senum");

        [TestMethod]
        public void TestCanCompleteUnparsedCommandLine()
        {
            CommandLineParser.GetCompletions(typeof(SimpleArgs), "False /SomeFla", 14)
                .Should().Equal("/SomeFlag");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), "False '/SomeFla", 15)
                .Should().Equal("/SomeFlag");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), "False '/SomeFla'", 15)
                .Should().Equal("/SomeFlag");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), "False \"/SomeFla\"", 15)
                .Should().Equal("/SomeFlag");
        }

        [TestMethod]
        public void TestCompletionSkipsExpectedNumberOfTokens()
        {
            CommandLineParser.GetCompletions(typeof(SimpleArgs), "Fa", 0, tokensToSkip: 1, null)
                .Should().BeEmpty();

            CommandLineParser.GetCompletions(typeof(SimpleArgs), "False Fa", 6, tokensToSkip: 1, null)
                .Should().Equal("False");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), "'Some Thing' Fa", 13, tokensToSkip: 1, null)
                .Should().Equal("False");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), "\"Some Thing\" Fa", 13, tokensToSkip: 1, null)
                .Should().Equal("False");
        }
    }
}
