using NClap.Types;

namespace NClap.Metadata
{
    /// <summary>
    /// Abstract base class for implementing argument validation attributes
    /// that inspect integers.
    /// </summary>
    public abstract class IntegerValidationAttribute : ArgumentValidationAttribute
    {
        /// <summary>
        /// Checks if this validation attributes accepts values of the specified
        /// type.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>True if this attribute accepts values of the specified
        /// type; false if not.</returns>
        public sealed override bool AcceptsType(IArgumentType type) =>
            type is IntegerArgumentType;

        /// <summary>
        /// Retrieves the the argument type associated with the provided integer
        /// value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The argument type.</returns>
        internal static IntegerArgumentType GetArgumentType(object value) =>
            (IntegerArgumentType)ArgumentType.GetType(value.GetType());
    }
}
