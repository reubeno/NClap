using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Parser;
using NSubstitute;

namespace NClap.Tests.Parser
{
    [TestClass]
    public class CompletionTests
    {
        private class SimpleArgs
        {
            // ReSharper disable once UnusedMember.Local
            [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "d")]
            public string Baz { get; set; }

            // ReSharper disable once UnusedMember.Local
            [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "c")]
            public int Bar { get; set; }

            // ReSharper disable once UnusedMember.Local
            [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "s", DefaultValue = true)]
            public bool SomeFlag { get; set; }

            // ReSharper disable once UnusedMember.Local
            [PositionalArgument(ArgumentFlags.Required)]
            public bool PositionalFlag { get; set; }
        }

        [ArgumentSet(AnswerFileArgumentPrefix = null)]
        private class ArgsWithNoAnswerFile
        {
            // ReSharper disable once UnusedMember.Local
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
            reader.EnumerateFileSystemEntries(Arg.Any<string>(), Arg.Any<string>()).Returns(new[] { ".\\FooBar", ".\\Foo.txt" });

            var options = new CommandLineParserOptions { FileSystemReader = reader };
            var completions = CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "@.\\Foo" }, 0, options).ToList();

            completions.Should().ContainInOrder("@.\\FooBar", "@.\\Foo.txt");
            reader.Received().EnumerateFileSystemEntries(".", "Foo*");
        }

        [TestMethod]
        public void CompleteAnswerFilePathButNoPrefixAvailable()
        {
            var reader = Substitute.For<IFileSystemReader>();
            reader.EnumerateFileSystemEntries(Arg.Any<string>(), Arg.Any<string>()).Returns(new[] { ".\\FooBar", ".\\Foo.txt" });

            var options = new CommandLineParserOptions { FileSystemReader = reader };
            var completions = CommandLineParser.GetCompletions(typeof(ArgsWithNoAnswerFile), new[] { "@.\\Foo" }, 0, options).ToList();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void CompleteArgNames()
        {
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "-" }, 0).ToList()
                .Should().ContainInOrder("-Bar", "-Baz", "-SomeFlag");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/" }, 0).ToList()
                .Should().ContainInOrder("/Bar", "/Baz", "/SomeFlag");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/ba" }, 0).ToList()
                .Should().ContainInOrder("/Bar", "/Baz");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/bar" }, 0).ToList()
                .Should().ContainInOrder("/Bar");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/bars" }, 0).ToList()
                .Should().BeEmpty();

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/Bar", "/Ba" }, 1).ToList()
                .Should().ContainInOrder("/Bar", "/Baz");
        }

        [TestMethod]
        public void CompleteBoolNamedArgument()
        {
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag" }, 0).ToList()
                .Should().ContainInOrder("/SomeFlag");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag=" }, 0).ToList()
                .Should().ContainInOrder("/SomeFlag=False", "/SomeFlag=True");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag=f" }, 0).ToList()
                .Should().ContainInOrder("/SomeFlag=False");

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag+" }, 0).ToList()
                .Should().BeEmpty();

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/SomeFlag-" }, 0).ToList()
                .Should().BeEmpty();

            CommandLineParser.GetCompletions(typeof(SimpleArgs), new[] { "/NotAFlag=f" }, 0).ToList()
                .Should().BeEmpty();
        }

        [TestMethod]
        public void CompleteBoolPositionalArgument()
        {
            CommandLineParser.GetCompletions(typeof(SimpleArgs), new string[] {}, 0).ToList()
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
