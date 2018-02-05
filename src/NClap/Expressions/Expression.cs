namespace NClap.Expressions
{
    /// <summary>
    /// Base class for all expressions.
    /// </summary>
    internal abstract class Expression
    {
        /// <summary>
        /// Tries to evaluate the given expression in the context of
        /// the given environment.
        /// </summary>
        /// <param name="env">Environment in which to evaluate the
        /// expression.</param>
        /// <param name="value">On success, receives the evaluation
        /// result.</param>
        /// <returns>true on success; false otherwise.</returns>
        public abstract bool TryEvaluate(ExpressionEnvironment env, out string value);
    }
}
