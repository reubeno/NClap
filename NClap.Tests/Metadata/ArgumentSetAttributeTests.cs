using System;
using NClap.Metadata;
using NClap.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class ArgumentSetAttributeTests
    {
    #pragma warning disable 0649 // Field is never assigned to, and will always have its default value

        [ArgumentSet(NamedArgumentPrefixes = new[] { "--" })]
        class AlternatePrefixArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public int Value;
        }

        [ArgumentSet(ArgumentValueSeparators = new[] { '$' })]
        class AlternateSeparatorArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public int Value;
        }

    #pragma warning restore 0649

        [TestMethod]
        public void InvalidPrefixes()
        {
            var attribs = new ArgumentSetAttribute();
            Action setPrefixes = () => attribs.NamedArgumentPrefixes = null;
            setPrefixes.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void InvalidSeparators()
        {
            var attribs = new ArgumentSetAttribute();
            Action setSeparators = () => attribs.ArgumentValueSeparators = null;
            setSeparators.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void AlternatePrefix()
        {
            var args = new AlternatePrefixArguments();

            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            args.Value.Should().Be(0);

            CommandLineParser.Parse(new[] { "/value=10" }, args).Should().BeFalse();
            args.Value.Should().Be(0);

            CommandLineParser.Parse(new[] { "-value=10" }, args).Should().BeFalse();
            args.Value.Should().Be(0);

            CommandLineParser.Parse(new[] { "--value=10" }, args).Should().BeTrue();
            args.Value.Should().Be(10);

            var usageInfo = CommandLineParser.GetUsageInfo(typeof(AlternatePrefixArguments));
            usageInfo.Should().NotContain("/");
            usageInfo.Should().Contain("--");
        }

        [TestMethod]
        public void AlternateSeparators()
        {
            var args = new AlternateSeparatorArguments();

            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            args.Value.Should().Be(0);

            CommandLineParser.Parse(new[] { "/value=10" }, args).Should().BeFalse();
            args.Value.Should().Be(0);

            CommandLineParser.Parse(new[] { "/value$10" }, args).Should().BeTrue();
            args.Value.Should().Be(10);

            var usageInfo = CommandLineParser.GetUsageInfo(typeof(AlternateSeparatorArguments));
            usageInfo.Should().NotContain("=");
            usageInfo.Should().Contain("$");
        }
    }
}
