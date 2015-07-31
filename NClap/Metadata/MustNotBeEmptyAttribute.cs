using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute that indicates the associated string argument member cannot be
    /// empty.
    /// </summary>
    public sealed class MustNotBeEmptyAttribute : StringValidationAttribute
    {
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
            if (!string.IsNullOrEmpty(GetString(value)))
            {
                reason = null;
                return true;
            }

            reason = Strings.StringIsEmpty;
            return false;
        }
    }
}
