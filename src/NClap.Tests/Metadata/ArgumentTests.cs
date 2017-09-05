using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Exceptions;
using NClap.Metadata;
using NClap.Parser;
using NClap.Utilities;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class ArgumentTests
    {
        public class StringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, AllowEmpty = true, ShortName = "v", DefaultValue = "def", HelpText = "Some value")]
            public string Value;
        }

        public class StringArgumentsThatMustBeNonEmpty
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, AllowEmpty = false, DefaultValue = "")]
            [MustNotBeEmpty]
            public string Value;
        }

        public class RestOfLineStringArguments
        {
            [PositionalArgument(ArgumentFlags.RestOfLine)]
            public string Value;
        }

        public class KeyValuePairArguments
        {
            [NamedArgument(ArgumentFlags.Required, ShortName = "w")]
            public KeyValuePair<int, int> Value;
        }

        public class StringArrayArguments
        {
            [NamedArgument(ArgumentFlags.Multiple, AllowEmpty = true, DefaultValue = new[] { "a", "b" })]
            public string[] Value;
        }

        public class StringArrayWithEmptyDefaultArguments
        {
            [NamedArgument(ArgumentFlags.Multiple, AllowEmpty = true, DefaultValue = new string[] {})]
            public string[] Value;
        }

        public enum TestEnum
        {
            First,
            Second,
            Third
        }

        public class EnumArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public TestEnum Value;
        }

        public class BoolArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public bool Value;
        }

        public class BoolArgumentsWithTrueDefault
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = true)]
            public bool Value;
        }

        public class EmptyLongNameArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, LongName = "")]
            public string Value;
        }

        public class TupleOfIntAndStringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public Tuple<int, string> Value;
        }

        public class ArgumentsWithUnsettableDefault
        {
            public bool underlyingValue;

            [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = true)]
            public bool Value
            {
                get => underlyingValue;
                set => throw new ArgumentOutOfRangeException();
            }
        }

        public class ArgumentsWithUnsettableCollectionDefault
        {
            public string[] underlyingValue;

            [NamedArgument(ArgumentFlags.Multiple, DefaultValue = new string[] { })]
            public string[] Value
            {
                get => underlyingValue;
                set => throw new ArgumentOutOfRangeException();
            }
        }

        public class DefaultedNamedArguments
        {
            [NamedArgument]
            public string ValueArgument;
        }

        [TestMethod]
        public void InvalidNamedArgumentPrefixes()
        {
            var arg = GetArgument(typeof(StringArguments), setAttrib: new ArgumentSetAttribute
            {
                NamedArgumentPrefixes = new string[] { },
            });

            Action getSyntaxHelpWithBogusSeparators = () => arg.GetSyntaxHelp();
            getSyntaxHelpWithBogusSeparators.ShouldThrow<NotSupportedException>();
        }

        [TestMethod]
        public void InvalidArgumentValueSeparators()
        {
            var arg = GetArgument(typeof(StringArguments), setAttrib: new ArgumentSetAttribute
            {
                ArgumentValueSeparators = new char[] { }
            });

            Action getSyntaxHelpWithBogusSeparators = () => arg.GetSyntaxHelp();
            getSyntaxHelpWithBogusSeparators.ShouldThrow<NotSupportedException>();
        }

        [TestMethod]
        public void StringArgument()
        {
            var arg = GetArgument(typeof(StringArguments));
            arg.EffectiveDefaultValue.Should().Be("def");
            arg.DefaultValue.Should().Be("def");
            arg.GetSyntaxHelp().Should().Be("[/Value[=<string>]]");
        }

        [TestMethod]
        public void NonEmptyStringArgument()
        {
            var arg = GetArgument(typeof(StringArgumentsThatMustBeNonEmpty));
            arg.EffectiveDefaultValue.Should().Be("");
            arg.DefaultValue.Should().Be("");
            arg.GetSyntaxHelp().Should().Be("[/Value=<string>]");
           
            var usageInfo = new ArgumentUsageInfo(arg);
            usageInfo.DefaultValue.Should().BeNull();
        }

        [TestMethod]
        public void RestOfLineArgument()
        {
            var arg = GetArgument(typeof(RestOfLineStringArguments));
            arg.EffectiveDefaultValue.Should().BeNull();
            arg.GetSyntaxHelp().Should().Be("[<Value : <string>>...]");
        }

        [TestMethod]
        public void KeyValuePairArgument()
        {
            var arg = GetArgument(typeof(KeyValuePairArguments));
            arg.EffectiveDefaultValue.Should().Be(new KeyValuePair<int, int>(0, 0));
            arg.GetSyntaxHelp().Should().Be("/Value=<int32>=<int32>");
        }

        [TestMethod]
        public void StringArrayArgument()
        {
            var arg = GetArgument(typeof(StringArrayArguments));

            var value = arg.EffectiveDefaultValue;
            value.Should().BeOfType(typeof(string[]));
            ((string[])value).Should().ContainInOrder("a", "b");

            arg.GetSyntaxHelp().Should().Be("[/Value[=<string>]]*");

            var usage = new ArgumentUsageInfo(arg);
            usage.DefaultValue.Should().Be("a b");
        }

        [TestMethod]
        public void StringArrayWithEmptyDefaultsArgument()
        {
            var arg = GetArgument(typeof(StringArrayWithEmptyDefaultArguments));

            var value = arg.EffectiveDefaultValue;
            value.Should().BeOfType(typeof(string[]));
            ((string[])value).Should().BeEmpty();

            arg.GetSyntaxHelp().Should().Be("[/Value[=<string>]]*");

            var usage = new ArgumentUsageInfo(arg);
            usage.DefaultValue.Should().BeNull();
        }

        [TestMethod]
        public void EnumArgument()
        {
            var arg = GetArgument(typeof(EnumArguments));
            arg.EffectiveDefaultValue.Should().Be(TestEnum.First);
            arg.GetSyntaxHelp().Should().Be("[/Value={First|Second|Third}]");
        }

        [TestMethod]
        public void BoolArgument()
        {
            var arg = GetArgument(typeof(BoolArguments));
            arg.EffectiveDefaultValue.Should().Be(false);
            arg.GetSyntaxHelp().Should().Be("[/Value]");
        }

        [TestMethod]
        public void BoolArgumentWithTrueDefault()
        {
            var arg = GetArgument(typeof(BoolArgumentsWithTrueDefault));
            arg.EffectiveDefaultValue.Should().Be(true);
            arg.GetSyntaxHelp().Should().Be("[/Value[={True | False}]]");
        }

        [TestMethod]
        public void EmptyLongNameArgument()
        {
            Action getArg = () => GetArgument(typeof(EmptyLongNameArguments));
            getArg.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void TupleOfIntAndStringArgument()
        {
            var arg = GetArgument(typeof(TupleOfIntAndStringArguments));
            arg.EffectiveDefaultValue.Should().BeNull();
            arg.GetSyntaxHelp().Should().Be("[/Value=<int32>,<string>]");
        }

        [TestMethod]
        public void ArgumentWithUnsettableDefault()
        {
            var arg = GetArgument(typeof(ArgumentsWithUnsettableDefault));
            arg.DefaultValue.Should().Be(true);
            arg.EffectiveDefaultValue.Should().Be(true);
            arg.TryFinalize(new ArgumentsWithUnsettableDefault(), FileSystemReader.Create()).Should().BeFalse();
        }

        [TestMethod]
        public void ArgumentWithUnsettableCollectionDefault()
        {
            var arg = GetArgument(typeof(ArgumentsWithUnsettableCollectionDefault));
            arg.DefaultValue.Should().BeOfType<string[]>();
            arg.TryFinalize(new ArgumentsWithUnsettableCollectionDefault(), FileSystemReader.Create()).Should().BeFalse();
        }

        [TestMethod]
        public void PreservedCaseShortNames()
        {
            var arg = GetArgument(typeof(DefaultedNamedArguments), fieldName: "ValueArgument");
            arg.ExplicitShortName.Should().BeFalse();
            arg.ShortName.Should().Be("V");
        }

        [TestMethod]
        public void LowercaseShortNames()
        {
            var arg = GetArgument(typeof(DefaultedNamedArguments), fieldName: "ValueArgument", setAttrib: new ArgumentSetAttribute
            {
                NameGenerationFlags = ArgumentNameGenerationFlags.PreferLowerCaseForShortNames
            });

            arg.ExplicitShortName.Should().BeFalse();
            arg.ShortName.Should().Be("v");
        }

        [TestMethod]
        public void PreservedCaseLongNames()
        {
            var arg = GetArgument(typeof(DefaultedNamedArguments), fieldName: "ValueArgument");
            arg.LongName.Should().Be("ValueArgument");
        }

        [TestMethod]
        public void LowercaseAndHyphenatedLongNames()
        {
            var arg = GetArgument(typeof(DefaultedNamedArguments), fieldName: "ValueArgument", setAttrib: new ArgumentSetAttribute
            {
                NameGenerationFlags = ArgumentNameGenerationFlags.GenerateHyphenatedLowerCaseLongNames
            });
            arg.LongName.Should().Be("value-argument");
        }

        internal static Argument GetArgument(Type type, string fieldName = "Value", ArgumentSetAttribute setAttrib = null)
        {
            var argMember = type.GetMember(fieldName)[0];
            var attrib = argMember.GetSingleAttribute<ArgumentBaseAttribute>();
            attrib.Should().NotBeNull();

            var options = new CommandLineParserOptions
            {
                FileSystemReader = FileSystemReader.Create(),
                Reporter = err => { }
            };

            IMutableMemberInfo mutableMemberInfo;
            switch (argMember)
            {
            case FieldInfo fi:
                mutableMemberInfo = new MutableFieldInfo(fi);
                break;
            case PropertyInfo pi:
                mutableMemberInfo = new MutablePropertyInfo(pi);
                break;
            default:
                throw new NotSupportedException();
            }

            return new Argument(mutableMemberInfo, attrib, setAttrib ?? new ArgumentSetAttribute(), options);
        }
    }
}
