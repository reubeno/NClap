using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Style flags for names generated automatically from code symbols.
    /// </summary>
    [Flags]
    public enum ArgumentNameGenerationFlags
    {
        /// <summary>
        /// Use the code symbol commandatim.
        /// </summary>
        UseOriginalCodeSymbol = 0x0,

        /// <summary>
        /// Make a best effort attempt to convert code symbols to hyphenated,
        /// lower-case symbols when generating long names.
        /// </summary>
        GenerateHyphenatedLowerCaseLongNames = 0x1,

        /// <summary>
        /// Prefer lower case short names.
        /// </summary>
        PreferLowerCaseForShortNames = 0x2
    }
}
