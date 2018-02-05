using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Expressions;

namespace NClap.Tests.Expressions
{
    [TestClass]
    public class OperatorExpressionTests : ExpressionTests
    {
        private const Operator AnyValidOperator = Operator.ConvertToLowerCase;

        internal override Expression CreateInstance() =>
            new OperatorExpression(AnyValidOperator, AnyExpression);

        [TestMethod]
        public void TestThatConstructorThrowsOnNullExpressions()
        {
            Action a = () => new OperatorExpression(AnyValidOperator, null);
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatConstructorThrowsOnUnspecifiedOperator()
        {
            Action a = () => new OperatorExpression(Operator.Unspecified, AnyExpression);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestThatInnerExpressionCanBeRetrieved()
        {
            var operand = new StringLiteral("operand");

            var expr = new OperatorExpression(AnyValidOperator, operand);
            expr.Operator.Should().Be(AnyValidOperator);
            expr.Operand.Should().BeSameAs(operand);
        }

        [TestMethod]
        public void TestThatLowerCaseOperatorWorksAsExpected()
        {
            TestThatOperatorYields(Operator.ConvertToLowerCase, "FOO", "foo");
            TestThatOperatorYields(Operator.ConvertToLowerCase, "foo", "foo");
            TestThatOperatorYields(Operator.ConvertToLowerCase, "fO1o", "fo1o");
            TestThatOperatorYields(Operator.ConvertToLowerCase, string.Empty, string.Empty);
        }

        [TestMethod]
        public void TestThatUpperCaseOperatorWorksAsExpected()
        {
            TestThatOperatorYields(Operator.ConvertToUpperCase, "foo", "FOO");
            TestThatOperatorYields(Operator.ConvertToUpperCase, "FOO", "FOO");
            TestThatOperatorYields(Operator.ConvertToUpperCase, "fO1o", "FO1O");
            TestThatOperatorYields(Operator.ConvertToUpperCase, string.Empty, string.Empty);
        }

        [TestMethod]
        public void TestThatEvalFailsIfInnerExpressionCannotBeEvaluated()
        {
            var concat = new OperatorExpression(AnyValidOperator, AnyUnevaluatableExpr);
            concat.TryEvaluate(EmptyEnvironment, out string value).Should().BeFalse();
        }

        private void TestThatOperatorYields(Operator op, string input, string expectedResult)
        {
            var operand = new StringLiteral(input);
            var expr = new OperatorExpression(op, operand);

            expr.TryEvaluate(EmptyEnvironment, out string actualResult).Should().BeTrue();
            actualResult.Should().Be(expectedResult);
        }
    }
}
