using System;
using System.Linq;

namespace NClap.Expressions
{
    /// <summary>
    /// A variable expression.
    /// </summary>
    internal class VariableExpression : Expression
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="variableName">Variable name.</param>
        public VariableExpression(string variableName)
        {
            if (variableName == null) throw new ArgumentNullException(nameof(variableName));
            if (!IsValidVariableName(variableName)) throw new ArgumentOutOfRangeException(nameof(variableName));

            VariableName = variableName;
        }

        /// <summary>
        /// Checks if the given name is valid for a variable.
        /// </summary>
        /// <param name="variableName">Possible name.</param>
        /// <returns>true if it's valid; false otherwise.</returns>
        public static bool IsValidVariableName(string variableName) =>
            !string.IsNullOrEmpty(variableName) &&
            variableName.All(IsValidInVariableName);

        /// <summary>
        /// Checks if the given character is valid in the name of a
        /// variable.
        /// </summary>
        /// <param name="c">Character to check.</param>
        /// <returns>true if it's valid; false otherwise.</returns>
        public static bool IsValidInVariableName(char c) =>
            char.IsLetterOrDigit(c);

        /// <summary>
        /// Variable name.
        /// </summary>
        public string VariableName { get; }

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

            if (env.TryGetVariable(VariableName, out value))
            {
                return true;
            }

            value = string.Empty;
            return true;
        }
    }
}
