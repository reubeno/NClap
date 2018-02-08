using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Exceptions;
using NClap.Help;
using NClap.Metadata;
using NClap.Types;
using NClap.Utilities;
using NSubstitute;

namespace NClap.Tests.Parser
{
    /// <summary>
    /// Tests for the CommandLineParser class.
    /// </summary>
    [TestClass]
    public class CommandLineParserTests
    {
        enum MyEnum
        {
            SomeOtherValue,
            SomeOtherOtherValue
        }

        enum MyBetterDocumentedEnum
        {
            [ArgumentValue(LongName = "first", ShortName = "f", Description = "The one that starts everything")]
            FirstValue,

            [ArgumentValue(LongName = "middle", ShortName = "m", Description = "The one in the middle")]
            TheOneAfterFirst,

            [ArgumentValue(LongName = "last", Description = "The one that wraps everything up")]
            Final
        }

#pragma warning disable 0649 // Field is never assigned to, and will always have its default value

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class NoArguments
        {
        }

        [ArgumentSet(
            Style = ArgumentSetStyle.WindowsCommandLine,
            Logo = "My logo",
            Examples = new[] { "SimpleArgs /mu=4" })]
        class SimpleArguments : HelpArgumentsBase
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, Description = "Some boolean")]
            public bool MyBool;

            [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "mu")]
            public uint MyUint;

            [NamedArgument(ArgumentFlags.AtMostOnce, LongName = "MyLong")]
            public long MyLong;

            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public ulong MyUlong;

            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public string MyString;

            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public virtual string MyStringProperty { get; set; }

            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public MyEnum MyEnum;

            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public MyEnum MyOtherEnum;

            [NamedArgument(
                ArgumentFlags.AtMostOnce,
                Description = "My infinitely cooler enum value",
                DefaultValue = MyBetterDocumentedEnum.TheOneAfterFirst)]
            public MyBetterDocumentedEnum MyDocumentedEnum;

            [NamedArgument(ArgumentFlags.Multiple, ElementSeparators = new[] { "," })]
            public string[] MyStringArray;

            [NamedArgument(ArgumentFlags.Multiple, ElementSeparators = new string[] { })]
            public string[] MyStringArrayWithoutSeparators;

            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public KeyValuePair<string, string> MyStringPair;

            public string SomeOtherField;

            public static SimpleArguments Parse(IEnumerable<string> tokens)
            {
                var args = new SimpleArguments();
                TryParse(tokens, args).Should().BeTrue();
                return args;
            }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class RequiredArguments : HelpArgumentsBase
        {
            [NamedArgument(ArgumentFlags.Required)]
            public string RequiredArgument;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class RequiredPositionalArguments
        {
            [PositionalArgument(ArgumentFlags.Required)]
            public string RequiredArgument;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class AllArgumentsAsArgumentString
        {
            [NamedArgument(ArgumentFlags.Required | ArgumentFlags.RestOfLine)]
            public string AllArguments;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class AllArgumentsAsUnsettableNamedArgumentString
        {
            [NamedArgument(ArgumentFlags.Required | ArgumentFlags.RestOfLine)]
            public string AllArguments
            {
                get => string.Empty;
                set => throw new ArgumentOutOfRangeException();
            }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class AllArgumentsAsUnsettablePositionalArgumentString
        {
            [PositionalArgument(ArgumentFlags.Required | ArgumentFlags.RestOfLine)]
            public string AllArguments
            {
                get => string.Empty;
                set => throw new ArgumentOutOfRangeException();
            }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class AllArgumentsAsPositionalArgumentString
        {
            [PositionalArgument(ArgumentFlags.Required | ArgumentFlags.RestOfLine)]
            public string AllArguments;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class AllArgumentsAsArray
        {
            [PositionalArgument(ArgumentFlags.Required | ArgumentFlags.RestOfLine)]
            public string[] AllArguments;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class ArgumentsWithDefaultValue
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = 10)]
            public int Argument;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class ArgumentsWithCoerceableDefaultValue
        {
            [NamedArgument(DefaultValue = 1)]
            public uint Argument;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class ArgumentsWithDynamicDefaultValue
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = 10, DynamicDefaultValue = true)]
            public int Argument;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class MultiplePositionalArguments
        {
            [PositionalArgument(ArgumentFlags.Multiple)]
            public string[] Args;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class ReadOnlyFieldArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public readonly int Argument;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class ConstFieldArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public const int Argument = 7;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class PrivateFieldArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, LongName = "argument")]
            private int _argument;

            public int PrivateArgument => _argument;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class StaticFieldArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public static int Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class DerivedArguments : SimpleArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, LongName = "DerivedMyString")]
            public new string MyString;

            public override string MyStringProperty
            {
                get => base.MyStringProperty;
                set => base.MyStringProperty = value;
            }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        struct ValueTypeArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public int Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class NoAttributedArguments
        {
            public int Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class OverriddenShortNameArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public int Argument;

            [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "a")]
            public int OtherArgument;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class EmptyShortNameArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "")]
            public int Argument;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class DuplicateLongNameArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, LongName = "Foo")]
            public string Arg1;

            [NamedArgument(ArgumentFlags.AtMostOnce, LongName = "Foo")]
            public string Arg2;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class NoAvailableShortNameArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public int F;

            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public int Foo;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class DuplicateShortNameArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "a")]
            public int Argument1;

            [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "a")]
            public int Argument2;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class PropertyArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public int Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class UnsettablePropertyArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public int Value => 0;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class SamePositionArguments
        {
            [PositionalArgument(ArgumentFlags.AtMostOnce)]
            public int Value1;

            [PositionalArgument(ArgumentFlags.AtMostOnce)]
            public int Value2;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class NoZeroPositionArguments
        {
            [PositionalArgument(ArgumentFlags.AtMostOnce, Position = 1)]
            public int Value1;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class PositionPlusRestOfLineArguments<T>
        {
            [PositionalArgument(ArgumentFlags.AtMostOnce, Position = 0)]
            public int Value;

            [PositionalArgument(ArgumentFlags.RestOfLine, Position = 1)]
            public T RestOfLine;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class NamedPlusRestOfLineArguments<T>
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public int Value;

            [PositionalArgument(ArgumentFlags.RestOfLine)]
            public T RestOfLine;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class RestOfLinePlusPositionArguments
        {
            [NamedArgument(ArgumentFlags.RestOfLine)]
            public string RestOfLine;

            [PositionalArgument(ArgumentFlags.AtMostOnce, Position = 1)]
            public int Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class MultiplePositionalArgumentsPlusPositionArguments
        {
            [PositionalArgument(ArgumentFlags.Multiple)]
            public string[] ManyValues;

            [PositionalArgument(ArgumentFlags.AtMostOnce, Position = 1)]
            public int Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class PositionalArguments
        {
            [PositionalArgument(ArgumentFlags.Required, Position = 1)]
            public int Value1;

            [PositionalArgument(ArgumentFlags.Required, Position = 0, AllowEmpty = true)]
            public string Value0;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine, Description = "More help content.")]
        class AdditionalHelpArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public int Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class ListArguments
        {
            [NamedArgument(ArgumentFlags.Multiple, LongName = "Value")]
            public List<string> Values;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class IListArguments
        {
            [NamedArgument(ArgumentFlags.Multiple, LongName = "Value")]
            public IList<string> Values;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class ListPropertyArguments
        {
            [NamedArgument(ArgumentFlags.Multiple, LongName = "Value")]
            public List<string> Values { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class ListPropertyThatThrowsArguments
        {
            [NamedArgument(ArgumentFlags.Multiple, LongName = "Value")]
            public List<string> Values
            {
                get => null;
                set => throw new ArgumentOutOfRangeException();
            }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class CustomPropertyArguments
        {
            private string _value;

            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public string Value
            {
                get => _value;
                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }

                    _value = value.Trim().Replace(" ", string.Empty);
                }
            }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class PropertyWithoutGetArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public string Value
            {
                set { }
            }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class ThrowingStringPropertyArguments<TException> where TException : Exception, new()
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public string Value
            {
                get => string.Empty;
                set => throw new TException();
            }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class ThrowingGuidPropertyArguments<TException> where TException : Exception, new()
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public Guid Value
            {
                get => Guid.Empty;
                set => throw new TException();
            }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class UnknownConflictingArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, ConflictsWith = new[] { "Foo" })]
            public string Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class SelfConflictingArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, ConflictsWith = new[] { nameof(Value) })]
            public string Value;

            [NamedArgument(ArgumentFlags.AtMostOnce, ConflictsWith = new[] { nameof(OtherValue) })]
            public string OtherValue;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class ConflictingArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, ConflictsWith = new[] { nameof(OtherValue) })]
            public string Value;

            [NamedArgument(ArgumentFlags.AtMostOnce, ConflictsWith = new[] { nameof(Value) })]
            public string OtherValue;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class PartlySpecifiedConflictingArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, ConflictsWith = new[] { nameof(OtherValue) })]
            public string Value;

            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public string OtherValue;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class FieldDefaultValueOfWrongTypeArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = "foo")]
            public long Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class PropertyDefaultValueOfWrongTypeArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = "foo")]
            public long Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class DefaultValueOfImplicitlyConvertibleTypeArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = 10)]
            public long Value;

            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = 10)]
            public long ValueProp;

            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = @"c:\myfile.txt")]
            public FileSystemPath Path;

            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = @"c:\myfile.txt")]
            public FileSystemPath PathProp;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class ZeroLengthLongNameArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, LongName = "")]
            public bool Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.GetOpt)]
        class CanonicalToEndOfLineArguments
        {
            [NamedArgument(ArgumentFlags.Optional)]
            public string Value { get; set; }

            [NamedArgument(ArgumentFlags.Optional | ArgumentFlags.RestOfLine, LongName = "", ShortName = null)]
            public List<string> FullLine { get; set; }
        }

        [Flags]
        enum MyFlagsEnum
        {
            None = 0,
            FlagOne = 0x1,
            FlagTwo = 0x2
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class FlagEnumArguments
        {
            [NamedArgument(ArgumentFlags.Multiple)]
            public MyFlagsEnum Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine, PublicMembersAreNamedArguments = true)]
        class UnannotatedArguments
        {
            public string StringValue { get; set; }

            public int IntValue { get; set; }

            public int NonWritableValue { get; } = 0;

            protected int ProtectedValue { get; set; }

            private int PrivateValue { get; set; }
        }

        [ArgumentSet(
            Style = ArgumentSetStyle.PowerShell,
            ShortNameArgumentPrefixes = new[] { "-" },
            NamedArgumentPrefixes = new[] {  "/" })]
        class ArgumentsWithDifferentNamePrefixes
        {
            [NamedArgument]
            public int Value { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.GetOpt)]
        class ArgumentsWithGetOptStyle
        {
            [NamedArgument]
            public int Value { get; set; }
        }

        class InvalidLayout : ArgumentHelpLayout
        {
            public override ArgumentHelpLayout DeepClone() => new InvalidLayout();
        }

        enum EmptyEnum
        {
        }

        enum EffectivelyEmptyEnum
        {
            [ArgumentValue(Flags = ArgumentValueFlags.Disallowed)]
            DisallowedValue
        }

        class ArgumentsWithEmptyEnum
        {
            [NamedArgument]
            public EmptyEnum Value { get; set; }

            [NamedArgument]
            public EmptyEnum OtherValue { get; set; }

            [NamedArgument]
            public EffectivelyEmptyEnum OtherOtherValue { get; set; }

            [NamedArgument]
            public EffectivelyEmptyEnum OtherOtherOtherValue { get; set; }
        }

