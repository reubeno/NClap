using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Expressions;

namespace NClap.Tests.Expressions
{
    public abstract class ExpressionTests
    {
        private const string AnyString = "test";

        internal readonly TestEnvironment EmptyEnvironment =
            new TestEnvironment();

        internal static readonly Expression AnyExpression = new StringLiteral(AnyString);
        internal static readonly string EvaluatedAnyExpression = AnyString;

        internal static readonly Expression AnyUnevaluatableExpr = new UnevaluatableExpression();
        internal static readonly Expression AnyEvaluatableExpr = new StringLiteral("foo");

        internal abstract Expression CreateInstance();

        class UnevaluatableExpression : Expression
        {
            public override bool TryEvaluate(NClap.Expressions.ExpressionEnvironment env, out string value)
            {
                value = null;
                return false;
            }
        }

        [TestMethod]
        public void TestThatEvaluationThrowsOnNullEnvironment()
        {
            var expr = CreateInstance();

            Action a = () => expr.TryEvaluate(null, out string value);
            a.Should().Throw<ArgumentNullException>();
        }
    }
}
