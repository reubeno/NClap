﻿namespace NClap.Metadata
{
    /// <summary>
    /// Indicates that this argument is a named argument.  Attach this attribute
    /// to instance fields (or properties) of types used as the destination
    /// of command-line argument parsing.
    /// </summary>
    public sealed class NamedArgumentAttribute : ArgumentBaseAttribute
    {
        /// <summary>
        /// Primary constructor.
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