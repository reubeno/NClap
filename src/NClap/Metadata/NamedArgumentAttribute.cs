using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Indicates that this argument is a named argument.  Attach this attribute
    /// to instance fields (or properties) of types used as the destination
    /// of command-line argument parsing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NamedArgumentAttribute : ArgumentBaseAttribute
    {
        /// <summary>
        /// Default constructor, which may be used to indicate an optional
        /// named argument that may appear at most once.
        /// </summary>
        public NamedArgumentAttribute() : this(ArgumentFlags.Optional)
        {
        }

        /// <summary>
        /// Constructor that requires specifying flags.
        /// </summary>
        /// <param name="flags">Specifies the error checking to be done on the
        /// argument.</param>
        public NamedArgumentAttribute(ArgumentFlags flags) : base(flags)
        {
        }

        /// <summary>
        /// The short name of the argument.  Set to null means use the default
        /// short name if it does not conflict with any other parameter name.
        /// Set to string.Empty for no short name.
        /// </summary>
        public string ShortName { get; set; }
    }
}
