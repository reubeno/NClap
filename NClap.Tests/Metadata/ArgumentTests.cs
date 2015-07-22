using System;
using System.Collections.Generic;
using NClap.Utilities;
using NClap.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

using NClap.Parser;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class ArgumentTests
    {
        private static readonly ArgumentSetAttribute s_defaultSetAttribute = new ArgumentSetAttribute();

        public class StringArguments
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "v", DefaultValue = "def", HelpText = "Some value")]
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
            [NamedArgument(ArgumentFlags.Multiple, DefaultValue = new[] { "a", "b" })]
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

        [TestMethod]
        public void InvalidNamedArgumentPrefixes()
        {
            var arg = GetArgument(typeof(StringArguments));
            var attrib = new ArgumentSetAttribute
            {
                NamedArgumentPrefixes = new string[] { },
            };

            Action getSyntaxHelpWithBogusSeparators = () => arg.GetSyntaxHelp(attrib);
            getSyntaxHelpWithBogusSeparators.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void InvalidArgumentValueSeparators()
        {
            var arg = GetArgument(typeof(StringArguments));
            var attrib = new ArgumentSetAttribute
            {
                ArgumentValueSeparators = new char[] { }
            };

            Action getSyntaxHelpWithBogusSeparators = () => arg.GetSyntaxHelp(attrib);
            getSyntaxHelpWithBogusSeparators.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void StringArgument()
        {
            var arg = GetArgument(typeof(StringArguments));
            arg.EffectiveDefaultValue.Should().Be("def");
            arg.GetSyntaxHelp(s_defaultSetAttribute).Should().Be("[/Value[=<String>]]");
        }

        [TestMethod]
        public void RestOfLineArgument()
        {
            var arg = GetArgument(typeof(RestOfLineStringArguments));
            arg.EffectiveDefaultValue.Should().BeNull();
            arg.GetSyntaxHelp(s_defaultSetAttribute).Should().Be("[<Value>...]");
        }

        [TestMethod]
        public void KeyValuePairArgument()
        {
            var arg = GetArgument(typeof(KeyValuePairArguments));
            arg.EffectiveDefaultValue.Should().Be(new KeyValuePair<int, int>(0, 0));
            arg.GetSyntaxHelp(s_defaultSetAttribute).Should().Be("/Value=<Int32>=<Int32>");
        }

        [TestMethod]
        public void StringArrayArgument()
        {
            var arg = GetArgument(typeof(StringArrayArguments));

            var value = arg.EffectiveDefaultValue;
            value.Should().BeOfType(typeof(string[]));
            ((string[])value).Should().ContainInOrder("a", "b");

            arg.GetSyntaxHelp(s_defaultSetAttribute).Should().Be("[/Value[=<String>]]*");
        }

        [TestMethod]
        public void EnumArgument()
        {
            var arg = GetArgument(typeof(EnumArguments));
            arg.EffectiveDefaultValue.Should().Be(TestEnum.First);
            arg.GetSyntaxHelp(s_defaultSetAttribute).Should().Be("[/Value={First | Second | Third}]");
        }

        [TestMethod]
        public void BoolArgument()
        {
            var arg = GetArgument(typeof(BoolArguments));
            arg.EffectiveDefaultValue.Should().Be(false);
            arg.GetSyntaxHelp(s_defaultSetAttribute).Should().Be("[/Value[={True | False}]]");
        }

        [TestMethod]
        public void EmptyLongNameArgument()
        {
            Action getArg = () => GetArgument(typeof(EmptyLongNameArguments));
            getArg.ShouldThrow<NotSupportedException>();
        }

        [TestMethod]
        public void TupleOfIntAndStringArgument()
        {
            var arg = GetArgument(typeof(TupleOfIntAndStringArguments));
            arg.EffectiveDefaultValue.Should().BeNull();
            arg.GetSyntaxHelp(s_defaultSetAttribute).Should().Be("[/Value=<Int32>,<String>]");
        }

        private static Argument GetArgument(Type type)
        {
            var argField = type.GetField("Value");
            var attrib = argField.GetSingleAttribute<ArgumentBaseAttribute>();
            attrib.Should().NotBeNull();

            var options = new CommandLineParserOptions
            {
                FileSystemReader = FileSystemReader.Create(),
                Reporter = err => { }
            };

            return new Argument(new MutableFieldInfo(argField), attrib, options);
        }
    }
}
