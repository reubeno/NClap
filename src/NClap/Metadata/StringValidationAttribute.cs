using System;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Metadata
{
    /// <summary>
    /// Abstract base class for implementing argument validation attributes
    /// that inspect strings.
    /// </summary>
    public abstract class StringValidationAttribute : ArgumentValidationAttribute
    {
        /// <summary>
        /// Checks if this validation attributes accepts values of the specified
        /// type.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>True if this attribute accepts values of the specified
        /// type; false if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type" />
        /// is null.</exception>
        public sealed override bool AcceptsType(IArgumentType type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return (type.Type == typeof(string)) ||
                   type.Type.IsEffectivelySameAs(typeof(FileSystemPath));
        }

        /// <summary>
        /// Retrieves a string from the object, for use in validation.
        /// </summary>
        /// <param name="value">The object to retrieve the string from.</param>
        /// <returns></returns>
        protected static string GetString(object value)
        {
            object valueToCast;
            if (value is string)
            {
                valueToCast = value;
            }
            else
            {
                if (!typeof(string).TryConvertFrom(value, out valueToCast))
                {
                    throw new InvalidCastException();
                }
            }

            return (string)valueToCast;
        }
    }
}
