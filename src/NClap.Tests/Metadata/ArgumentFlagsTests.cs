using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Exceptions;
using NClap.Metadata;
using NClap.Parser;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class ArgumentFlagsTests
    {
        public class RequiredArguments<T>
        {
            [NamedArgument(ArgumentFlags.Required)]
            public T Value;
        }

        public class AtMostOnceArguments<T>
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public T Value;
        }

        public class AtLeastOnceArguments<T>
        {
            [NamedArgument(ArgumentFlags.AtLeastOnce)]
            public T Value;
        }

        public class MultipleArguments<T>
        {
            [NamedArgument(ArgumentFlags.Multiple)]
            public T Value;
        }

        public class MultipleUniqueArguments<T>
        {
            [NamedArgument(ArgumentFlags.MultipleUnique)]
            public T Value;
        }

        [TestMethod]
        public void RequiredScalar()
        {
            ShouldFailToParse<RequiredArguments<string>>(new string[] { });
            ShouldParseAs<RequiredArguments<string>, string>(new[] { "/value:a" }, "a");
            ShouldFailToParse<RequiredArguments<string>>(new[] { "/value:a", "/value:b" });
            ShouldFailToParse<RequiredArguments<string>>(new[] { "/value:a", "/value:a" });
        }

        [TestMethod]
        public void AtMostOnceScalar()
        {
            ShouldParseAs<AtMostOnceArguments<string>, string>(new string[] { }, null);
            ShouldParseAs<AtMostOnceArguments<string>, string>(new[] { "/value:a" }, "a");
            ShouldFailToParse<AtMostOnceArguments<string>>(new[] { "/value:a", "/value:b" });
            ShouldFailToParse<AtMostOnceArguments<string>>(new[] { "/value:a", "/value:a" });
        }

        [TestMethod]
        public void AtLeastOnceScalar()
        {
            ShouldFailToParse<AtLeastOnceArguments<string>>(new string[] { });
            ShouldParseAs<AtLeastOnceArguments<string>, string>(new[] { "/value:a" }, "a");
            ShouldParseAs<AtLeastOnceArguments<string>, string>(new[] { "/value:a", "/value:b" }, "b");
            ShouldParseAs<AtLeastOnceArguments<string>, string>(new[] { "/value:a", "/value:a" }, "a");
        }

        [TestMethod]
        public void MultipleScalar()
        {
            ShouldParseAs<MultipleArguments<string>, string>(new string[] { }, null);
            ShouldParseAs<MultipleArguments<string>, string>(new[] { "/value:a" }, "a");
            ShouldParseAs<MultipleArguments<string>, string>(new[] { "/value:a", "/value:b" }, "b");
            ShouldParseAs<MultipleArguments<string>, string>(new[] { "/value:a", "/value:a" }, "a");
        }

        [TestMethod]
        public void MultipleUniqueScalar()
        {
            ShouldThrow<MultipleUniqueArguments<string>, InvalidArgumentSetException>(new string[] { });
            ShouldThrow<MultipleUniqueArguments<string>, InvalidArgumentSetException>(new[] { "/value:a" });
            ShouldThrow<MultipleUniqueArguments<string>, InvalidArgumentSetException>(new[] { "/value:a", "/value:b" });
            ShouldThrow<MultipleUniqueArguments<string>, InvalidArgumentSetException>(new[] { "/value:a", "/value:a" });
        }

        [TestMethod]
        public void MultipleUniqueCollection()
        {

            Parse(new string[] { }, out MultipleUniqueArguments<string[]> parsedArgs).Should().BeTrue();
            parsedArgs.Value.Length.Should().Be(0);

            Parse(new[] { "/value:a" }, out parsedArgs).Should().BeTrue();
            parsedArgs.Value.Length.Should().Be(1);
            parsedArgs.Value[0].Should().Be("a");

            Parse(new[] { "/value:a", "/value:b" }, out parsedArgs).Should().BeTrue();
            parsedArgs.Value.Length.Should().Be(2);
            parsedArgs.Value[0].Should().Be("a");
            parsedArgs.Value[1].Should().Be("b");

            Parse(new[] { "/value:a", "/value:a" }, out parsedArgs).Should().BeFalse();
        }

        private static void ShouldThrow<T, TException>(IEnumerable<string> args) 
            where T : new()
            where TException : Exception
        {
            Action parse = () => Parse(args, out T parsedArgs);
            parse.ShouldThrow<TException>();
        }

        private static void ShouldFailToParse<T>(IEnumerable<string> args) where T : new()
        {
            ValidateParse<T, object>(args, false);
        }

        private static void ShouldParseAs<T, TField>(IEnumerable<string> args, TField expectedValue) where T : new()
        {
            ValidateParse<T, TField>(args, true, expectedValue);
        }

        private static void ValidateParse<T, TField>(IEnumerable<string> args, bool expectedParseResult, TField expectedValue = default(TField)) where T : new()
        {

            Parse(args, out T parsedArgs).Should().Be(expectedParseResult);
            if (expectedParseResult)
            {
                typeof(T).GetTypeInfo().GetField("Value").GetValue(parsedArgs).Should().Be(expectedValue);
            }
        }

        private static bool Parse<T>(IEnumerable<string> args, out T parsedArgs) where T : new()
        {
            parsedArgs = new T();

            var parser = new CommandLineParserEngine(typeof(T));
            if (parser.Parse(args.ToList(), parsedArgs))
            {
                return true;
            }

            parsedArgs = default(T);
            return false;
        }
    }
}
