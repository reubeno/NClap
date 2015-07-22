namespace NClap.Metadata
{
    /// <summary>
    /// Indicates that this argument is an (unnamed) positional argument.  The
    /// LongName property is used for usage text only and does not affect the
    /// usage of the argument.
    /// </summary>
    public sealed class PositionalArgumentAttribute : ArgumentBaseAttribute
    {
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
