using System;

namespace NClap.Parser
{
    /// <summary>
    /// Options for tokenizing command lines.
    /// </summary>
    [Flags]
    public enum CommandLineTokenizerOptions
    {
        /// <summary>
        /// Use default semantics.
        /// </summary>
        None,

        /// <summary>
        /// Allow tokenizing of partial (incomplete) input lines; this includes
        /// ignoring errors related to unmatched quotes around the last token.
        /// </summary>
        AllowPartialInput
    }
}
