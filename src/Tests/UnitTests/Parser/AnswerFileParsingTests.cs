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
    public class AnswerFileParsingTests
    {
    #pragma warning disable 0649 // Field is never assigned to, and will always have its default value

        [ArgumentSet(
            Style = ArgumentSetStyle.WindowsCommandLine,
            AnswerFileArgumentPrefix = "@")]
        class Arguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public int IntValue;

            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public string StringValue;
        }
        
        [ArgumentSet(
            Style = ArgumentSetStyle.WindowsCommandLine,
            AnswerFileArgumentPrefix = "#!")]
        class AlternateSyntaxArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public string Value;
        }

    #pragma warning restore 0649

        [TestMethod]
        public void FileDoesNotExist()
        {
            var reader = CreateReaderThatThrows(new FileNotFoundException());
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            TryParse(
                new[] { "@foo" },
                new Arguments(),
                options).Should().BeFalse();

            reader.Received().GetLines("foo");
        }

        [TestMethod]
        public void FileIsEmpty()
        {
            var args = new Arguments();
            var reader = CreateReaderThatReturns(Enumerable.Empty<string>());
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            TryParse(
                new[] { "@foo" },
                args,
                options).Should().BeTrue();

            reader.Received().GetLines("foo");
            args.IntValue.Should().Be(0);
            args.StringValue.Should().BeNull();
        }

        [TestMethod]
        public void FileWithValidArgs()
        {
            var args = new Arguments();
            var reader = CreateReaderThatReturns(new[] { "/intvalue:17", "/stringvalue:a b" });
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            TryParse(
                new[] { "@foo" },
                args,
                options).Should().BeTrue();

            reader.Received().GetLines("foo");
            args.IntValue.Should().Be(17);
            args.StringValue.Should().Be("a b");
        }

        [TestMethod]
        public void AlternateSyntax()
        {
            var args = new AlternateSyntaxArguments();
            var reader = CreateReaderThatReturns(new[] { "/value:abc" });
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            TryParse(new[] { "@foo" }, args, options).Should().BeFalse();
            reader.DidNotReceive().GetLines("foo");

            TryParse(new[] { "#!foo" }, args, options).Should().BeTrue();
            reader.Received().GetLines("foo");
            args.Value.Should().Be("abc");
        }

        [TestMethod]
        public void FileWithComments()
        {
            var args = new Arguments();
            var reader = CreateReaderThatReturns(new[] { "# /intvalue:17", " #asd" });
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            TryParse(
                new[] { "@foo" },
                args,
                options).Should().BeTrue();

            reader.Received().GetLines("foo");
            args.IntValue.Should().Be(0);
            args.StringValue.Should().BeNull();
        }

        [TestMethod]
        public void FileWithEmptyLine()
        {
            var args = new Arguments();
            var reader = CreateReaderThatReturns(new[] { string.Empty });
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            TryParse(
                new[] { "@foo" },
                args,
                options).Should().BeTrue();

            reader.Received().GetLines("foo");
            args.IntValue.Should().Be(0);
            args.StringValue.Should().BeNull();
        }

        private static IFileSystemReader CreateReaderThatThrows(Exception exception)
        {
            var reader = Substitute.For<IFileSystemReader>();
            reader.GetLines(null).ReturnsForAnyArgs(path => { throw exception; });

            return reader;
        }

        private static IFileSystemReader CreateReaderThatReturns(IEnumerable<string> lines)
        {
            var reader = Substitute.For<IFileSystemReader>();
            reader.GetLines(null).ReturnsForAnyArgs(lines);

            return reader;
        }

        private static bool TryParse<T>(IEnumerable<string> args, T dest, CommandLineParserOptions options = null) where T : class =>
            CommandLineParser.TryParse(args, dest, options ?? new CommandLineParserOptions { DisplayUsageInfoOnError = false });
    }
}
