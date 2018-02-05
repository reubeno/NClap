using System;
using NClap.Utilities;

namespace NClap.Expressions
{
    /// <summary>
    /// Conditional expression.
    /// </summary>
    internal class ConditionalExpression : Expression
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="condition">Condition expression.</param>
        /// <param name="thenExpr">Then expression. Required.</param>
        /// <param name="elseExpression">Else expression. Optional.</param>
        public ConditionalExpression(Expression condition, Expression thenExpr, Maybe<Expression> elseExpression)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            ThenExpression = thenExpr ?? throw new ArgumentNullException(nameof(thenExpr));
            ElseExpression = elseExpression;
        }

        /// <summary>
        /// Condition expression.
        /// </summary>
        public Expression Condition { get; }

        /// <summary>
        /// Then expression.
        /// </summary>
        public Expression ThenExpression { get; }

        /// <summary>
        /// Else expression.
        /// </summary>
        public Maybe<Expression> ElseExpression { get; }

        /// <summary>
        /// Tries to evaluate the given expression in the context of
        /// the given environment.
        /// </summary>
        /// <param name="env">Environment in which to evaluate the
        /// expression.</param>
        /// <param name="value">On success, receives the evaluation
        /// result.</param>
        /// <returns>true on success; false otherwise.</returns>
        public override bool TryEvaluate(ExpressionEnvironment env, out string value)
        {
            if (env == null) throw new ArgumentNullException(nameof(env));

            if (!Condition.TryEvaluate(env, out string condition) ||
                !ThenExpression.TryEvaluate(env, out string thenValue))
            {
                value = null;
                return false;
            }

            string elseValue = string.Empty;
            if (ElseExpression.HasValue &&
                !ElseExpression.Value.TryEvaluate(env, out elseValue))
            {
                value = null;
                return false;
            }

            var conditionValue = !string.IsNullOrEmpty(condition);

            value = conditionValue ? thenValue : elseValue;
            return true;
        }
    }
}
