using System.Collections.Generic;

namespace NClap.Expressions
{
    /// <summary>
    /// An expression environment.
    /// </summary>
    internal class TestEnvironment : NClap.Expressions.ExpressionEnvironment
    {
        private Dictionary<string, string> _variables = new Dictionary<string, string>();

        /// <summary>
        /// Associates the given variable with the given value. If the
        /// variable is already defined, then the existing association
        /// will be replaced with this new one.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="value">Value to associate with the variable.</param>
        public void Define(string variableName, string value)
        {
            _variables[variableName] = value;
        }

        /// <summary>
        /// Tries to retrieve the value associated with the given
        /// variable.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="value">On success, receives the value associated
        /// with the variable.</param>
        /// <returns>true if the variable was found; false otherwise.</returns>
        public override bool TryGetVariable(string variableName, out string value) =>
            _variables.TryGetValue(variableName, out value);
    }
}