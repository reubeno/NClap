using System;

namespace NClap.Expressions
{
    /// <summary>
    /// Parenthesis expression.
    /// </summary>
    internal class ParenthesisExpression : Expression
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="innerExpression">Inner expression.</param>
        public ParenthesisExpression(Expression innerExpression)
        {
            InnerExpression = innerExpression ?? throw new ArgumentNullException(nameof(innerExpression));
        }

        /// <summary>
        /// Inner expression.
        /// </summary>
        public Expression InnerExpression { get; }

        /// <summary>
        /// Tries to evaluate the given expression in the context of
        /// the given environment.
        /// </summary>
        /// <param name="env">Environment in which to evaluate the
        /// expression.</param>
        /// <param name="value">On success, receives the evaluation
        /// result.</param>
        /// <returns>true on success; false otherwise.</returns>
        public override bool TryEvaluate(ExpressionEnvironment env, out string value) =>
            InnerExpression.TryEvaluate(env, out value);
    }
}
