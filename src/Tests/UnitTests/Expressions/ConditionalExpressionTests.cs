using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Expressions;
using NClap.Utilities;

namespace NClap.Tests.Expressions
{
    [TestClass]
    public class ConditionalExpressionTests : ExpressionTests
    {
        internal override Expression CreateInstance() =>
            new ConditionalExpression(AnyExpression, AnyExpression, AnyExpression);

        [TestMethod]
        public void TestThatConstructorThrowsOnNullIfOrThenExpressions()
        {
            Action a = () => new ConditionalExpression(null, AnyExpression, AnyExpression);
            a.Should().Throw<ArgumentNullException>();

            a = () => new ConditionalExpression(AnyExpression, null, AnyExpression);
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatConstructorAllowMissingElseExpression()
        {
            var expr = new ConditionalExpression(AnyExpression, AnyExpression, new None());
            expr.ElseExpression.HasValue.Should().BeFalse();
        }

        [TestMethod]
        public void TestThatInnerExpressionsCanBeRetrieved()
        {
            var condExpr = new StringLiteral("cond");
            var thenExpr = new StringLiteral("then");
            var elseExpr = new StringLiteral("else");

            var cond = new ConditionalExpression(condExpr, thenExpr, elseExpr);
            cond.Condition.Should().BeSameAs(condExpr);
            cond.ThenExpression.Should().BeSameAs(thenExpr);
            cond.ElseExpression.HasValue.Should().BeTrue();
            cond.ElseExpression.Value.Should().BeSameAs(elseExpr);
        }

        [TestMethod]
        public void TestThatNonEmptyStringTakesThenCondition()
        {
            var condExpr = new StringLiteral("something-not-empty");
            var thenExpr = new StringLiteral("then");
            var elseExpr = new StringLiteral("else");

            var cond = new ConditionalExpression(condExpr, thenExpr, elseExpr);
            cond.TryEvaluate(EmptyEnvironment, out string value).Should().BeTrue();
            value.Should().Be(thenExpr.Value);
        }

        [TestMethod]
        public void TestThatEmptyStringTakesElseConditionWhenPresent()
        {
            var condExpr = new StringLiteral(string.Empty);
            var thenExpr = new StringLiteral("then");
            var elseExpr = new StringLiteral("else");

            var cond = new ConditionalExpression(condExpr, thenExpr, elseExpr);
            cond.TryEvaluate(EmptyEnvironment, out string value).Should().BeTrue();
            value.Should().Be(elseExpr.Value);
        }

        [TestMethod]
        public void TestThatEmptyStringConditionYieldsEmptyStringWhenNoElse()
        {
            var condExpr = new StringLiteral(string.Empty);
            var thenExpr = new StringLiteral("then");

            var cond = new ConditionalExpression(condExpr, thenExpr, new None());
            cond.TryEvaluate(EmptyEnvironment, out string value).Should().BeTrue();
            value.Should().Be(string.Empty);
        }

        [TestMethod]
        public void TestThatEvalFailsWhenAnyInnerExprCannotBeEvaluated()
        {
            var cond = new ConditionalExpression(AnyUnevaluatableExpr, AnyEvaluatableExpr, AnyEvaluatableExpr);
            cond.TryEvaluate(EmptyEnvironment, out string value).Should().BeFalse();

            cond = new ConditionalExpression(AnyEvaluatableExpr, AnyUnevaluatableExpr, AnyEvaluatableExpr);
            cond.TryEvaluate(EmptyEnvironment, out value).Should().BeFalse();

            cond = new ConditionalExpression(AnyEvaluatableExpr, AnyEvaluatableExpr, AnyUnevaluatableExpr);
            cond.TryEvaluate(EmptyEnvironment, out value).Should().BeFalse();
        }
    }
}
