using System;
using System.Globalization;
using NClap.Types;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute that indicates the associated file-system path argument
    /// member must name a directory that exists.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class MustNotExistAttribute : FileSystemPathValidationAttribute
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
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var path = (FileSystemPath)value;
            if (!context.FileSystemReader.FileExists(path) && !context.FileSystemReader.DirectoryExists(path))
            {
                reason = null;
                return true;
            }

            reason = string.Format(CultureInfo.CurrentCulture, Strings.PathExists);
            return false;
        }
    }
}
