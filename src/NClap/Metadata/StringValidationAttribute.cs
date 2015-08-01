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
        public sealed override bool AcceptsType(IArgumentType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return (type.Type == typeof(string)) ||
                   (type.Type == typeof(FileSystemPath));
        }

        /// <summary>
        /// Retrieves a string from the object, for use in validation.
        /// </summary>
        /// <param name="obj">The object to retrieve the string from.</param>
        /// <returns></returns>
        protected static string GetString(object obj)
        {
            object value;
            if (obj is string)
            {
                value = obj;
            }
            else
            {
                if (!typeof (string).TryConvertFrom(obj, out value))
                {
                    throw new InvalidCastException();
                }
            }

            return (string)value;
        }
    }
}
