using System;

namespace NClap.Utilities
{
    /// <summary>
    /// Extension methods for manipulating colored strings.
    /// </summary>
    public static class ColoredStringExtensionMethods
    {
        /// <summary>
        /// Transform the given string into a new one, preserving color.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <param name="func">The function to apply.</param>
        /// <returns>The generated string.</returns>
        public static ColoredString Transform(this ColoredString s, Func<string, string> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return new ColoredString(func(s.Content), s.ForegroundColor, s.BackgroundColor);
        }

        /// <summary>
        /// Returns a new string containing a substring of the given string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="startIndex">The 0-based index to start from.</param>
        /// <returns>The new string.</returns>
        public static ColoredString Substring(this ColoredString s, int startIndex) =>
            s.Transform(content => content.Substring(startIndex));

        /// <summary>
        /// Returns a new string containing a substring of the given string.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="startIndex">The 0-based index to start from.</param>
        /// <param name="length">The length of the substring, expressed as
        /// a count of characters.</param>
        /// <returns>The new string.</returns>
        public static ColoredString Substring(this ColoredString s, int startIndex, int length) =>
            s.Transform(content => content.Substring(startIndex, length));
    }
}
