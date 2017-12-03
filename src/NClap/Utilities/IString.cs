using System.Collections.Generic;

namespace NClap.Utilities
{
    /// <summary>
    /// Abstract interface for objects that represent string content.
    /// </summary>
    public interface IString
    {
        /// <summary>
        /// Accesses a character in the string.
        /// </summary>
        /// <param name="index">Zero-based index of the character.</param>
        /// <returns>The character at the specified index.</returns>
        char this[int index] { get; }

        /// <summary>
        /// Length of the string, counted in characters.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Generate a new string, with existing instances of <paramref name="pattern"/>
        /// with <paramref name="replacement"/>.
        /// </summary>
        /// <param name="pattern">The pattern to search for.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns>The new string, with replacements made.</returns>
        IString Replace(string pattern, string replacement);

        /// <summary>
        /// Split the string by the indicated separator.
        /// </summary>
        /// <param name="separator">Separator for splitting.</param>
        /// <returns>The split pieces of the string.</returns>
        IEnumerable<IString> Split(char separator);

        /// <summary>
        /// Extract a substring.
        /// </summary>
        /// <param name="startIndex">Index to start from.</param>
        /// <param name="length">Count of characters to extract.</param>
        /// <returns>The substring.</returns>
        IString Substring(int startIndex, int length);

        /// <summary>
        /// Generate a new string from this one, with any trailing whitespace
        /// removed.
        /// </summary>
        /// <returns>The new string.</returns>
        IString TrimEnd();

        /// <summary>
        /// Search within this string for any of the provided characters,
        /// starting from the specified index.
        /// </summary>
        /// <param name="chars">Characters to look for.</param>
        /// <param name="startIndex">Index of the character to start looking
        /// at.</param>
        /// <param name="count">The count of characters to consider.</param>
        /// <returns>The last index of any of the provided characters in
        /// the specified sub-range of the string, or -1 if no such
        /// character was found.</returns>
        int LastIndexOfAny(char[] chars, int startIndex, int count);

        /// <summary>
        /// Construct a builder that can generate a string of this type.
        /// </summary>
        /// <returns>A new builder.</returns>
        IStringBuilder CreateNewBuilder();
    }
}
