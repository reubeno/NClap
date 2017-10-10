using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Exceptions;
using NClap.Metadata;
using NClap.Parser;
using NClap.Types;
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

    #pragma warning disable 0649 // Field is never assigned to, and will always have its default value

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class NoArguments
        {
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine, Examples = new[] { "SimpleArgs /mu=4" })]
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

            [NamedArgument(ArgumentFlags.Multiple)]
            public string[] MyStringArray;

            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public KeyValuePair<string, string> MyStringPair;

            public string SomeOtherField;

            public static SimpleArguments Parse(IEnumerable<string> tokens)
            {
                var args = new SimpleArguments();
                CommandLineParser.Parse(tokens, args).Should().BeTrue();
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

            [PositionalArgument(ArgumentFlags.Required, Position = 0)]
            public string Value0;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine, AdditionalHelp = "More help content.")]
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
            [NamedArgument(ArgumentFlags.AtMostOnce, ConflictsWith = new[] {"Foo"})]
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

#pragma warning restore 0649

        [TestMethod]
        public void NoArgumentsWorks()
        {
            var args = new NoArguments();
            CommandLineParser.ParseWithUsage(new string[] { }, args).Should().BeTrue();
            CommandLineParser.ParseWithUsage(new[] { "/unknown" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void HelpArgumentWorks()
        {
            var reporter = Substitute.For<NClap.ErrorReporter>();

            CommandLineParser.ParseWithUsage(
                new[] { "/?" },
                new SimpleArguments(),
                reporter).Should().BeFalse();

            var calls = reporter.ReceivedCalls().ToList();
            calls.Count.Should().BeGreaterOrEqualTo(1);

            var args = calls[0].GetArguments();
            args.Length.Should().Be(1);
            args.All(arg => arg is string).Should().BeTrue();
            args.Cast<string>().Any(arg => arg.Contains("Usage:")).Should().BeTrue();
        }

        [TestMethod]
        public void GetUsageStringWorks()
        {
            GetUsageStringWorks(new SimpleArguments());
            GetUsageStringWorks(new SimpleArguments(), options: UsageInfoOptions.IncludeBasicSyntax | UsageInfoOptions.UseColor);
            GetUsageStringWorks(new SimpleArguments(), options: UsageInfoOptions.IncludeLogo);
            GetUsageStringWorks(new SimpleArguments(), options: UsageInfoOptions.IncludeExamples);
            GetUsageStringWorks(new SimpleArguments(), options: UsageInfoOptions.IncludeParameterDescriptions | UsageInfoOptions.IncludeParameterDefaultValues);
            GetUsageStringWorks(new SimpleArguments(), options: UsageInfoOptions.Default);
            GetUsageStringWorks(new SimpleArguments(), options: UsageInfoOptions.Default | UsageInfoOptions.VerticallyExpandedOutput);
            GetUsageStringWorks(new SimpleArguments(), options: UsageInfoOptions.DefaultAbridged);
            GetUsageStringWorks(new SimpleArguments(), options: UsageInfoOptions.DefaultAbridged | UsageInfoOptions.VerticallyExpandedOutput);

            GetUsageStringWorks(new PositionalArguments());
        }

        [TestMethod]
        public void GetUsageInfoWorksEvenIfNoConsolePresent()
        {
            var originalGetWidth = CommandLineParser.GetConsoleWidth;

            try
            {
                CommandLineParser.GetConsoleWidth = () => { throw new IOException(); };

                var usageInfo = CommandLineParser.GetUsageInfo(typeof(SimpleArguments), UsageInfoOptions.Default);
                usageInfo.ToString().Should().NotBeNullOrWhiteSpace();
            }
            finally
            {
                CommandLineParser.GetConsoleWidth = originalGetWidth;
            }
        }

        [TestMethod]
        public void GetUsageStringThrowsWithVerySmallWidth()
        {
            var cl = new CommandLineParserEngine(typeof(SimpleArguments));

            Action getUsage = () => cl.GetUsageInfo(4, null, UsageInfoOptions.Default);
            getUsage.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void GetUsageStringWorkWithSmallButNotTinyWidth()
        {
            GetUsageStringWorks(new SimpleArguments(), 40);
        }

        [TestMethod]
        public void GetUsageStringWithAdditionalHelp()
        {
            var cl = new CommandLineParserEngine(typeof(AdditionalHelpArguments));
            var usageStr = cl.GetUsageInfo(80, null, UsageInfoOptions.Default).ToString();

            usageStr.Should().Contain("More help content.");
        }

        private static void GetUsageStringWorks<T>(T args, int width = 80, UsageInfoOptions? options = null)
        {
            var cl = new CommandLineParserEngine(typeof(T), args, null);

            var usageStr = cl.GetUsageInfo(width, null, options ?? UsageInfoOptions.Default).ToString();
            usageStr.Should().NotBeNullOrWhiteSpace();

            var narrowUsageStr = cl.GetUsageInfo(60, null, options ?? UsageInfoOptions.Default).ToString();
            narrowUsageStr.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public void InvalidArgumentsGetThrown()
        {
            var args = new SimpleArguments();

            Action parse0 = () => CommandLineParser.ParseWithUsage(null, args);
            parse0.ShouldThrow<ArgumentNullException>();

            Action parse1 = () => CommandLineParser.ParseWithUsage(new string[] { }, (SimpleArguments)null);
            parse1.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void MultiplePositionalArgumentsParseCorrectly()
        {
            var args = new MultiplePositionalArguments();
            CommandLineParser.ParseWithUsage(new[] { "foo", "bar" }, args).Should().BeTrue();
            args.Args.Should().NotBeNull();
            args.Args.Length.Should().Be(2);
            args.Args[0].Should().Be("foo");
            args.Args[1].Should().Be("bar");

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
            CommandLineParser.ParseWithUsage(new List<string>(), args, options, UsageInfoOptions.Default).Should().BeFalse();

            // Make sure the reported content contains an empty line followed by
            // generic usage info.
            var reported = reportedBuilder.ToString();
            reported.Should().Contain(Environment.NewLine + Environment.NewLine + "Usage:");
        }

        [TestMethod]
        public void DefaultRequiredArgumentNotGiven()
        {
            var args = new RequiredPositionalArguments();
            CommandLineParser.ParseWithUsage(new List<string>(), args).Should().BeFalse();
        }

        [TestMethod]
        public void UnknownOptionFailsToParse()
        {
            var args = new SimpleArguments();
            CommandLineParser.Parse(new[] { "/unknown:foo" }, args).Should().BeFalse();

            var args2 = new SimpleArguments();
            CommandLineParser.Parse(new[] { "unknown" }, args2).Should().BeFalse();
        }

        [TestMethod]
        public void RestOfLineAsOneString()
        {
            var args = new AllArgumentsAsPositionalArgumentString();
            CommandLineParser.Parse(new[] { "foo", "bar" }, args).Should().BeTrue();
            args.AllArguments.Should().Be("foo bar");

            CommandLineParser.Format(args).Should().Equal(new[] { "foo bar" });
        }

        [TestMethod]
        public void RestOfLineWithEmbeddedSpacesAsOneString()
        {
            var args = new AllArgumentsAsPositionalArgumentString();
            CommandLineParser.Parse(new[] { "foo", "bar baz" }, args).Should().BeTrue();
            args.AllArguments.Should().Be("foo \"bar baz\"");

            CommandLineParser.Format(args).Should().Equal("foo \"bar baz\"");
        }

        [TestMethod]
        public void RestOfLineAsOneNamedString()
        {
            var args = new AllArgumentsAsArgumentString();
            CommandLineParser.Parse(new[] { "/AllArguments:foo", "bar" }, args).Should().BeTrue();
            args.AllArguments.Should().Be("foo bar");

            CommandLineParser.Format(args).Should().Equal("/AllArguments=foo bar");
        }

        [TestMethod]
        public void RestOfLineAsArrayContainingQuestionMark()
        {
            var args = new AllArgumentsAsArray();
            CommandLineParser.Parse(new[] { "foo", "/?" }, args).Should().BeTrue();
            args.AllArguments.Should().NotBeNull();
            args.AllArguments.Length.Should().Be(2);
            args.AllArguments[0].Should().Be("foo");
            args.AllArguments[1].Should().Be("/?");
        }

        [TestMethod]
        public void RestOfLineAsStringContainingQuestionMark()
        {
            var args = new AllArgumentsAsPositionalArgumentString();
            CommandLineParser.Parse(new[] { "foo", "/?" }, args).Should().BeTrue();
            args.AllArguments.Should().Be("foo /?");
        }

        [TestMethod]
        public void RestOfLineAsArrayContainingTokensThatLookLikeOptions()
        {
            var args = new AllArgumentsAsArray();
            CommandLineParser.Parse(new[] { "foo", "-bar+", "/foo=baz" }, args).Should().BeTrue();
            args.AllArguments.Should().NotBeNull();
            args.AllArguments.Length.Should().Be(3);
            args.AllArguments[0].Should().Be("foo");
            args.AllArguments[1].Should().Be("-bar+");
            args.AllArguments[2].Should().Be("/foo=baz");
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
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            args.Argument.Should().Be(10);

            CommandLineParser.Format(args).Should().BeEmpty();
        }

        [TestMethod]
        public void CoerceableDefaultValueWorks()
        {
            var args = new ArgumentsWithCoerceableDefaultValue();
            CommandLineParser.Parse(Array.Empty<string>(), args).Should().BeTrue();
            args.Argument.Should().Be(1U);
        }

        [TestMethod]
        public void AlreadySetValueIsIgnoredByDefault()
        {
            var args = new ArgumentsWithDefaultValue { Argument = 17 };
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            args.Argument.Should().Be(10);

            CommandLineParser.Format(args).Should().BeEmpty();
        }

        [TestMethod]
        public void DynamicDefaultValueIsObserved()
        {
            var args = new ArgumentsWithDynamicDefaultValue { Argument = 17 };
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            args.Argument.Should().Be(17);

            CommandLineParser.Format(args).Should().Equal("/Argument=17");
        }

        [TestMethod]
        public void EmptyCommandLineParsesOkay()
        {
            var args = new SimpleArguments();
            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            CommandLineParser.Format(args).Should().BeEmpty();
        }

        [TestMethod]
        public void EmptyOptionIsTreatedAsUnknownArgument()
        {
            var args = new SimpleArguments();
            CommandLineParser.Parse(new[] { "/" }, args).Should().BeFalse();

            var args2 = new AllArgumentsAsPositionalArgumentString();
            CommandLineParser.Parse(new[] { "/", "a" }, args2).Should().BeFalse();
        }

        [TestMethod]
        public void UnsupportedFieldTypesThrow()
        {
            Action readOnlyField = () => CommandLineParser.Parse(new string[] { }, new ReadOnlyFieldArguments());
            readOnlyField.ShouldThrow<InvalidArgumentSetException>();

            Action constField = () => CommandLineParser.Parse(new string[] { }, new ConstFieldArguments());
            constField.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void PrivateFieldArgument()
        {
            var args = new PrivateFieldArguments();

            args.PrivateArgument.Should().Be(0);
            CommandLineParser.Parse(new[] { "/argument=7" }, args).Should().BeTrue();
            args.PrivateArgument.Should().Be(7);
        }

        [TestMethod]
        public void StaticFieldArgument()
        {
            var args = new StaticFieldArguments();

            StaticFieldArguments.Value.Should().Be(0);
            CommandLineParser.Parse(new[] { "/value=11" }, args).Should().BeTrue();
            StaticFieldArguments.Value.Should().Be(11);
        }

        [TestMethod]
        public void DerivedClassArguments()
        {
            var args = new DerivedArguments();

            CommandLineParser.Parse(new[] { "/DerivedMyString=7", "/MyString=xyzzy", "/MyStringProperty=abcd" }, args).Should().BeTrue();
            args.MyString.Should().Be("7");
            ((SimpleArguments)args).MyString.Should().Be("xyzzy");
            args.MyStringProperty.Should().Be("abcd");
        }

        [TestMethod]
        public void ParseIntoValueType()
        {
            var args = new ValueTypeArguments();

            CommandLineParser.ParseWithUsage(new[] { "/va" }, ref args).Should().BeFalse();
            CommandLineParser.Parse(new[] { "/va" }, ref args).Should().BeFalse();

            CommandLineParser.ParseWithUsage(new string[] { }, ref args).Should().BeTrue();
            args.Value.Should().Be(0);

            CommandLineParser.Parse(new[] { "/value=17" }, ref args).Should().BeTrue();
            args.Value.Should().Be(17);
        }

        [TestMethod]
        public void NoAttributesInArguments()
        {
            var args = new NoAttributedArguments();

            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            CommandLineParser.Parse(new[] { "/value=7" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void OverriddenShortName()
        {
            var args = new OverriddenShortNameArguments();

            CommandLineParser.Parse(new[] { "/a=7" }, args).Should().BeTrue();
            args.Argument.Should().Be(0);
            args.OtherArgument.Should().Be(7);
        }

        [TestMethod]
        public void EmptyShortName()
        {
            var args = new EmptyShortNameArguments();

            Action parseFunc = () => CommandLineParser.Parse(new string[] { }, args);
            parseFunc.ShouldNotThrow();
        }

        [TestMethod]
        public void DuplicateLongNames()
        {
            var args = new DuplicateLongNameArguments();

            Action parseFunc = () => CommandLineParser.Parse(new string[] { }, args);
            parseFunc.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void NoAvailableShortNames()
        {
            var args = new NoAvailableShortNameArguments();

            CommandLineParser.Parse(new[] {"/F=42"}, args).Should().BeTrue();
            args.F.Should().Be(42);
            args.Foo.Should().Be(0);
        }

        [TestMethod]
        public void DuplicateShortNames()
        {
            var args = new DuplicateShortNameArguments();

            Action parseFunc = () => CommandLineParser.Parse(new string[] { }, args);
            parseFunc.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void PropertyArgument()
        {
            var args = new PropertyArguments();

            args.Value.Should().Be(0);
            CommandLineParser.Parse(new[] { "/value=12" }, args).Should().BeTrue();
            args.Value.Should().Be(12);
        }

        [TestMethod]
        public void UnsettablePropertyArgument()
        {
            Action parse = () => CommandLineParser.Parse(new string[] { }, new UnsettablePropertyArguments());
            parse.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void DuplicatePositionThrows()
        {
            Action parse = () => CommandLineParser.Parse(new string[] { }, new SamePositionArguments());
            parse.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void NoZeroPositionThrows()
        {
            Action parse = () => CommandLineParser.Parse(new string[] { }, new NoZeroPositionArguments());
            parse.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void PositionPlusRestOfLineAsStringArgumentsTest()
        {
            var args = new PositionPlusRestOfLineArguments<string>();
            CommandLineParser.Parse(new[] { "10", "bar", "baz" }, args).Should().BeTrue();

            args.Value.Should().Be(10);
            args.RestOfLine.Should().Be("bar baz");
        }

        [TestMethod]
        public void PositionPlusRestOfLineAsStringArrayArgumentsTest()
        {
            var args = new PositionPlusRestOfLineArguments<string[]>();
            CommandLineParser.Parse(new[] { "10", "bar", "baz" }, args).Should().BeTrue();

            args.Value.Should().Be(10);
            args.RestOfLine.Should().ContainInOrder("bar", "baz");
        }

        [TestMethod]
        public void NamedPlusRestOfLineAsStringArgumentsTest()
        {
            var args = new NamedPlusRestOfLineArguments<string>();
            CommandLineParser.Parse(new[] { "/Value=10", "bar", "baz" }, args).Should().BeTrue();

            args.Value.Should().Be(10);
            args.RestOfLine.Should().Be("bar baz");
        }

        [TestMethod]
        public void NamedPlusRestOfLineAsStringArrayArgumentsTest()
        {
            var args = new NamedPlusRestOfLineArguments<string[]>();
            CommandLineParser.Parse(new[] { "/Value=10", "bar", "baz" }, args).Should().BeTrue();

            args.Value.Should().Be(10);
            args.RestOfLine.Should().ContainInOrder("bar", "baz");
        }

        [TestMethod]
        public void RestOfLinePlusPositionalArgumentThrows()
        {
            Action parse = () => CommandLineParser.Parse(new string[] { }, new RestOfLinePlusPositionArguments());
            parse.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void MultiplePositionalArgumentsPlusPositionArgumentsThrows()
        {
            Action parse = () => CommandLineParser.Parse(new string[] { }, new MultiplePositionalArgumentsPlusPositionArguments());
            parse.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void PositionalArgumentsWork()
        {
            var args = new PositionalArguments();

            CommandLineParser.Parse(new[] { "foo", "9" }, args).Should().BeTrue();
            args.Value0.Should().Be("foo");
            args.Value1.Should().Be(9);
        }

        [TestMethod]
        public void StringListParsesOkay()
        {
            var args = new ListArguments();
            CommandLineParser.Parse(new[] { "/value=a", "/value=b" }, args).Should().Be(true);

            args.Values[0].Should().Be("a");
            args.Values[1].Should().Be("b");
        }

        [TestMethod]
        public void StringIListDoesNotParseOkay()
        {
            var args = new IListArguments();

            Action parse = () => CommandLineParser.Parse(new[] { "/value=a", "/value=b" }, args);
            parse.ShouldThrow<Exception>();
        }

        [TestMethod]
        public void StringListPropertyParsesOkay()
        {
            var args = new ListPropertyArguments();
            CommandLineParser.Parse(new[] { "/value=a", "/value=b" }, args).Should().BeTrue();

            args.Values[0].Should().Be("a");
            args.Values[1].Should().Be("b");
        }

        [TestMethod]
        public void CustomPropertyParsesOkay()
        {
            var args = new CustomPropertyArguments();

            CommandLineParser.Parse(new[] { "/value=a" }, args).Should().BeTrue();
            args.Value.Should().Be("a");

            CommandLineParser.Parse(new[] { "/value= a bc " }, args).Should().BeTrue();
            args.Value.Should().Be("abc");

            CommandLineParser.Parse(new[] { "/value= " }, args).Should().BeFalse();
        }

        [TestMethod]
        public void NotImplPropertyExceptionIsNotCaught()
        {
            var args = new ThrowingStringPropertyArguments<NotImplementedException>();

            Action parse = () => CommandLineParser.Parse(new[] { "/value=a" }, args);
            parse.ShouldThrow<NotImplementedException>();
        }

        [TestMethod]
        public void ArgOutOfRangeStringPropertyExceptionIsNotCaught()
        {
            var args = new ThrowingStringPropertyArguments<ArgumentOutOfRangeException>();
            CommandLineParser.Parse(new[] { "/value=a" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void ArgOutOfRangeGuidPropertyExceptionIsNotCaught()
        {
            var args = new ThrowingGuidPropertyArguments<ArgumentOutOfRangeException>();
            CommandLineParser.Parse(new[] { "/value=8496ade9-c703-4bce-afc2-b98b63e4fa86" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void PropertyWithoutGetThrows()
        {
            var args = new PropertyWithoutGetArguments();

            Action parse = () => CommandLineParser.Parse(new[] { "/value=a" }, args);
            parse.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void UnknownConflictingArgumentNameThrows()
        {
            var args = new UnknownConflictingArguments();

            Action parse = () => CommandLineParser.Parse(new[] { "/value=a" }, args);
            parse.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void SelfConflictingArgumentThrows()
        {
            var args = new SelfConflictingArguments();

            Action parse = () => CommandLineParser.Parse(new[] { "/value=a" }, args);
            parse.ShouldThrow<ArgumentException>();
        }

        [TestMethod]
        public void ConflictingArgumentsWorkAsExpected()
        {
            var argsList = new object[] { new ConflictingArguments(), new PartlySpecifiedConflictingArguments() };

            foreach (var args in argsList)
            {
                CommandLineParser.Parse(new[] { "/value=a" }, args).Should().BeTrue();
                CommandLineParser.Parse(new[] { "/othervalue=b" }, args).Should().BeTrue();
                CommandLineParser.Parse(new[] { "/value=a", "/othervalue=b" }, args).Should().BeFalse();
            }
        }

        [TestMethod]
        public void FieldDefaultValueOfWrongType()
        {
            var args = new FieldDefaultValueOfWrongTypeArguments();
            Action parse = () => CommandLineParser.Parse(new string[] { }, args);
            parse.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void PropertyDefaultValueOfWrongType()
        {
            var args = new PropertyDefaultValueOfWrongTypeArguments();
            Action parse = () => CommandLineParser.Parse(new string[] { }, args);
            parse.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void DefaultValueOfImplicitlyConvertibleType()
        {
            var args = new DefaultValueOfImplicitlyConvertibleTypeArguments();
            CommandLineParser.Parse(Array.Empty<string>(), args).Should().BeTrue();

            args.Value.Should().Be(10);
            args.ValueProp.Should().Be(10);

            args.Path.Path.Should().Be(@"c:\myfile.txt");
            args.PathProp.Path.Should().Be(@"c:\myfile.txt");
        }

        [TestMethod]
        public void ZeroLengthLongNameThrows()
        {
            var args = new ZeroLengthLongNameArguments();
            Action parse = () => CommandLineParser.Parse(new string[] { }, args);
            parse.ShouldThrow<Exception>();
        }

        [TestMethod]
        public void FlagEnumArgs()
        {
            var args = new FlagEnumArguments();

            CommandLineParser.Parse(new string[] { }, args).Should().BeTrue();
            args.Value.Should().Be(MyFlagsEnum.None);

            CommandLineParser.Parse(new[] { "/Value=FlagOne" }, args).Should().BeTrue();
            args.Value.Should().Be(MyFlagsEnum.FlagOne);

            CommandLineParser.Parse(new[] { "/Value=FlagOne|FlagTwo" }, args).Should().BeTrue();
            args.Value.Should().Be(MyFlagsEnum.FlagOne | MyFlagsEnum.FlagTwo);
        }

        [TestMethod]
        public void NonWritableUnannotatedMembersDoNotBecomeArguments()
        {
            var args = new UnannotatedArguments();
            CommandLineParser.Parse(new[] { "/NonWritableValue=7" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void ProtectedUnannotatedMembersDoNotBecomeArguments()
        {
            var args = new UnannotatedArguments();
            CommandLineParser.Parse(new[] { "/ProtectedValue=7" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void PrivateUnannotatedMembersDoNotBecomeArguments()
        {
            var args = new UnannotatedArguments();
            CommandLineParser.Parse(new[] { "/PrivateValue=7" }, args).Should().BeFalse();
        }

        [TestMethod]
        public void PublicUnannotatedMembersBecomeArguments()
        {
            var args = new UnannotatedArguments();
            CommandLineParser.Parse(Array.Empty<string>(), args).Should().BeTrue();
            args.StringValue.Should().BeNull();
            args.IntValue.Should().Be(0);

            args = new UnannotatedArguments();
            CommandLineParser.Parse(new[] {"/StringValue=x"}, args).Should().BeTrue();
            args.StringValue.Should().Be("x");
            args.IntValue.Should().Be(0);

            args = new UnannotatedArguments();
            CommandLineParser.Parse(new[] {"/IntValue=7"}, args).Should().BeTrue();
            args.StringValue.Should().BeNull();
            args.IntValue.Should().Be(7);

            args = new UnannotatedArguments();
            CommandLineParser.Parse(new[] {"/StringValue=x", "/IntValue=7"}, args).Should().BeTrue();
            args.StringValue.Should().Be("x");
            args.IntValue.Should().Be(7);
        }

        [TestMethod]
        public void GetLogoWorks()
        {
            var logo = CommandLineParser.GetLogo();
            logo.Should().NotBeNullOrWhiteSpace();
            logo.Should().EndWith(Environment.NewLine);
        }
    }
}
