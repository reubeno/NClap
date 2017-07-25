using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Flags controlling the use of argument values.
    /// </summary>
    [Flags]
    public enum ArgumentValueFlags
    {
        /// <summary>
        /// Indicates default behavior is desired.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that the related value should not be allowed.
        /// </summary>
        Disallowed,

        /// <summary>
        /// Indicates that the related value should not be displayed in help
        /// text.
        /// </summary>
        Hidden
    }
}