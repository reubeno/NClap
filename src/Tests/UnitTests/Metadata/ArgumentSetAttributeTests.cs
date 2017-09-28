using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class ArgumentSetAttributeTests
    {
    #pragma warning disable 0649 // Field is never assigned to, and will always have its default value

        [ArgumentSet(NamedArgumentPrefixes = new[] { "--" }, ShortNameArgumentPrefixes = new[] { ";" })]
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

        [ArgumentSet(AllowNamedArgumentValueAsSucceedingToken = true)]
        class AllowArgumentValueAfterSpaceArguments
        {
            [NamedArgument]
            public int Value { get; set; }

            [NamedArgument]
            public bool Flag { get; set; }
        }

        [ArgumentSet(
            NamedArgumentPrefixes = new[] { "--" },
            ShortNameArgumentPrefixes = new[] { "/" },
            AllowMultipleShortNamesInOneToken = true)]
        class AllowMultipleShortNamesInOneTokenArguments
        {
            [NamedArgument(ShortName = "x")]
            public bool Value1 { get; set; }

            [NamedArgument(ShortName = "y")]
            public bool Value2 { get; set; }

            [NamedArgument(ShortName = "z")]
            public int Value3 { get; set; }
        }

        [ArgumentSet(
            NamedArgumentPrefixes = new[] { "--" },
            ShortNameArgumentPrefixes = new[] { "/" },
            AllowElidingSeparatorAfterShortName = true)]
        class AllowElidingSeparatorAfterShortNameArguments
        {
            [NamedArgument(ShortName = "v")]
            public int Value { get; set; }
        }

        [ArgumentSet(NameGenerationFlags = ArgumentNameGenerationFlags.GenerateHyphenatedLowerCaseLongNames)]
        class HyphenatedLongNamesArguments
        {
            [NamedArgument]
            public int SomeValue { get; set; }
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

            CommandLineParser.Parse(new[] { ";v=10" }, args).Should().BeTrue();
            args.Value.Should().Be(10);

            var usageInfo = CommandLineParser.GetUsageInfo(typeof(AlternatePrefixArguments));
            usageInfo.ToString().Should().NotContain("/");
            usageInfo.ToString().Should().Contain("--");
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
            usageInfo.ToString().Should().NotContain("=");
            usageInfo.ToString().Should().Contain("$");
        }

        [TestMethod]
        public void ArgumentValueAfterSpace()
        {
            var args = new AllowArgumentValueAfterSpaceArguments();

            CommandLineParser.Parse(new[] { "/value", "10" }, args).Should().BeTrue();
            args.Value.Should().Be(10);

            CommandLineParser.Parse(new[] { "/value=11" }, args).Should().BeTrue();
            args.Value.Should().Be(11);

            CommandLineParser.Parse(new[] { "/value=", "12" }, args).Should().BeTrue();
            args.Value.Should().Be(12);

            CommandLineParser.Parse(new[] { "/flag", "/value=10" }, args).Should().BeTrue();
            args.Value.Should().Be(10);
            args.Flag.Should().BeTrue();

            CommandLineParser.Parse(new[] { "/value=10", "10" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/flag", "false" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void MultipleShortNamesInOneToken()
        {
            var args = new AllowMultipleShortNamesInOneTokenArguments();

            CommandLineParser.Parse(new[] { "/x", "/y", "/z=10" }, args).Should().BeTrue();
            args.Value1.Should().BeTrue();
            args.Value2.Should().BeTrue();
            args.Value3.Should().Be(10);

            CommandLineParser.Parse(new[] { "/xyz=10" }, args).Should().BeTrue();
            args.Value1.Should().BeTrue();
            args.Value2.Should().BeTrue();
            args.Value3.Should().Be(10);

            CommandLineParser.Parse(new[] { "/xy" }, args).Should().BeTrue();
            args.Value1.Should().BeTrue();
            args.Value2.Should().BeTrue();

            CommandLineParser.Parse(new[] { "/xz" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void NoSpaceBetweenShortNameAndValue()
        {
            var args = new AllowElidingSeparatorAfterShortNameArguments();

            CommandLineParser.Parse(new[] { "/v=7" }, args).Should().BeTrue();
            args.Value.Should().Be(7);

            CommandLineParser.Parse(new[] { "/v8" }, args).Should().BeTrue();
            args.Value.Should().Be(8);

            CommandLineParser.Parse(new[] { "-v" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void HyphenatedLongNames()
        {
            var args = new HyphenatedLongNamesArguments();

            CommandLineParser.Parse(new[] { "/some-value=11" }, args).Should().BeTrue();
            args.SomeValue.Should().Be(11);

            CommandLineParser.Format(args).ToList().Should().BeEquivalentTo("/some-value=11");

            CommandLineParser.Parse(new[] { "/SomeValue" }, args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/somevalue" }, args).Should().BeFalse();
        }
    }
}
