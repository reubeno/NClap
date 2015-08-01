using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Options for displaying usage help information.
    /// </summary>
    [Flags]
    public enum HelpOptions
    {
        /// <summary>
        /// Do not display any optional help.
        /// </summary>
        None = 0,

        /// <summary>
        /// Display example usage.
        /// </summary>
        Examples = 0x1
    }
}
