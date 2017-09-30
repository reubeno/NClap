using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Utilities;

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

            setPrefixes = () => attribs.ShortNameArgumentPrefixes = null;
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

        [TestMethod]
        public void ShortNameArgPrefixesCannotOverlapLongNameArgPrefixes()
        {
            var attrib = new ArgumentSetAttribute
            {
                NamedArgumentPrefixes = Array.Empty<string>(),
                ShortNameArgumentPrefixes = Array.Empty<string>()
            };

            attrib.AllowMultipleShortNamesInOneToken = true;
            attrib.NamedArgumentPrefixes = new[] { "-", "/" };

            Action a = () => attrib.ShortNameArgumentPrefixes = new[] { ":", "-" };
            a.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void LongNameArgPrefixesCannotOverlapShortNameArgPrefixes()
        {
            var attrib = new ArgumentSetAttribute
            {
                NamedArgumentPrefixes = Array.Empty<string>(),
                ShortNameArgumentPrefixes = Array.Empty<string>()
            };

            attrib.AllowMultipleShortNamesInOneToken = true;
            attrib.ShortNameArgumentPrefixes = new[] { "-", "/" };

            Action a = () => attrib.NamedArgumentPrefixes = new[] { ":", "-" };
            a.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void CanSelectUnspecifiedStyleWithoutThrowingException()
        {
            var attrib = new ArgumentSetAttribute();
            attrib.Style = ArgumentSetStyle.Unspecified;
        }

        [TestMethod]
        public void CanSelectGetOptStyleWithoutThrowingException()
        {
            var attrib = new ArgumentSetAttribute();
            attrib.Style = ArgumentSetStyle.GetOpt;
        }

        [TestMethod]
        public void CanSelectPowerShellStyleWithoutThrowingException()
        {
            var attrib = new ArgumentSetAttribute();
            attrib.Style = ArgumentSetStyle.PowerShell;
        }

        [TestMethod]
        public void CanSelectWindowsCommandLineStyleWithoutThrowingException()
        {
            var attrib = new ArgumentSetAttribute();
            attrib.Style = ArgumentSetStyle.WindowsCommandLine;
        }

        [TestMethod]
        public void SelectingInvalidStyleThrows()
        {
            var attrib = new ArgumentSetAttribute();

            Action a = () => attrib.Style = (ArgumentSetStyle)0xFF;
            a.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void StyleIsUnreadable()
        {
            var attrib = new ArgumentSetAttribute();

            Action a = () => { var x = attrib.Style; };
            a.ShouldThrow<NotSupportedException>();
        }

        [TestMethod]
        public void CertainOptionsAreUnsupportedIfNamedArgPrefixesOverlap()
        {
            var attrib = new ArgumentSetAttribute();
            attrib.NamedArgumentPrefixes.Overlaps(attrib.ShortNameArgumentPrefixes).Should().BeTrue();

            Action a = () => attrib.AllowMultipleShortNamesInOneToken = true;
            a.ShouldThrow<NotSupportedException>();

            a = () => attrib.AllowElidingSeparatorAfterShortName = true;
            a.ShouldThrow<NotSupportedException>();
        }

        [TestMethod]
        public void LogoIsSettableWithString()
        {
            var attrib = new ArgumentSetAttribute { Logo = "Test" };
            attrib.LogoString.ToString().Should().Be("Test");
        }

        [TestMethod]
        public void LogoIsSettableWithColoredString()
        {
            var attrib = new ArgumentSetAttribute
            {
                Logo = new ColoredString("Test", ConsoleColor.Yellow)
            };
            attrib.LogoString.Content.Should().HaveCount(1);
            attrib.LogoString.Content[0].Content.Should().Be("Test");
            attrib.LogoString.Content[0].ForegroundColor.Should().Be(ConsoleColor.Yellow);
        }

        [TestMethod]
        public void LogoIsSettableWithColoredMultistring()
        {
            var attrib = new ArgumentSetAttribute
            {
                Logo = new ColoredMultistring(new[]
                {
                    new ColoredString("Test", ConsoleColor.Yellow),
                    new ColoredString("String", null, ConsoleColor.Cyan)
                })
            };

            attrib.LogoString.Content.Should().HaveCount(2);
            attrib.LogoString.Content[0].Content.Should().Be("Test");
            attrib.LogoString.Content[0].ForegroundColor.Should().Be(ConsoleColor.Yellow);
            attrib.LogoString.Content[1].Content.Should().Be("String");
            attrib.LogoString.Content[1].BackgroundColor.Should().Be(ConsoleColor.Cyan);
        }

        [TestMethod]
        public void LogoThrowsOnUnsupportedTypes()
        {
            var attrib = new ArgumentSetAttribute();

            Action a = () => { attrib.Logo = 3; };
            a.ShouldThrow<ArgumentOutOfRangeException>();
        }
    }
}
