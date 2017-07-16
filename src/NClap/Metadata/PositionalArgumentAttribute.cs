using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Indicates that this argument is an (unnamed) positional argument.  The
    /// LongName property is used for usage text only and does not affect the
    /// usage of the argument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class PositionalArgumentAttribute : ArgumentBaseAttribute
    {
        /// <summary>
        /// Default constructor, which may be used to indicate an optional
        /// positional argument that may appear at most once.
        /// </summary>
        public PositionalArgumentAttribute() : this(ArgumentFlags.AtMostOnce)
        {
        }

        /// <summary>
        /// Indicates that this argument is a default, positional argument.
        /// </summary>
        /// <param name="flags">Specifies the error checking to be done on the
        /// argument.</param>
        public PositionalArgumentAttribute(ArgumentFlags flags) : base(flags)
        {
        }

        /// <summary>
        /// The zero-based index of this argument amongst all (positional)
        /// default arguments.  Each default argument present within an
        /// object must have a unique position value, and they must be
        /// consecutive, with the smallest being zero.
        /// </summary>
        public int Position { get; set; }
    }
}
