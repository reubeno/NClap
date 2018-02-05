using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Expressions;

namespace NClap.Tests.Expressions
{
    [TestClass]
    public class VariableExpressionTests : ExpressionTests
    {
        [TestMethod]
        public void TestThatConstructorThrowsOnNull()
        {
            Action a = () => new VariableExpression(null);
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatVariableNameIsCorrect()
        {
            const string anyString = "foo";
            var expr = new VariableExpression(anyString);
            expr.VariableName.Should().Be(anyString);
        }

        [TestMethod]
        public void TestThatEmptyStringIsNotValidVariableName()
        {
            TestThatIsNotValidName(string.Empty);
        }

        [TestMethod]
        public void TestThatNonAlphanumericStringsAreNotValidNames()
        {
            TestThatIsNotValidName("foo-foo");
            TestThatIsNotValidName("foo_foo");
            TestThatIsNotValidName("foo@foo");
            TestThatIsNotValidName("foo foo");
        }

        [TestMethod]
        public void TestThatAlphanumericStringsAreValidNames()
        {
            TestThatIsValidName("foo2");
            TestThatIsValidName("2foo");
            TestThatIsValidName("2foo2");
        }

        [TestMethod]
        public void TestThatUndefinedVariableEvaluatesToEmptyString()
        {
            const string anyString = "foo";
            var expr = new VariableExpression(anyString);
            expr.TryEvaluate(EmptyEnvironment, out string value).Should().BeTrue();
            value.Should().Be(string.Empty);
        }

        public void TestThatDefinedVariableEvaluatesToItsValue()
        {
            const string anyVariableName = "foo";
            const string anyValue = "value";

            var expr = new VariableExpression(anyVariableName);

            var env = EmptyEnvironment;
            env.Define(anyVariableName, anyValue);

            expr.TryEvaluate(env, out string value).Should().BeTrue();
            value.Should().Be(anyValue);
        }

        private void TestThatIsValidName(string name)
        {
            Action a = () => new VariableExpression(name);
            a.Should().NotThrow();
        }

        private void TestThatIsNotValidName(string name)
        {
            Action a = () => new VariableExpression(name);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        internal override Expression CreateInstance() =>
            new VariableExpression("foo");
    }
}
