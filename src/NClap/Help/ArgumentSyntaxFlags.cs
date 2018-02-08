using System;

namespace NClap.Help
{
    /// <summary>
    /// Flags describing the format of argument syntax summaries.
    /// </summary>
    [Flags]
    internal enum ArgumentSyntaxFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Visibly distinguish optional arguments.
        /// </summary>
        DistinguishOptionalArguments = 0x1,

        /// <summary>
        /// Visibly indicate how many times the argument may occur.
        /// </summary>
        IndicateCardinality = 0x2,

        /// <summary>
        /// Whether or not to indicate the type of positional arguments.
        /// </summary>
        IndicatePositionalArgumentType = 0x4,

        /// <summary>
        /// Whether or not to indicate if the argument accepts an empty string.
        /// </summary>
        IndicateArgumentsThatAcceptEmptyString = 0x8,

        /// <summary>
        /// Whether or not to include the syntax of specifying the value associated with
        /// the argument.
        /// </summary>
        IncludeValueSyntax = 0x10,

        /// <summary>
        /// Defaults.
        /// </summary>
        Default =
            DistinguishOptionalArguments |
            IndicateCardinality |
            IndicateArgumentsThatAcceptEmptyString |
            IncludeValueSyntax,

        /// <summary>
        /// All flags.
        /// </summary>
        All = Default | IndicatePositionalArgumentType,
    }
}
