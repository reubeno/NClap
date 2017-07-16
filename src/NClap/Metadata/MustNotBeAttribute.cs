using System.Globalization;
using NClap.Types;
using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute that indicates the associated string argument member cannot be
    /// empty.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class MustNotBeAttribute : ArgumentValidationAttribute
    {
        /// <summary>
        /// Constructs a new validation attribute that requires the associated
        /// argument's value to not be equal to the specified value.
        /// </summary>
        /// <param name="value">The value that the argument may not equal.
        /// </param>
        public MustNotBeAttribute(object value)
        {
            Value = value;
        }

        /// <summary>
        /// The value that this attribute checks against.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Checks if this validation attributes accepts values of the specified
        /// type.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>True if this attribute accepts values of the specified
        /// type; false if not.</returns>
        public override bool AcceptsType(IArgumentType type) => true;

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
        public override bool TryValidate(ArgumentValidationContext context, object value, out string reason)
        {
            if (!Value.Equals(value))
            {
                reason = null;
                return true;
            }

            reason = string.Format(CultureInfo.CurrentCulture, Strings.ValueMayNotBe, Value);
            return false;
        }
    }
}
