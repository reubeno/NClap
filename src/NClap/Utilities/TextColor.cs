using System;

namespace NClap.Utilities
{
    /// <summary>
    /// Encapsulates text color.
    /// </summary>
    public struct TextColor
    {
        /// <summary>
        /// Optionally provides foreground color.  If not provided, indicates
        /// agnosticism toward foregound color.
        /// </summary>
        public ConsoleColor? Foreground { get; set; }

        /// <summary>
        /// Optionally provides background color.  If not provided, indicates
        /// agnosticism toward background color.
        /// </summary>
        public ConsoleColor? Background { get; set; }
    }
}
