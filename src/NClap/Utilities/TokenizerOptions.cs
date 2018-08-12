using System;

namespace NClap.Utilities
{
    /// <summary>
    /// Options for tokenizing command lines.
    /// </summary>
    [Flags]
    internal enum TokenizerOptions
    {
        /// <summary>
        /// Do not apply other policy options.
        /// </summary>
        None,

        /// <summary>
        /// Allow tokenizing of partial (incomplete) input lines; this includes
        /// ignoring errors related to unmatched quotes around the last token.
        /// </summary>
        AllowPartialInput,

        /// <summary>
        /// Handle double quote character as a token delimiter (allowing embedded
        /// whitespace within the token.
        /// </summary>
        HandleDoubleQuoteAsTokenDelimiter,

        /// <summary>
        /// Handle single quote character as a token delimiter (allowing embedded
        /// whitespace within the token.
        /// </summary>
        HandleSingleQuoteAsTokenDelimiter,
    }
}
