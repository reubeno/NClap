using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Expressions;

namespace NClap.Tests.Expressions
{
    [TestClass]
    public class ParenthesisExpressionTests : ExpressionTests
    {
        internal override Expression CreateInstance() =>
            new ParenthesisExpression(AnyExpression);

        [TestMethod]
        public void TestThatConstructorThrowsOnNullInnerExpression()
        {
            Action a = () => new ParenthesisExpression(null);
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatExpressionWrapsCorrectInnerExpression()
        {
            var expr = new ParenthesisExpression(AnyExpression);
            expr.InnerExpression.Should().BeSameAs(AnyExpression);
        }

        [TestMethod]
        public void TestThatExpressionEvaluatesToEvaluatedInnerExpression()
        {
            var expr = new ParenthesisExpression(AnyExpression);
            expr.TryEvaluate(EmptyEnvironment, out string value).Should().BeTrue();
            value.Should().Be(EvaluatedAnyExpression);
        }

        [TestMethod]
        public void TestThatEvalFailsWhenInnerExprCannotBeEvaluated()
        {
            var expr = new ParenthesisExpression(AnyUnevaluatableExpr);
            expr.TryEvaluate(EmptyEnvironment, out string value).Should().BeFalse();
        }
    }
}
