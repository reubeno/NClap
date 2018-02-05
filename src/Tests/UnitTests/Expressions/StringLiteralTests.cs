using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Expressions;

namespace NClap.Tests.Expressions
{
    [TestClass]
    public class StringLiteralTests : ExpressionTests
    {
        [TestMethod]
        public void TestThatConstructorThrowsOnNull()
        {
            Action a = () => new StringLiteral(null);
            a.Should().Throw<ArgumentNullException>();
        }
        
        [TestMethod]
        public void TestThatLiteralIsCorrect()
        {
            const string anyString = "foo";
            var literal = new StringLiteral(anyString);
            literal.Value.Should().Be(anyString);
        }

        [TestMethod]
        public void TestThatEmptyStringIsValidLiteral()
        {
            var literal = new StringLiteral(string.Empty);
            literal.Value.Should().Be(string.Empty);
        }

        [TestMethod]
        public void TestThatLiteralEvaluatesToInnerString()
        {
            const string anyString = "foo";
            var literal = new StringLiteral(anyString);
            literal.TryEvaluate(EmptyEnvironment, out string value).Should().BeTrue();
            value.Should().Be(anyString);
        }

        internal override Expression CreateInstance() =>
            new StringLiteral("foo");
    }
}
