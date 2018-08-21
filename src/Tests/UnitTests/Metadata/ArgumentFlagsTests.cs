using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Exceptions;
using NClap.Metadata;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class ArgumentFlagsTests
    {
        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class RequiredArguments<T>
        {
            [NamedArgument(ArgumentFlags.Required)]
            public T Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class AtMostOnceArguments<T>
        {
            [NamedArgument(ArgumentFlags.AtMostOnce)]
            public T Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class AtLeastOnceArguments<T>
        {
            [NamedArgument(ArgumentFlags.AtLeastOnce)]
            public T Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class MultipleArguments<T>
        {
            [NamedArgument(ArgumentFlags.Multiple)]
            public T Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class MultipleUniqueArguments<T>
        {
            [NamedArgument(ArgumentFlags.MultipleUnique)]
            public T Value;
        }

        [TestMethod]
        public void RequiredScalar()
        {
            ShouldFailToParse<RequiredArguments<string>>(Array.Empty<string>());
            ShouldParseAs<RequiredArguments<string>, string>(new[] { "/value:a" }, "a");
            ShouldFailToParse<RequiredArguments<string>>(new[] { "/value:a", "/value:b" });
            ShouldFailToParse<RequiredArguments<string>>(new[] { "/value:a", "/value:a" });
        }

        [TestMethod]
        public void AtMostOnceScalar()
        {
            ShouldParseAs<AtMostOnceArguments<string>, string>(Array.Empty<string>(), null);
            ShouldParseAs<AtMostOnceArguments<string>, string>(new[] { "/value:a" }, "a");
            ShouldFailToParse<AtMostOnceArguments<string>>(new[] { "/value:a", "/value:b" });
            ShouldFailToParse<AtMostOnceArguments<string>>(new[] { "/value:a", "/value:a" });
        }

        [TestMethod]
        public void AtLeastOnceScalar()
        {
            ShouldFailToParse<AtLeastOnceArguments<string>>(Array.Empty<string>());
            ShouldParseAs<AtLeastOnceArguments<string>, string>(new[] { "/value:a" }, "a");
            ShouldParseAs<AtLeastOnceArguments<string>, string>(new[] { "/value:a", "/value:b" }, "b");
            ShouldParseAs<AtLeastOnceArguments<string>, string>(new[] { "/value:a", "/value:a" }, "a");
        }

        [TestMethod]
        public void MultipleScalar()
        {
            ShouldParseAs<MultipleArguments<string>, string>(Array.Empty<string>(), null);
            ShouldParseAs<MultipleArguments<string>, string>(new[] { "/value:a" }, "a");
            ShouldParseAs<MultipleArguments<string>, string>(new[] { "/value:a", "/value:b" }, "b");
            ShouldParseAs<MultipleArguments<string>, string>(new[] { "/value:a", "/value:a" }, "a");
        }

        [TestMethod]
        public void MultipleUniqueScalar()
        {
            ShouldThrow<MultipleUniqueArguments<string>, InvalidArgumentSetException>(Array.Empty<string>());
            ShouldThrow<MultipleUniqueArguments<string>, InvalidArgumentSetException>(new[] { "/value:a" });
            ShouldThrow<MultipleUniqueArguments<string>, InvalidArgumentSetException>(new[] { "/value:a", "/value:b" });
            ShouldThrow<MultipleUniqueArguments<string>, InvalidArgumentSetException>(new[] { "/value:a", "/value:a" });
        }

        [TestMethod]
        public void MultipleUniqueCollection()
        {

            Parse(Array.Empty<string>(), out MultipleUniqueArguments<string[]> parsedArgs).Should().BeTrue();
            parsedArgs.Value.Should().BeEmpty();

            Parse(new[] { "/value:a" }, out parsedArgs).Should().BeTrue();
            parsedArgs.Value.Should().Equal("a");

            Parse(new[] { "/value:a", "/value:b" }, out parsedArgs).Should().BeTrue();
            parsedArgs.Value.Should().Equal("a", "b");

            Parse(new[] { "/value:a,b" }, out parsedArgs).Should().BeTrue();
            parsedArgs.Value.Should().Equal("a", "b");

            Parse(new[] { "/value:a", "/value:a" }, out parsedArgs).Should().BeFalse();
            Parse(new[] { "/value:a,a" }, out parsedArgs).Should().BeFalse();
        }

        private static void ShouldThrow<T, TException>(IEnumerable<string> args) 
            where T : class, new()
            where TException : Exception
        {
            Action parse = () => Parse(args, out T parsedArgs);
            parse.Should().Throw<TException>();
        }

        private static void ShouldFailToParse<T>(IEnumerable<string> args) where T : class, new()
        {
            ValidateParse<T, object>(args, false);
        }

        private static void ShouldParseAs<T, TField>(IEnumerable<string> args, TField expectedValue) where T : class, new()
        {
            ValidateParse<T, TField>(args, true, expectedValue);
        }

        private static void ValidateParse<T, TField>(IEnumerable<string> args, bool expectedParseResult, TField expectedValue = default(TField)) where T : class, new()
        {
            Parse(args, out T parsedArgs).Should().Be(expectedParseResult);
            if (expectedParseResult)
            {
                typeof(T).GetTypeInfo().GetField("Value").GetValue(parsedArgs).Should().Be(expectedValue);
            }
        }

        private static bool Parse<T>(IEnumerable<string> args, out T parsedArgs) where T : class, new() =>
            CommandLineParser.TryParse(args, new CommandLineParserOptions { DisplayUsageInfoOnError = false }, out parsedArgs);
    }
}
