using System;
using NClap.Types;

namespace NClap.Metadata
{
    /// <summary>
    /// Abstract base class for implementing argument validation attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public abstract class ArgumentValidationAttribute : Attribute
    {
        /// <summary>
        /// Checks if this validation attributes accepts values of the specified
        /// type.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>True if this attribute accepts values of the specified
        /// type; false if not.</returns>
        public abstract bool AcceptsType(IArgumentType type);

        /// <summary>
        /// Validate the provided value in accordance with the attribute's
        /// policy.
        /// </summary>
        /// <param name="context">Context for validation.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="reason">On failure, receives a user-readable string
        /// message explaining why the value is not valid.</param>
        /// <returns>True if the value passes validation; false otherwise.
        /// </returns>
        public abstract bool TryValidate(ArgumentValidationContext context, object value, out string reason);
    }
}
