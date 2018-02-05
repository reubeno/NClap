namespace NClap.Expressions
{
    /// <summary>
    /// An expression environment.
    /// </summary>
    internal abstract class ExpressionEnvironment
    {
        /// <summary>
        /// Tries to retrieve the value associated with the given
        /// variable.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="value">On success, receives the value associated
        /// with the variable.</param>
        /// <returns>true if the variable was found; false otherwise.</returns>
        public abstract bool TryGetVariable(string variableName, out string value);
    }
}