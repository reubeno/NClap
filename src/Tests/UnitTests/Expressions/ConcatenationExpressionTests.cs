using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Expressions;

namespace NClap.Tests.Expressions
{
    [TestClass]
    public class ConcatenationExpressionTests : ExpressionTests
    {
        internal override Expression CreateInstance() =>
            new ConcatenationExpression(AnyExpression, AnyExpression);

        [TestMethod]
        public void TestThatConstructorThrowsOnNullExpressions()
        {
            Action a = () => new ConcatenationExpression(null, AnyExpression);
            a.Should().Throw<ArgumentNullException>();

            a = () => new ConcatenationExpression(AnyExpression, null);
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatInnerExpressionsCanBeRetrieved()
        {
            var left = new StringLiteral("left");
            var right = new StringLiteral("right");

            var concat = new ConcatenationExpression(left, right);
            concat.Left.Should().BeSameAs(left);
            concat.Right.Should().BeSameAs(right);
        }

        [TestMethod]
        public void TestThatStringsConcatenateCorrectly()
        {
            TestThatConcatenationYields(string.Empty, "foo", "foo");
            TestThatConcatenationYields("foo", "right", "fooright");
            TestThatConcatenationYields("foo", string.Empty, "foo");
        }

        [TestMethod]
        public void TestThatEvalFailsIfEitherExpressionCannotBeEvaluated()
        {
            var concat = new ConcatenationExpression(AnyUnevaluatableExpr, AnyEvaluatableExpr);
            concat.TryEvaluate(EmptyEnvironment, out string value).Should().BeFalse();

            concat = new ConcatenationExpression(AnyEvaluatableExpr, AnyUnevaluatableExpr);
            concat.TryEvaluate(EmptyEnvironment, out value).Should().BeFalse();
        }

        private void TestThatConcatenationYields(string left, string right, string expectedResult)
        {
            var leftExpr = new StringLiteral(left);
            var rightExpr = new StringLiteral(right);
            var concatExpr = new ConcatenationExpression(leftExpr, rightExpr);

            concatExpr.TryEvaluate(EmptyEnvironment, out string actualResult).Should().BeTrue();
            actualResult.Should().Be(expectedResult);
        }
    }
}
