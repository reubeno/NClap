using System;
using NClap.Exceptions;

namespace NClap.Expressions
{
    /// <summary>
    /// Operator expression.
    /// </summary>
    internal class OperatorExpression : Expression
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="op">Operator.</param>
        /// <param name="operand">Operand.</param>
        public OperatorExpression(Operator op, Expression operand)
        {
            if (op == Operator.Unspecified)
            {
                throw new ArgumentOutOfRangeException(nameof(op));
            }

            Operator = op;
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        }

        /// <summary>
        /// Operator type.
        /// </summary>
        public Operator Operator { get; }

        /// <summary>
        /// Operand expression.
        /// </summary>
        public Expression Operand { get; }

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

            if (!Operand.TryEvaluate(env, out string operand))
            {
                value = null;
                return false;
            }

            switch (Operator)
            {
                case Operator.ConvertToUpperCase:
                    value = operand.ToUpper();
                    return true;
                case Operator.ConvertToLowerCase:
                    value = operand.ToLower();
                    return true;
                case Operator.Unspecified:
                default:
                    throw new InternalInvariantBrokenException();
            }
        }
    }
}
