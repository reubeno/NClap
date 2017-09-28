using System;
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
        class IncorrectlyUsedMustNotBeEmptyAttributeArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustNotBeEmpty]
            public int Value { get; set; }
        }

        class InvalidDefaultValueArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = "")]
            [MustNotBeEmpty]
            public string Value { get; set; }
        }

        class NonEmptyStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustNotBeEmpty]
            public string Value { get; set; }

            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustNotBeEmpty]
            public FileSystemPath Path { get; set; }
        }

        class NotValueIntArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustNotBe(0)]
            public int Value { get; set; }
        }

        class MultiNotValueIntArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustNotBe(0), MustNotBe(10)]
            public int Value { get; set; }
        }

        class NotValueStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustNotBe("Hello")]
            public string Value { get; set; }
        }

        class RegExStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustMatchRegex("hall(o*)", Options = RegexOptions.None)]
            public string Value { get; set; }
        }

        class NotRegExStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, AllowEmpty = true)]
            [MustNotMatchRegex("hall(o*)", Options = RegexOptions.None)]
            public string Value { get; set; }
        }

        class CaseInsensitiveRegExStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustMatchRegex("hall(o*)", Options = RegexOptions.IgnoreCase)]
            public string Value { get; set; }
        }

        class GreaterThanArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustBeGreaterThan(10)]
            public int Value { get; set; }
        }

        class GreaterThanOrEqualToArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustBeGreaterThanOrEqualTo(10)]
            public int Value { get; set; }
        }

        class LessThanArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustBeLessThan(10)]
            public int Value { get; set; }
        }

        class LessThanOrEqualToArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustBeLessThanOrEqualTo(10)]
            public int Value { get; set; }
        }

        class FileExistsStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustExist(PathExists.AsFile)]
            public string Value { get; set; }
        }

        class FileExistsArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustExist(PathExists.AsFile)]
            public FileSystemPath Value { get; set; }
        }

        class DirectoryExistsArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustExist(PathExists.AsDirectory)]
            public FileSystemPath Value { get; set; }
        }

        class ExistsArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            [MustExist(PathExists.AsFileOrDirectory)]
            public FileSystemPath Value { get; set; }
        }

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
            Action parse = () => CommandLineParser.Parse(new[] { "/value=5" }, args);
            parse.ShouldThrow<InvalidArgumentSetException>();
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
                acceptTypeWithNull.ShouldThrow<ArgumentNullException>();
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
                tryValidateWithNullContext.ShouldThrow<ArgumentNullException>("because {0} shouldn't let it", attrib.GetType().Name);
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
            CommandLineParser.Parse(new string[] { }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=a" }, args).Should().BeTrue();
        }

        [TestMethod]
        public void NonEmptyAttribute()
        {
            var args = new NonEmptyStringArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=a" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value= " }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/path=" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/path=a" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/path= " }, args).Should().BeTrue();
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
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=0" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=7" }, args).Should().BeTrue();
        }

        [TestMethod]
        public void MultiNotValueInt()
        {
            var args = new MultiNotValueIntArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=0" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=7" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=10" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void NotValueString()
        {
            var args = new NotValueStringArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=abc" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=Hello" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=Hello " }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=H ello " }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=HELLO" }, args).Should().BeTrue();
        }

        [TestMethod]
        public void MatchesRegEx()
        {
            var args = new RegExStringArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=hallo" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=halloo" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=hallooo" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=HALLO" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void DoesNotMatchRegEx()
        {
            var args = new NotRegExStringArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=hallo" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=halloo" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=hallooo" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=HALLO" }, args).Should().BeTrue();
        }

        [TestMethod]
        public void CaseInsensitivelyMatchesRegEx()
        {
            var args = new CaseInsensitiveRegExStringArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=hallo" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=halloo" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=hallooo" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=HALLO" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=HALLOoOo" }, args).Should().BeTrue();
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
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=0" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=10" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=11" }, args).Should().BeTrue();
        }

        [TestMethod]
        public void GreaterThanOrEqualTo()
        {
            var args = new GreaterThanOrEqualToArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=0" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=10" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=11" }, args).Should().BeTrue();
        }

        [TestMethod]
        public void LessThan()
        {
            var args = new LessThanArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=0" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=10" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/value=11" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void LessThanOrEqualTo()
        {
            var args = new LessThanOrEqualToArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=0" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=10" }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=11" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void FileStringExistence()
        {
            var args = new FileExistsStringArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();

            var reader = Substitute.For<IFileSystemReader>();
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            reader.FileExists(@"h:\temp").Returns(true);
            CommandLineParser.Parse(new[] { @"/value=h:\temp" }, args, options).Should().BeTrue();

            reader.FileExists(@"h:\temp").Returns(false);
            CommandLineParser.Parse(new[] { @"/value=h:\temp" }, args, options).Should().BeFalse();
        }

        [TestMethod]
        public void FileExistence()
        {
            var args = new FileExistsArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();

            var reader = Substitute.For<IFileSystemReader>();
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            reader.FileExists(@"h:\temp").Returns(true);
            CommandLineParser.Parse(new[] { @"/value=h:\temp" }, args, options).Should().BeTrue();

            reader.FileExists(@"h:\temp").Returns(false);
            CommandLineParser.Parse(new[] { @"/value=h:\temp" }, args, options).Should().BeFalse();
        }

        [TestMethod]
        public void DirectoryExistence()
        {
            var args = new DirectoryExistsArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();

            var reader = Substitute.For<IFileSystemReader>();
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            reader.DirectoryExists(@"h:\temp").Returns(true);
            CommandLineParser.Parse(new[] { @"/value=h:\temp" }, args, options).Should().BeTrue();

            reader.DirectoryExists(@"h:\temp").Returns(false);
            CommandLineParser.Parse(new[] { @"/value=h:\temp" }, args, options).Should().BeFalse();
        }

        [TestMethod]
        public void AnyExistence()
        {
            var args = new ExistsArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();

            var reader = Substitute.For<IFileSystemReader>();
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            reader.FileExists(@"h:\temp").Returns(false);
            reader.DirectoryExists(@"h:\temp").Returns(false);
            CommandLineParser.Parse(new[] { @"/value=h:\temp" }, args, options).Should().BeFalse();

            reader.FileExists(@"h:\temp").Returns(true);
            reader.DirectoryExists(@"h:\temp").Returns(false);
            CommandLineParser.Parse(new[] { @"/value=h:\temp" }, args, options).Should().BeTrue();

            reader.FileExists(@"h:\temp").Returns(false);
            reader.DirectoryExists(@"h:\temp").Returns(true);
            CommandLineParser.Parse(new[] { @"/value=h:\temp" }, args, options).Should().BeTrue();
        }

        [TestMethod]
        public void MustNotExist()
        {
            var args = new NotExistsArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();

            var reader = Substitute.For<IFileSystemReader>();
            var options = new CommandLineParserOptions { FileSystemReader = reader };

            reader.FileExists(@"h:\temp").Returns(false);
            reader.DirectoryExists(@"h:\temp").Returns(false);
            CommandLineParser.Parse(new[] { @"/value=h:\temp" }, args, options).Should().BeTrue();

            reader.FileExists(@"h:\temp").Returns(true);
            reader.DirectoryExists(@"h:\temp").Returns(false);
            CommandLineParser.Parse(new[] { @"/value=h:\temp" }, args, options).Should().BeFalse();

            reader.FileExists(@"h:\temp").Returns(false);
            reader.DirectoryExists(@"h:\temp").Returns(true);
            CommandLineParser.Parse(new[] { @"/value=h:\temp" }, args, options).Should().BeFalse();
        }
    }
}
