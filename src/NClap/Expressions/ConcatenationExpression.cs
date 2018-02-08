using System;

namespace NClap.Expressions
{
    /// <summary>
    /// Concatenation expression.
    /// </summary>
    internal class ConcatenationExpression : Expression
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="left">Left-hand expression.</param>
        /// <param name="right">Right-hand expression.</param>
        public ConcatenationExpression(Expression left, Expression right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        /// <summary>
        /// Left-hand expression.
        /// </summary>
        public Expression Left { get; }

        /// <summary>
        /// Right-hand expression.
        /// </summary>
        public Expression Right { get; }

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

            if (!Left.TryEvaluate(env, out string left) ||
                !Right.TryEvaluate(env, out string right))
            {
                value = null;
                return false;
            }

            value = left + right;
            return true;
        }
    }
}
