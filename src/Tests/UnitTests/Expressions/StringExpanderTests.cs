using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Expressions;

namespace NClap.Tests.Expressions
{
    [TestClass]
    public class StringExpanderTests
    {
        private readonly ExpressionEnvironment _env = new TestEnvironment();

        [TestMethod]
        public void TestThatStringWithoutExpressionExpandsToItself()
        {
            const string anyString = "foo";
            TryExpand(anyString, out string result).Should().BeTrue();
            result.Should().Be(anyString);
        }

        [TestMethod]
        public void TestThatEmptyStringExpandsToItself()
        {
            TryExpand(string.Empty, out string result).Should().BeTrue();
            result.Should().Be(string.Empty);
        }

        [TestMethod]
        public void TestThatUnterminatedExpressionFailsExpansion()
        {
            TryExpand("something {", out string result).Should().BeFalse();
            result.Should().BeNull();

            TryExpand("something {\"foo\"", out result).Should().BeFalse();
            result.Should().BeNull();
        }

        [TestMethod]
        public void TestThatEmptyExpressionFailsExpansion()
        {
            TryExpand("{}", out string result).Should().BeFalse();
            result.Should().BeNull();
        }

        [TestMethod]
        public void TestThatInvalidExpressionFailsExpansion()
        {
            TryExpand("{INVALIDEXPRESSION}", out string result).Should().BeFalse();
            result.Should().BeNull();
        }

        [TestMethod]
        public void TestThatValidExpressionIsCorrectlyExpanded()
        {
            TryExpand("Hello {\"world\"}, foo", out string result).Should().BeTrue();
            result.Should().Be("Hello world, foo");
        }

        [TestMethod]
        public void TestThatMultipleExpansionsAreSupported()
        {
            TryExpand("{\"Hello\"}{\", \"} {\"world\"}!", out string result).Should().BeTrue();
            result.Should().Be("Hello,  world!");
        }

        private bool TryExpand(string value, out string result) =>
            StringExpander.TryExpand(_env, value, out result);
    }
}
