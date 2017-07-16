using System;
using System.Globalization;
using NClap.Types;

namespace NClap.Metadata
{
    /// <summary>
    /// Describes a path's existence.
    /// </summary>
    [Flags]
    public enum PathExists
    {
        /// <summary>
        /// The path exists as a file.
        /// </summary>
        AsFile = 0x1,

        /// <summary>
        /// The path exists as a directory.
        /// </summary>
        AsDirectory = 0x2,

        /// <summary>
        /// The path exists as a file or directory.
        /// </summary>
        AsFileOrDirectory = AsFile | AsDirectory
    }

    /// <summary>
    /// Attribute that indicates the associated file-system path argument
    /// member must name a file or directory that exists.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class MustExistAttribute : FileSystemPathValidationAttribute
    {
        /// <summary>
        /// Constructs a new attribute instance.
        /// </summary>
        /// <param name="exists">Required existence.</param>
        public MustExistAttribute(PathExists exists)
        {
            Exists = exists;
        }

        /// <summary>
        /// Flags controlling existence validation.
        /// </summary>
        public PathExists Exists { get; }

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

            var path = (value as FileSystemPath) ?? (string)value;

            var exists = false;

            if (Exists.HasFlag(PathExists.AsFile))
            {
                exists = exists || context.FileSystemReader.FileExists(path);
            }

            if (Exists.HasFlag(PathExists.AsDirectory))
            {
                exists = exists || context.FileSystemReader.DirectoryExists(path);
            }

            if (exists)
            {
                reason = null;
                return true;
            }

            string reasonMessage;
            switch (Exists)
            {
                case PathExists.AsFile:
                    reasonMessage = Strings.FileDoesNotExist;
                    break;
                case PathExists.AsDirectory:
                    reasonMessage = Strings.DirectoryDoesNotExist;
                    break;
                case PathExists.AsFileOrDirectory:
                default:
                    reasonMessage = Strings.PathDoesNotExist;
                    break;
            }

            reason = string.Format(CultureInfo.CurrentCulture, reasonMessage);
            return false;
        }
    }
}
