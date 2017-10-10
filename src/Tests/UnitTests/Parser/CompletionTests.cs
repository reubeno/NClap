using System;
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
        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
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
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine, AnswerFileArgumentPrefix = null)]
        private class ArgsWithNoAnswerFile
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public bool Flag { get; set; }
        }

        [TestMethod]
        public void CompleteBogusIndex()
        {
            Action a = () => CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/" }, 2).ToList();
            a.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void CompleteAnswerFilePath()
        {
            var reader = Substitute.For<IFileSystemReader>();
            reader.EnumerateFileSystemEntries(Arg.Any<string>(), Arg.Any<string>()).Returns(new[]
            {
                Path.Combine(".", "FooBar"), Path.Combine(".", "Foo.txt")
            });

            var options = new CommandLineParserOptions { FileSystemReader = reader };
            var completions = CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "@" + Path.Combine(".", "Foo") }, 0, options).ToList();

            completions.Should().ContainInOrder("@" + Path.Combine(".", "FooBar"), "@" + Path.Combine(".", "Foo.txt"));
            reader.Received().EnumerateFileSystemEntries(".", "Foo*");
        }

        [TestMethod]
        public void CompleteAnswerFilePathButNoPrefixAvailable()
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
        public void CanCompleteArgNamesPrefixedWithHyphen() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "-" }, 0).ToList()
                .Should().ContainInOrder("-Bar", "-Baz", "-SomeFlag");

        [TestMethod]
        public void CanCompleteArgNamesPrefixedWithSlash() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/" }, 0).ToList()
                .Should().ContainInOrder("/Bar", "/Baz", "/SomeFlag");

        [TestMethod]
        public void CanCompleteArgNamesWithPartialPrefix() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/ba" }, 0).ToList()
                .Should().ContainInOrder("/Bar", "/Baz");

        [TestMethod]
        public void CanCompleteArgNameCapitalizationWithFullMatch() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/bar" }, 0).ToList()
                .Should().ContainInOrder("/Bar");

        [TestMethod]
        public void ArgNameWithStrictPrefixMatchDoesNotComplete() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/bars" }, 0).ToList()
                .Should().BeEmpty();

        [TestMethod]
        public void CanCompleteArgNameEvenIfItHasAlreadyAppeared() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/Bar", "/Ba" }, 1).ToList()
                .Should().ContainInOrder("/Bar", "/Baz");

        [TestMethod]
        public void CompleteBoolArgCompletesToItself() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag" }, 0).ToList()
                .Should().ContainInOrder("/SomeFlag");

        [TestMethod]
        public void BoolArgWithEqualsCompletesToFalseAndTrueCompletions() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag=" }, 0).ToList()
                .Should().ContainInOrder("/SomeFlag=False", "/SomeFlag=True");

        [TestMethod]
        public void BoolArgWithPrefixOfFalseCompletes() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag=f" }, 0).ToList()
                .Should().ContainInOrder("/SomeFlag=False");

        [TestMethod]
        public void BoolArgWithPlusHasNoCompletions() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag+" }, 0).ToList()
                .Should().BeEmpty();

        [TestMethod]
        public void BoolArgWithMinusHasNoCompletions() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag-" }, 0).ToList()
                .Should().BeEmpty();

        [TestMethod]
        public void UnknownArgHasNoCompletions() =>
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/NotAFlag=f" }, 0).ToList()
                .Should().BeEmpty();

        [TestMethod]
        public void CompleteBoolPositionalArgument()
        {
            CommandLineParser.GetCompletions(typeof(SimpleArgs), Array.Empty<string>(), 0).ToList()
                .Should().ContainInOrder("False", "True");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { string.Empty }, 0).ToList()
                .Should().ContainInOrder("False", "True");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "f" }, 0).ToList()
                .Should().ContainInOrder("False");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "x" }, 0).ToList()
                .Should().BeEmpty();

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "False" }, 1).ToList()
                .Should().BeEmpty();

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag=true", "FA", "/Baz" }, 1).ToList()
                .Should().ContainInOrder("False");
        }
    }
}