#pragma warning restore 0649

        [TestMethod]
        public void NoArgumentsWorks()
        {
            var args = new NoArguments();
            TryParseWithUsage(Array.Empty<string>(), args).Should().BeTrue();
            TryParseWithUsage(new[] { "/unknown" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void HelpArgumentWorks()
        {
            var reporter = Substitute.For<NClap.ErrorReporter>();

            TryParseWithUsage(
                new[] { "/help" },
                new SimpleArguments(),
                new CommandLineParserOptions { Reporter = reporter })
                    .Should().BeFalse();

            var calls = reporter.ReceivedCalls().ToList();
            calls.Count.Should().BeGreaterOrEqualTo(1);

            var args = calls[0].GetArguments();
            args.Length.Should().Be(1);
            args.All(arg => arg is ColoredMultistring).Should().BeTrue();
            args.Cast<ColoredMultistring>().Any(arg => arg.ToString().Contains("Usage:")).Should().BeTrue();
        }

        [TestMethod]
        public void GetUsageStringWorksWithNoArguments()
        {
            GetUsageStringWorks(new NoArguments());
        }

        [TestMethod]
        public void GetUsageStringWorksWithSimpleArguments()
        {
            GetUsageStringWorks(new SimpleArguments());
        }

        [TestMethod]
        public void GetUsageStringWorksWithNewlyConstructedOptions()
        {

            GetUsageStringWorks(new SimpleArguments(), options: new ArgumentSetHelpOptions());
        }

        [TestMethod]
        public void GetUsageStringWorksWithNoColor()
        {
            GetUsageStringWorks(new SimpleArguments(), options: new ArgumentSetHelpOptions()
                .With()
                .Color(false));
        }

        [TestMethod]
        public void GetUsageStringWorksWithOneColumnLayout()
        {
            GetUsageStringWorks(new SimpleArguments(), options: new ArgumentSetHelpOptions()
                .With()
                .OneColumnLayout());
        }

        [TestMethod]
        public void GetUsageStringWorksWithInterestingLayout()
        {
            GetUsageStringWorks(new SimpleArguments(), options: new ArgumentSetHelpOptions()
                .With()
                .TwoColumnLayout()
                .BlankLinesBetweenArguments()
                .ShortNames(ArgumentShortNameHelpMode.IncludeWithLongName)
                .DefaultValues(ArgumentDefaultValueHelpMode.PrependToDescription));
        }

        [TestMethod]
        public void GetUsageStringWorksWithMultipleSkippedBlankLines()
        {
            GetUsageStringWorks(new SimpleArguments(), options: new ArgumentSetHelpOptions()
                .With()
                .BlankLinesBetweenArguments(2));
        }

        [TestMethod]
        public void GetUsageStringWorksWithEmptyEnumAndNoneEnumFlags()
        {
            GetUsageStringWorks(new ArgumentsWithEmptyEnum(), options: new ArgumentSetHelpOptions()
                .With()
                .OneColumnLayout()
                .EnumValueFlags(ArgumentEnumValueHelpFlags.None));

            GetUsageStringWorks(new ArgumentsWithEmptyEnum(), options: new ArgumentSetHelpOptions()
                .With()
                .TwoColumnLayout()
                .EnumValueFlags(ArgumentEnumValueHelpFlags.None));
        }

        [TestMethod]
        public void GetUsageStringWorksWithEmptyEnumAndCoalescedEnums()
        {
            GetUsageStringWorks(new ArgumentsWithEmptyEnum(), options: new ArgumentSetHelpOptions()
                .With()
                .OneColumnLayout()
                .EnumValueFlags(ArgumentEnumValueHelpFlags.SingleSummaryOfEnumsWithMultipleUses));

            GetUsageStringWorks(new ArgumentsWithEmptyEnum(), options: new ArgumentSetHelpOptions()
                .With()
                .TwoColumnLayout()
                .EnumValueFlags(ArgumentEnumValueHelpFlags.SingleSummaryOfEnumsWithMultipleUses));
        }

        [TestMethod]
        public void GetUsageStringWorksWithPositionalArguments()
        {
            GetUsageStringWorks(new PositionalArguments());
        }

        [TestMethod]
        public void GetUsageStringWorksWithMissingSections()
        {
            GetUsageStringWorks(new SimpleArguments(), options: new ArgumentSetHelpOptions()
                .With()
                .NoDescription());

            GetUsageStringWorks(new SimpleArguments(), options: new ArgumentSetHelpOptions()
                .With()
                .NoLogo());

            GetUsageStringWorks(new SimpleArguments(), options: new ArgumentSetHelpOptions()
                .With()
                .NoExamples());

            GetUsageStringWorks(new SimpleArguments(), options: new ArgumentSetHelpOptions()
                .With()
                .NoSyntaxSummary());

            GetUsageStringWorks(new SimpleArguments(), options: new ArgumentSetHelpOptions()
                .With()
                .NoEnumValues());
        }

        [TestMethod]
        public void GetUsageInfoThrowsOnInvalidConfigurations()
        {
            ArgumentSetHelpOptions helpOptions = new ArgumentSetHelpOptions();
            Action a = () => CommandLineParser.GetUsageInfo(typeof(SimpleArguments), helpOptions);

            // Column widths are both 0.
            helpOptions = helpOptions
                .With()
                .TwoColumnLayout()
                .ColumnWidths(0, 0);
            a.Should().Throw<NotSupportedException>();

            // At least one column width is negative.
            helpOptions = helpOptions
                .With()
                .TwoColumnLayout()
                .ColumnWidths(Any.NegativeInt(), 10);
            a.Should().Throw<NotSupportedException>();

            // Columns don't fit in total width.
            helpOptions = helpOptions
                .With()
                .MaxWidth(30)
                .ColumnWidths(18, 18);
            a.Should().Throw<NotSupportedException>();

            // Separators are different lengths.
            helpOptions = helpOptions
                .With()
                .ColumnSeparator(" ", "-----");
            a.Should().Throw<NotSupportedException>();

            // Invalid layout.
            helpOptions.Arguments.Layout = new InvalidLayout();
            a.Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void GetUsageStringThrowsWithVerySmallWidth()
        {
            Action getUsage = () => CommandLineParser.GetUsageInfo(typeof(SimpleArguments), new ArgumentSetHelpOptions { MaxWidth = 4 });
            getUsage.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void GetUsageStringWorkWithSmallButNotTinyWidth()
        {
            GetUsageStringWorks(new SimpleArguments(), new ArgumentSetHelpOptions().With().MaxWidth(40));
        }

        [TestMethod]
        public void GetUsageStringWithAdditionalHelp()
        {
            var usageStr = CommandLineParser.GetUsageInfo(typeof(AdditionalHelpArguments)).ToString();

            usageStr.Should().Contain("More help content.");
        }

        private static void GetUsageStringWorks<T>(T args, ArgumentSetHelpOptions options = null)
        {
            var widerOptions = options?.DeepClone() ?? new ArgumentSetHelpOptions();
            widerOptions.MaxWidth = 80;

            var usageStr = CommandLineParser.GetUsageInfo(
                typeof(T), widerOptions, args).ToString();
            AssertThatUsageInfoSeemsSane(usageStr, widerOptions.MaxWidth.Value);

            var narrowOptions = options?.DeepClone() ?? new ArgumentSetHelpOptions();
            narrowOptions.MaxWidth = 20;

            var narrowUsageStr = CommandLineParser.GetUsageInfo(
                typeof(T), narrowOptions, args).ToString();
            AssertThatUsageInfoSeemsSane(usageStr, widerOptions.MaxWidth.Value);
        }

        private static void AssertThatUsageInfoSeemsSane(string value, int maxWidth)
        {
            value.Should().NotBeNullOrEmpty();

            var lines = value.Replace("\r", string.Empty).Split('\n');
            lines.Should().NotContain(line => line.Length > maxWidth);
        }

        [TestMethod]
        public void InvalidArgumentsGetThrown()
        {
            var args = new SimpleArguments();

            Action parse0 = () => TryParseWithUsage(null, args);
            parse0.Should().Throw<ArgumentNullException>();

            Action parse1 = () => TryParseWithUsage(Array.Empty<string>(), (SimpleArguments)null);
            parse1.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void MultiplePositionalArgumentsParseCorrectly()
        {
            var args = new MultiplePositionalArguments();
            TryParseWithUsage(new[] { "foo", "bar" }, args).Should().BeTrue();
            args.Args.Should().NotBeNull();
            args.Args.Length.Should().Be(2);
            args.Args.Should().HaveElementAt(0, "foo");
            args.Args.Should().HaveElementAt(1, "bar");

            CommandLineParser.Format(args).Should().Equal("foo", "bar");
        }

        [TestMethod]
        public void BoolParsesCorrectly()
        {
            var args = SimpleArguments.Parse(new[] { "/mybool" });
            args.MyBool.Should().BeTrue();
            CommandLineParser.Format(args).Should().Equal("/MyBool=True");

            var args1 = SimpleArguments.Parse(new[] { "/mybool+" });
            args1.MyBool.Should().BeTrue();
            CommandLineParser.Format(args1).Should().Equal("/MyBool=True");

            var args2 = SimpleArguments.Parse(new[] { "/mybool-" });
            args2.MyBool.Should().BeFalse();
            CommandLineParser.Format(args2).Should().BeEmpty();
        }

        [TestMethod]
        public void IntegersParseCorrectly()
        {
            var argsUInt = SimpleArguments.Parse(new[] { "/myuint:33" });
            argsUInt.MyUint.Should().Be(33);
            CommandLineParser.Format(argsUInt).Should().Equal("/MyUint=33");

            var argsUIntHex = SimpleArguments.Parse(new[] { "/myuint:0x21" });
            argsUIntHex.MyUint.Should().Be(33);
            CommandLineParser.Format(argsUIntHex).Should().Equal("/MyUint=33");

            var argsUIntDecimal = SimpleArguments.Parse(new[] { "/myuint:0n33" });
            argsUIntDecimal.MyUint.Should().Be(33);
            CommandLineParser.Format(argsUIntDecimal).Should().Equal("/MyUint=33");

            var argsLong = SimpleArguments.Parse(new[] { "/mylong:33" });
            argsLong.MyLong.Should().Be(33);
            CommandLineParser.Format(argsLong).Should().Equal("/MyLong=33");

            var argsUlong = SimpleArguments.Parse(new[] { "/myulong:33" });
            argsUlong.MyUlong.Should().Be(33);
            CommandLineParser.Format(argsUlong).Should().Equal("/MyUlong=33");
        }

        [TestMethod]
        public void StringParsesCorrectly()
        {
            var args = SimpleArguments.Parse(new[] { "/mystring:myvalue" });
            args.MyString.Should().Be("myvalue");
            CommandLineParser.Format(args).Should().Equal("/MyString=myvalue");
        }

        [TestMethod]
        public void EnumParsesCorrectly()
        {
            var args = SimpleArguments.Parse(new[] { "/myenum:someothervalue" });
            args.MyEnum.Should().Be(MyEnum.SomeOtherValue);
            CommandLineParser.Format(args).Should().BeEmpty();
        }

        [TestMethod]
        public void KeyValuePairParsesCorrectly()
        {
            var args = SimpleArguments.Parse(new[] { "/mystringpair:somekey=somevalue" });
            args.MyStringPair.Key.Should().Be("somekey");
            args.MyStringPair.Value.Should().Be("somevalue");
            CommandLineParser.Format(args).Should().Equal("/MyStringPair=somekey=somevalue");
        }

        [TestMethod]
        public void RequiredArgumentNotGiven()
        {
            var args = new RequiredArguments();

            var reportedBuilder = new StringBuilder();

            var options = new CommandLineParserOptions
            {
                Reporter = s => reportedBuilder.Append(s)
            };

            // Make sure the parse fails.
            TryParseWithUsage(new List<string>(), args, options).Should().BeFalse();

            // Make sure the reported content contains an empty line followed by
            // generic usage info.
            var reported = reportedBuilder.ToString();
            reported.Should().Contain(Environment.NewLine + Environment.NewLine + "Usage:");
        }

        [TestMethod]
        public void DefaultRequiredArgumentNotGiven()
        {
            var args = new RequiredPositionalArguments();
            TryParseWithUsage(new List<string>(), args).Should().BeFalse();
        }

        [TestMethod]
        public void UnknownOptionFailsToParse()
        {
            var args = new SimpleArguments();
            TryParse(new[] { "/unknown:foo" }, args).Should().BeFalse();

            var args2 = new SimpleArguments();
            TryParse(new[] { "unknown" }, args2).Should().BeFalse();
        }

        [TestMethod]
        public void ZeroLengthOptionFailsToParse()
        {
            var args = new ArgumentsWithDifferentNamePrefixes();
            TryParse(new[] { "/" }, args).Should().BeFalse();
            TryParse(new[] { "-" }, args).Should().BeFalse();

            var args2 = new ArgumentsWithGetOptStyle();
            TryParse(new[] { "-" }, args2).Should().BeFalse();
            TryParse(new[] { "--" }, args2).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatEmptyStringCanBeValidPositionalArgument()
        {
            var args = new PositionalArguments();
            TryParse(new[] { string.Empty, "7" }, args).Should().BeTrue();
            args.Value0.Should().Be(string.Empty);
            args.Value1.Should().Be(7);
        }

        [TestMethod]
        public void RestOfLineAsOneString()
        {
            var args = new AllArgumentsAsPositionalArgumentString();
            TryParse(new[] { "foo", "bar" }, args).Should().BeTrue();
            args.AllArguments.Should().Be("foo bar");

            CommandLineParser.Format(args).Should().Equal(new[] { "foo bar" });
        }

        [TestMethod]
        public void RestOfLineWithEmbeddedSpacesAsOneString()
        {
            var args = new AllArgumentsAsPositionalArgumentString();
            TryParse(new[] { "foo", "bar baz" }, args).Should().BeTrue();
            args.AllArguments.Should().Be("foo \"bar baz\"");

            CommandLineParser.Format(args).Should().Equal("foo \"bar baz\"");
        }

        [TestMethod]
        public void RestOfLineAsOneNamedString()
        {
            var args = new AllArgumentsAsArgumentString();
            TryParse(new[] { "/AllArguments:foo", "bar" }, args).Should().BeTrue();
            args.AllArguments.Should().Be("foo bar");

            CommandLineParser.Format(args).Should().Equal("/AllArguments=foo bar");
        }

        [TestMethod]
        public void RestOfLineAsOneUnsettableNamedString()
        {
            var args = new AllArgumentsAsUnsettableNamedArgumentString();
            TryParse(new[] { "/AllArguments:foo", "bar" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void RestOfLineAsOneUnsettablePositionalString()
        {
            var args = new AllArgumentsAsUnsettablePositionalArgumentString();
            TryParse(new[] { "foo", "bar" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void RestOfLineAsArrayContainingQuestionMark()
        {
            var args = new AllArgumentsAsArray();
            TryParse(new[] { "foo", "/?" }, args).Should().BeTrue();
            args.AllArguments.Should().NotBeNull();
            args.AllArguments.Length.Should().Be(2);
            args.AllArguments.Should().HaveElementAt(0, "foo");
            args.AllArguments.Should().HaveElementAt(1, "/?");
        }

        [TestMethod]
        public void RestOfLineAsStringContainingQuestionMark()
        {
            var args = new AllArgumentsAsPositionalArgumentString();
            TryParse(new[] { "foo", "/?" }, args).Should().BeTrue();
            args.AllArguments.Should().Be("foo /?");
        }

        [TestMethod]
        public void RestOfLineAsArrayContainingTokensThatLookLikeOptions()
        {
            var args = new AllArgumentsAsArray();
            TryParse(new[] { "foo", "-bar+", "/foo=baz" }, args).Should().BeTrue();
            args.AllArguments.Should().NotBeNull();
            args.AllArguments.Length.Should().Be(3);
            args.AllArguments.Should().HaveElementAt(0, "foo");
            args.AllArguments.Should().HaveElementAt(1, "-bar+");
            args.AllArguments.Should().HaveElementAt(2, "/foo=baz");
        }

        [TestMethod]
        public void RestOfLineUsageInfo()
        {
            var usage = CommandLineParser.GetUsageInfo(typeof(AllArgumentsAsArgumentString));
            usage.Should().NotBeNull();
            usage.ToString().Should().Contain("/AllArguments=<...>");

            usage = CommandLineParser.GetUsageInfo(typeof(AllArgumentsAsPositionalArgumentString));
            usage.Should().NotBeNull();
            usage.ToString().Should().Contain("<AllArguments>...");
        }

        [TestMethod]
        public void DefaultValueIsUsedWhenArgumentNotPresent()
        {
            var args = new ArgumentsWithDefaultValue();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            args.Argument.Should().Be(10);

            CommandLineParser.Format(args).Should().BeEmpty();
        }

        [TestMethod]
        public void CoerceableDefaultValueWorks()
        {
            var args = new ArgumentsWithCoerceableDefaultValue();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            args.Argument.Should().Be(1U);
        }

        [TestMethod]
        public void AlreadySetValueIsIgnoredByDefault()
        {
            var args = new ArgumentsWithDefaultValue { Argument = 17 };
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            args.Argument.Should().Be(10);

            CommandLineParser.Format(args).Should().BeEmpty();
        }

        [TestMethod]
        public void DynamicDefaultValueIsObserved()
        {
            var args = new ArgumentsWithDynamicDefaultValue { Argument = 17 };
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            args.Argument.Should().Be(17);

            CommandLineParser.Format(args).Should().Equal("/Argument=17");
        }

        [TestMethod]
        public void EmptyCommandLineParsesOkay()
        {
            var args = new SimpleArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            CommandLineParser.Format(args).Should().BeEmpty();
        }

        [TestMethod]
        public void EmptyOptionIsTreatedAsUnknownArgument()
        {
            var args = new SimpleArguments();
            TryParse(new[] { "/" }, args).Should().BeFalse();

            var args2 = new AllArgumentsAsPositionalArgumentString();
            TryParse(new[] { "/", "a" }, args2).Should().BeFalse();
        }

        [TestMethod]
        public void UnsupportedFieldTypesThrow()
        {
            Action readOnlyField = () => TryParse(Array.Empty<string>(), new ReadOnlyFieldArguments());
            readOnlyField.Should().Throw<InvalidArgumentSetException>();

            Action constField = () => TryParse(Array.Empty<string>(), new ConstFieldArguments());
            constField.Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void PrivateFieldArgument()
        {
            var args = new PrivateFieldArguments();

            args.PrivateArgument.Should().Be(0);
            TryParse(new[] { "/argument=7" }, args).Should().BeTrue();
            args.PrivateArgument.Should().Be(7);
        }

        [TestMethod]
        public void StaticFieldArgument()
        {
            var args = new StaticFieldArguments();

            StaticFieldArguments.Value.Should().Be(0);
            TryParse(new[] { "/value=11" }, args).Should().BeTrue();
            StaticFieldArguments.Value.Should().Be(11);
        }

        [TestMethod]
        public void DerivedClassArguments()
        {
            var args = new DerivedArguments();

            TryParse(new[] { "/DerivedMyString=7", "/MyString=xyzzy", "/MyStringProperty=abcd" }, args).Should().BeTrue();
            args.MyString.Should().Be("7");
            ((SimpleArguments)args).MyString.Should().Be("xyzzy");
            args.MyStringProperty.Should().Be("abcd");
        }

        [TestMethod]
        public void NoAttributesInArguments()
        {
            var args = new NoAttributedArguments();

            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            TryParse(new[] { "/value=7" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void OverriddenShortName()
        {
            var args = new OverriddenShortNameArguments();

            TryParse(new[] { "/a=7" }, args).Should().BeTrue();
            args.Argument.Should().Be(0);
            args.OtherArgument.Should().Be(7);
        }

        [TestMethod]
        public void EmptyShortName()
        {
            var args = new EmptyShortNameArguments();

            Action parseFunc = () => TryParse(Array.Empty<string>(), args);
            parseFunc.Should().NotThrow();
        }

        [TestMethod]
        public void DuplicateLongNames()
        {
            var args = new DuplicateLongNameArguments();

            Action parseFunc = () => TryParse(Array.Empty<string>(), args);
            parseFunc.Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void NoAvailableShortNames()
        {
            var args = new NoAvailableShortNameArguments();

            TryParse(new[] {"/F=42"}, args).Should().BeTrue();
            args.F.Should().Be(42);
            args.Foo.Should().Be(0);
        }

        [TestMethod]
        public void DuplicateShortNames()
        {
            var args = new DuplicateShortNameArguments();

            Action parseFunc = () => TryParse(Array.Empty<string>(), args);
            parseFunc.Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void PropertyArgument()
        {
            var args = new PropertyArguments();

            args.Value.Should().Be(0);
            TryParse(new[] { "/value=12" }, args).Should().BeTrue();
            args.Value.Should().Be(12);
        }

        [TestMethod]
        public void UnsettablePropertyArgument()
        {
            Action parse = () => TryParse(Array.Empty<string>(), new UnsettablePropertyArguments());
            parse.Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void DuplicatePositionThrows()
        {
            Action parse = () => TryParse(Array.Empty<string>(), new SamePositionArguments());
            parse.Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void NoZeroPositionThrows()
        {
            Action parse = () => TryParse(Array.Empty<string>(), new NoZeroPositionArguments());
            parse.Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void PositionPlusRestOfLineAsStringArgumentsTest()
        {
            var args = new PositionPlusRestOfLineArguments<string>();
            TryParse(new[] { "10", "bar", "baz" }, args).Should().BeTrue();

            args.Value.Should().Be(10);
            args.RestOfLine.Should().Be("bar baz");
        }

        [TestMethod]
        public void PositionPlusRestOfLineAsStringArrayArgumentsTest()
        {
            var args = new PositionPlusRestOfLineArguments<string[]>();
            TryParse(new[] { "10", "bar", "baz" }, args).Should().BeTrue();

            args.Value.Should().Be(10);
            args.RestOfLine.Should().Equal("bar", "baz");
        }

        [TestMethod]
        public void NamedPlusRestOfLineAsStringArgumentsTest()
        {
            var args = new NamedPlusRestOfLineArguments<string>();
            TryParse(new[] { "/Value=10", "bar", "baz" }, args).Should().BeTrue();

            args.Value.Should().Be(10);
            args.RestOfLine.Should().Be("bar baz");
        }

        [TestMethod]
        public void NamedPlusRestOfLineAsStringArrayArgumentsTest()
        {
            var args = new NamedPlusRestOfLineArguments<string[]>();
            TryParse(new[] { "/Value=10", "bar", "baz" }, args).Should().BeTrue();

            args.Value.Should().Be(10);
            args.RestOfLine.Should().Equal("bar", "baz");
        }

        [TestMethod]
        public void RestOfLinePlusPositionalArgumentThrows()
        {
            Action parse = () => TryParse(Array.Empty<string>(), new RestOfLinePlusPositionArguments());
            parse.Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void MultiplePositionalArgumentsPlusPositionArgumentsThrows()
        {
            Action parse = () => TryParse(Array.Empty<string>(), new MultiplePositionalArgumentsPlusPositionArguments());
            parse.Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void PositionalArgumentsWork()
        {
            var args = new PositionalArguments();

            TryParse(new[] { "foo", "9" }, args).Should().BeTrue();
            args.Value0.Should().Be("foo");
            args.Value1.Should().Be(9);
        }

        [TestMethod]
        public void StringListParsesOkay()
        {
            var args = new ListArguments();
            TryParse(new[] { "/value=a", "/value=b" }, args).Should().Be(true);

            args.Values.Should().HaveElementAt(0, "a");
            args.Values.Should().HaveElementAt(1, "b");
        }

        [TestMethod]
        public void StringIListDoesNotParseOkay()
        {
            var args = new IListArguments();

            Action parse = () => TryParse(new[] { "/value=a", "/value=b" }, args);
            parse.Should().Throw<Exception>();
        }

        [TestMethod]
        public void StringListPropertyParsesOkay()
        {
            var args = new ListPropertyArguments();
            TryParse(new[] { "/value=a", "/value=b" }, args).Should().BeTrue();

            args.Values.Should().HaveElementAt(0, "a");
            args.Values.Should().HaveElementAt(1, "b");
        }

        [TestMethod]
        public void TestThatStringListPropertyThatThrowsDoesNotParse()
        {
            var args = new ListPropertyThatThrowsArguments();
            TryParse(new[] { "/value=a", "/value=b" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void CustomPropertyParsesOkay()
        {
            var args = new CustomPropertyArguments();

            TryParse(new[] { "/value=a" }, args).Should().BeTrue();
            args.Value.Should().Be("a");

            TryParse(new[] { "/value= a bc " }, args).Should().BeTrue();
            args.Value.Should().Be("abc");

            TryParse(new[] { "/value= " }, args).Should().BeFalse();
        }

        [TestMethod]
        public void NotImplPropertyExceptionIsNotCaught()
        {
            var args = new ThrowingStringPropertyArguments<NotImplementedException>();

            Action parse = () => TryParse(new[] { "/value=a" }, args);
            parse.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public void ArgOutOfRangeStringPropertyExceptionIsNotCaught()
        {
            var args = new ThrowingStringPropertyArguments<ArgumentOutOfRangeException>();
            TryParse(new[] { "/value=a" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void ArgOutOfRangeGuidPropertyExceptionIsNotCaught()
        {
            var args = new ThrowingGuidPropertyArguments<ArgumentOutOfRangeException>();
            TryParse(new[] { "/value=8496ade9-c703-4bce-afc2-b98b63e4fa86" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void PropertyWithoutGetThrows()
        {
            var args = new PropertyWithoutGetArguments();

            Action parse = () => TryParse(new[] { "/value=a" }, args);
            parse.Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void UnknownConflictingArgumentNameThrows()
        {
            var args = new UnknownConflictingArguments();

            Action parse = () => TryParse(new[] { "/value=a" }, args);
            parse.Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void SelfConflictingArgumentThrows()
        {
            var args = new SelfConflictingArguments();

            Action parse = () => TryParse(new[] { "/value=a" }, args);
            parse.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ConflictingArgumentsWorkAsExpected()
        {
            var argsList = new object[] { new ConflictingArguments(), new PartlySpecifiedConflictingArguments() };

            foreach (var args in argsList)
            {
                TryParse(new[] { "/value=a" }, args).Should().BeTrue();
                TryParse(new[] { "/othervalue=b" }, args).Should().BeTrue();
                TryParse(new[] { "/value=a", "/othervalue=b" }, args).Should().BeFalse();
            }
        }

        [TestMethod]
        public void FieldDefaultValueOfWrongType()
        {
            var args = new FieldDefaultValueOfWrongTypeArguments();
            Action parse = () => TryParse(Array.Empty<string>(), args);
            parse.Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void PropertyDefaultValueOfWrongType()
        {
            var args = new PropertyDefaultValueOfWrongTypeArguments();
            Action parse = () => TryParse(Array.Empty<string>(), args);
            parse.Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void DefaultValueOfImplicitlyConvertibleType()
        {
            var args = new DefaultValueOfImplicitlyConvertibleTypeArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();

            args.Value.Should().Be(10);
            args.ValueProp.Should().Be(10);

            args.Path.Path.Should().Be(@"c:\myfile.txt");
            args.PathProp.Path.Should().Be(@"c:\myfile.txt");
        }

        [TestMethod]
        public void ZeroLengthLongNameThrows()
        {
            var args = new ZeroLengthLongNameArguments();
            Action parse = () => TryParse(Array.Empty<string>(), args);
            parse.Should().Throw<Exception>();
        }

        [TestMethod]
        public void CanonicalToEndOfLine()
        {
            var args = new CanonicalToEndOfLineArguments();

            // TODO: Not yet implemented.
#if true
            Action a = () => TryParse(new[] { "--value", "v", "--", "--value", "t" }, args);
            a.Should().Throw<InvalidArgumentSetException>();
#else
            TryParse(new[] { "--value", "v", "--", "--value", "t" }, args).Should().BeTrue();
            args.Value.Should().Be("v");
            args.FullLine.Should().Equal("--value", "t");
#endif
        }

        [TestMethod]
        public void FlagEnumArgs()
        {
            var args = new FlagEnumArguments();

            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            args.Value.Should().Be(MyFlagsEnum.None);

            TryParse(new[] { "/Value=FlagOne" }, args).Should().BeTrue();
            args.Value.Should().Be(MyFlagsEnum.FlagOne);

            TryParse(new[] { "/Value=FlagOne|FlagTwo" }, args).Should().BeTrue();
            args.Value.Should().Be(MyFlagsEnum.FlagOne | MyFlagsEnum.FlagTwo);
        }

        [TestMethod]
        public void StringArrayArgsWithSeparateTokens()
        {
            var args = new SimpleArguments();

            TryParse(new[] { "/MyStringArray=foo", "/MyStringArray=bar" }, args).Should().BeTrue();
            args.MyStringArray.Should().NotBeNull();
            args.MyStringArray.Should().Equal("foo", "bar");
        }

        [TestMethod]
        public void StringArrayArgsInSingleTokenWithAllowedSeparators()
        {
            var args = new SimpleArguments();

            TryParse(new[] { "/MyStringArray=foo,bar" }, args).Should().BeTrue();
            args.MyStringArray.Should().NotBeNull();
            args.MyStringArray.Should().Equal("foo", "bar");
        }

        [TestMethod]
        public void StringArrayArgsInSingleTokenWithoutAllowedSeparators()
        {
            var args = new SimpleArguments();

            TryParse(new[] { "/MyStringArrayWithoutSeparators=foo,bar" }, args).Should().BeTrue();
            args.MyStringArrayWithoutSeparators.Should().NotBeNull();
            args.MyStringArrayWithoutSeparators.Should().Equal("foo,bar");
        }

        [TestMethod]
        public void NonWritableUnannotatedMembersDoNotBecomeArguments()
        {
            var args = new UnannotatedArguments();
            TryParse(new[] { "/NonWritableValue=7" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void ProtectedUnannotatedMembersDoNotBecomeArguments()
        {
            var args = new UnannotatedArguments();
            TryParse(new[] { "/ProtectedValue=7" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void PrivateUnannotatedMembersDoNotBecomeArguments()
        {
            var args = new UnannotatedArguments();
            TryParse(new[] { "/PrivateValue=7" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void PublicUnannotatedMembersBecomeArguments()
        {
            var args = new UnannotatedArguments();
            TryParse(Array.Empty<string>(), args).Should().BeTrue();
            args.StringValue.Should().BeNull();
            args.IntValue.Should().Be(0);

            args = new UnannotatedArguments();
            TryParse(new[] {"/StringValue=x"}, args).Should().BeTrue();
            args.StringValue.Should().Be("x");
            args.IntValue.Should().Be(0);

            args = new UnannotatedArguments();
            TryParse(new[] {"/IntValue=7"}, args).Should().BeTrue();
            args.StringValue.Should().BeNull();
            args.IntValue.Should().Be(7);

            args = new UnannotatedArguments();
            TryParse(new[] {"/StringValue=x", "/IntValue=7"}, args).Should().BeTrue();
            args.StringValue.Should().Be("x");
            args.IntValue.Should().Be(7);
        }

        [TestMethod]
        public void GetLogoWorks()
        {
            var logo = CommandLineParser.GetLogo();
            logo.Should().NotBeNullOrWhiteSpace();
            logo.Should().EndWith("\n");
        }

        private static bool TryParseWithUsage<T>(IEnumerable<string> args, T dest, CommandLineParserOptions options = null) where T : class =>
            CommandLineParser.TryParse(args, dest, options ?? new CommandLineParserOptions());

        private static bool TryParse<T>(IEnumerable<string> args, T dest) where T : class =>
            CommandLineParser.TryParse(args, dest, new CommandLineParserOptions { DisplayUsageInfoOnError = false });
    }
}
