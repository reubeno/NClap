using System;

namespace NClap.Expressions
{
    /// <summary>
    /// A string literal expression.
    /// </summary>
    internal class StringLiteral : Expression
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="value">Literal string value.</param>
        public StringLiteral(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Literal string.
        /// </summary>
        public string Value { get; }

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

            value = Value;
            return true;
        }
    }
}
