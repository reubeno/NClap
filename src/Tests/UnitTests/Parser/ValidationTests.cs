using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Exceptions;
using NClap.Metadata;
using NClap.Types;
using NSubstitute;

namespace NClap.Tests.Parser
{
    [TestClass]
    public class ValidationTests
    {
        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class IncorrectlyUsedMustNotBeEmptyAttributeArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustNotBeEmpty]
            public int Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class InvalidDefaultValueArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = "")]
            [MustNotBeEmpty]
            public string Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class NonEmptyStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustNotBeEmpty]
            public string Value { get; set; }

            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustNotBeEmpty]
            public FileSystemPath Path { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class NotValueIntArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustNotBe(0)]
            public int Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class MultiNotValueIntArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustNotBe(0), MustNotBe(10)]
            public int Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class NotValueStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustNotBe("Hello")]
            public string Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class RegExStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustMatchRegex("hall(o*)", Options = RegexOptions.None)]
            public string Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class NotRegExStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, AllowEmpty = true)]
            [MustNotMatchRegex("hall(o*)", Options = RegexOptions.None)]
            public string Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class CaseInsensitiveRegExStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustMatchRegex("hall(o*)", Options = RegexOptions.IgnoreCase)]
            public string Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class GreaterThanArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustBeGreaterThan(10)]
            public int Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class GreaterThanOrEqualToArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustBeGreaterThanOrEqualTo(10)]
            public int Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class LessThanArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustBeLessThan(10)]
            public int Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class LessThanOrEqualToArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustBeLessThanOrEqualTo(10)]
            public int Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class FileExistsStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustExist(PathExists.AsFile)]
            public string Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class FileExistsArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustExist(PathExists.AsFile)]
            public FileSystemPath Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class DirectoryExistsArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustExist(PathExists.AsDirectory)]
            public FileSystemPath Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class ExistsArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustExist(PathExists.AsFileOrDirectory)]
            public FileSystemPath Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class NotExistsArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustNotExist]
            public FileSystemPath Value { get; set; }
        }

        [TestMethod]
        public void IncorrectUseOfEmptyAttribute()
        {
            var args = new IncorrectlyUsedMustNotBeEmptyAttributeArguments();
            Action parse = () => TryParse(new[] { "/value=5" }, args);
            parse.Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void IncorrectUseOfAcceptTypeOnAttributes()
        {
            var attribs = new ArgumentValidationAttribute[]
            {
                new MustExistAttribute(PathExists.AsFile),
                new MustNotBeEmptyAttribute()
            };

            foreach (var attrib in attribs)
            {
                Action acceptTypeWithNull = () => attrib.AcceptsType(null);
                acceptTypeWithNull.Should().Throw<ArgumentNullException>();
            }
        }

        [TestMethod]
        public void IncorrectUseOfValidationOnAttributes()
        {
            var attribs = new ArgumentValidationAttribute[]
            {
                new MustExistAttribute(PathExists.AsFile),
                new MustNotExistAttribute()
            };

            foreach (var attrib in attribs)
            {
                Action tryValidateWithNullContext = () => attrib.TryValidate(null, 0, out string reason);
                tryValidateWithNullContext.Should().Throw<ArgumentNullException>("because {0} shouldn't let it", attrib.GetType().Name);
            }
        }

        [TestMethod]
        public void RegexValidationAttributes()
        {
            var attrib = new MustMatchRegexAttribute("a*");
            attrib.Pattern.Should().Be("a*");

            var notAttrib = new MustNotMatchRegexAttribute("a*");
            notAttrib.Pattern.Should().Be("a*");
        }

        [TestMethod]
        public void ExistsAttribute()
        {
            var attrib = new MustExistAttribute(PathExists.AsDirectory);
            attrib.Exists.Should().Be(PathExists.AsDirectory);
        }

        [TestMethod]
        public void InvalidDefaultValue()
        {
            var args = new InvalidDefaultValueArguments();
            TryParse(Array.Empty<string>(), args).Should().BeFalse();
            TryParse(new[] { "/value=" }, args).Should().BeFalse();
            TryParse(new[] { "/value=a" }, args).Should().BeTrue();
        }

        [TestMethod]
        public void NonEmptyAttribute()
        {
            var args = new NonEmptyStringArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            TryParse(new[] { "/value=" }, args).Should().BeFalse();
            TryParse(new[] { "/value=a" }, args).Should().BeTrue();
            TryParse(new[] { "/value= " }, args).Should().BeTrue();
            TryParse(new[] { "/path=" }, args).Should().BeFalse();
            TryParse(new[] { "/path=a" }, args).Should().BeTrue();
            TryParse(new[] { "/path= " }, args).Should().BeTrue();
        }

        [TestMethod]
        public void NotValueValue()
        {
            var member = typeof(NotValueIntArguments).GetMember("Value").Single();
            var attrib = member.GetCustomAttributes<MustNotBeAttribute>().Single();
            attrib.Value.Should().Be(0);
        }

        [TestMethod]
        public void NotValueInt()
        {
            var args = new NotValueIntArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            TryParse(new[] { "/value=0" }, args).Should().BeFalse();
            TryParse(new[] { "/value=7" }, args).Should().BeTrue();
        }

        [TestMethod]
        public void MultiNotValueInt()
        {
            var args = new MultiNotValueIntArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            TryParse(new[] { "/value=0" }, args).Should().BeFalse();
            TryParse(new[] { "/value=7" }, args).Should().BeTrue();
            TryParse(new[] { "/value=10" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void NotValueString()
        {
            var args = new NotValueStringArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            TryParse(new[] { "/value=abc" }, args).Should().BeTrue();
            TryParse(new[] { "/value=Hello" }, args).Should().BeFalse();
            TryParse(new[] { "/value=Hello " }, args).Should().BeTrue();
            TryParse(new[] { "/value=H ello " }, args).Should().BeTrue();
            TryParse(new[] { "/value=HELLO" }, args).Should().BeTrue();
        }

        [TestMethod]
        public void MatchesRegEx()
        {
            var args = new RegExStringArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            TryParse(new[] { "/value=" }, args).Should().BeFalse();
            TryParse(new[] { "/value=hallo" }, args).Should().BeTrue();
            TryParse(new[] { "/value=halloo" }, args).Should().BeTrue();
            TryParse(new[] { "/value=hallooo" }, args).Should().BeTrue();
            TryParse(new[] { "/value=HALLO" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void DoesNotMatchRegEx()
        {
            var args = new NotRegExStringArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            TryParse(new[] { "/value=" }, args).Should().BeTrue();
            TryParse(new[] { "/value=hallo" }, args).Should().BeFalse();
            TryParse(new[] { "/value=halloo" }, args).Should().BeFalse();
            TryParse(new[] { "/value=hallooo" }, args).Should().BeFalse();
            TryParse(new[] { "/value=HALLO" }, args).Should().BeTrue();
        }

        [TestMethod]
        public void CaseInsensitivelyMatchesRegEx()
        {
            var args = new CaseInsensitiveRegExStringArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            TryParse(new[] { "/value=" }, args).Should().BeFalse();
            TryParse(new[] { "/value=hallo" }, args).Should().BeTrue();
            TryParse(new[] { "/value=halloo" }, args).Should().BeTrue();
            TryParse(new[] { "/value=hallooo" }, args).Should().BeTrue();
            TryParse(new[] { "/value=HALLO" }, args).Should().BeTrue();
            TryParse(new[] { "/value=HALLOoOo" }, args).Should().BeTrue();
        }

        [TestMethod]
        public void RegExOptionsAccessor()
        {
            var member = typeof(RegExStringArguments).GetMember("Value").Single();
            var matchAttrib = member.GetCustomAttributes<MustMatchRegexAttribute>().Single();
            matchAttrib.Options.Should().Be(RegexOptions.None);

            member = typeof(NotRegExStringArguments).GetMember("Value").Single();
            var insensitiveMatchAttrib = member.GetCustomAttributes<MustNotMatchRegexAttribute>().Single();
            insensitiveMatchAttrib.Options.Should().Be(RegexOptions.None);
        }

        [TestMethod]
        public void GreaterThan()
        {
            var args = new GreaterThanArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            TryParse(new[] { "/value=0" }, args).Should().BeFalse();
            TryParse(new[] { "/value=10" }, args).Should().BeFalse();
            TryParse(new[] { "/value=11" }, args).Should().BeTrue();
        }

        [TestMethod]
        public void GreaterThanOrEqualTo()
        {
            var args = new GreaterThanOrEqualToArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            TryParse(new[] { "/value=0" }, args).Should().BeFalse();
            TryParse(new[] { "/value=10" }, args).Should().BeTrue();
            TryParse(new[] { "/value=11" }, args).Should().BeTrue();
        }

        [TestMethod]
        public void LessThan()
        {
            var args = new LessThanArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            TryParse(new[] { "/value=0" }, args).Should().BeTrue();
            TryParse(new[] { "/value=10" }, args).Should().BeFalse();
            TryParse(new[] { "/value=11" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void LessThanOrEqualTo()
        {
            var args = new LessThanOrEqualToArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            TryParse(new[] { "/value=0" }, args).Should().BeTrue();
            TryParse(new[] { "/value=10" }, args).Should().BeTrue();
            TryParse(new[] { "/value=11" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void FileStringExistence()
        {
            var args = new FileExistsStringArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();

            var reader = Substitute.For<IFileSystemReader>();
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            reader.FileExists(@"h:\temp").Returns(true);
            TryParse(new[] { @"/value=h:\temp" }, args, options).Should().BeTrue();

            reader.FileExists(@"h:\temp").Returns(false);
            TryParse(new[] { @"/value=h:\temp" }, args, options).Should().BeFalse();
        }

        [TestMethod]
        public void FileExistence()
        {
            var args = new FileExistsArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();

            var reader = Substitute.For<IFileSystemReader>();
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            reader.FileExists(@"h:\temp").Returns(true);
            TryParse(new[] { @"/value=h:\temp" }, args, options).Should().BeTrue();

            reader.FileExists(@"h:\temp").Returns(false);
            TryParse(new[] { @"/value=h:\temp" }, args, options).Should().BeFalse();
        }

        [TestMethod]
        public void DirectoryExistence()
        {
            var args = new DirectoryExistsArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();

            var reader = Substitute.For<IFileSystemReader>();
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            reader.DirectoryExists(@"h:\temp").Returns(true);
            TryParse(new[] { @"/value=h:\temp" }, args, options).Should().BeTrue();

            reader.DirectoryExists(@"h:\temp").Returns(false);
            TryParse(new[] { @"/value=h:\temp" }, args, options).Should().BeFalse();
        }

        [TestMethod]
        public void AnyExistence()
        {
            var args = new ExistsArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();

            var reader = Substitute.For<IFileSystemReader>();
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            reader.FileExists(@"h:\temp").Returns(false);
            reader.DirectoryExists(@"h:\temp").Returns(false);
            TryParse(new[] { @"/value=h:\temp" }, args, options).Should().BeFalse();

            reader.FileExists(@"h:\temp").Returns(true);
            reader.DirectoryExists(@"h:\temp").Returns(false);
            TryParse(new[] { @"/value=h:\temp" }, args, options).Should().BeTrue();

            reader.FileExists(@"h:\temp").Returns(false);
            reader.DirectoryExists(@"h:\temp").Returns(true);
            TryParse(new[] { @"/value=h:\temp" }, args, options).Should().BeTrue();
        }

        [TestMethod]
        public void MustNotExist()
        {
            var args = new NotExistsArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();

            var reader = Substitute.For<IFileSystemReader>();
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            reader.FileExists(@"h:\temp").Returns(false);
            reader.DirectoryExists(@"h:\temp").Returns(false);
            TryParse(new[] { @"/value=h:\temp" }, args, options).Should().BeTrue();

            reader.FileExists(@"h:\temp").Returns(true);
            reader.DirectoryExists(@"h:\temp").Returns(false);
            TryParse(new[] { @"/value=h:\temp" }, args, options).Should().BeFalse();

            reader.FileExists(@"h:\temp").Returns(false);
            reader.DirectoryExists(@"h:\temp").Returns(true);
            TryParse(new[] { @"/value=h:\temp" }, args, options).Should().BeFalse();
        }

        private static bool TryParse<T>(IEnumerable<string> args, T dest, CommandLineParserOptions options = null) where T : class =>
            CommandLineParser.TryParse(args, dest, options ?? new CommandLineParserOptions { DisplayUsageInfoOnError = false });
    }
}
