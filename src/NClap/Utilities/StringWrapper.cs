using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace NClap.Utilities
{
    /// <summary>
    /// Simple wrapper, implementing <see cref="IString"/> for plain strings.
    /// </summary>
    internal class StringWrapper : IString
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="s">String to wrap.</param>
        public StringWrapper(string s)
        {
            Content = s;
        }

        /// <summary>
        /// Implicit conversion operator to wrap a string.
        /// </summary>
        /// <param name="s">String to wrap.</param>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator StringWrapper(string s) => new StringWrapper(s);

        /// <summary>
        /// Implicit conversion operator to unwrap a string.
        /// </summary>
        /// <param name="s">String to unwrap.</param>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator string(StringWrapper s) => s.Content;

        /// <summary>
        /// Inner string content.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Accesses a character in the string.
        /// </summary>
        /// <param name="index">Zero-based index of the character.</param>
        /// <returns>The character at the specified index.</returns>
        public char this[int index] => Content[index];

        /// <summary>
        /// Length of the string, counted in characters.
        /// </summary>
        public int Length => Content.Length;

        /// <summary>
        /// Generate a new string, with existing instances of <paramref name="pattern"/>
        /// with <paramref name="replacement"/>.
        /// </summary>
        /// <param name="pattern">The pattern to search for.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns>The new string, with replacements made.</returns>
        public IString Replace(string pattern, string replacement) =>
            (StringWrapper)Content.Replace(pattern, replacement);

        /// <summary>
        /// Split the string by the indicated separator.
        /// </summary>
        /// <param name="separator">Separator for splitting.</param>
        /// <returns>The split pieces of the string.</returns>
        public IEnumerable<IString> Split(char separator) =>
            Content.Split(new[] { separator }, StringSplitOptions.None).Select(s => (StringWrapper)s);

        /// <summary>
        /// Extract a substring.
        /// </summary>
        /// <param name="startIndex">Index to start from.</param>
        /// <param name="length">Count of characters to extract.</param>
        /// <returns>The substring.</returns>
        public IString Substring(int startIndex, int length) =>
            (StringWrapper)Content.Substring(startIndex, length);

        /// <summary>
        /// Generate a new string from this one, with any trailing whitespace
        /// removed.
        /// </summary>
        /// <returns>The new string.</returns>
        public IString TrimEnd() => (StringWrapper)Content.TrimEnd();

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
        public int LastIndexOfAny(char[] chars, int startIndex, int count) =>
            Content.LastIndexOfAny(chars, startIndex, count);


        /// <summary>
        /// Construct a builder that can generate a string of this type.
        /// </summary>
        /// <returns>A new builder.</returns>
        public IStringBuilder CreateBuilder() => new StringBuilderWrapper();

        private class StringBuilderWrapper : IStringBuilder
        {
            private readonly StringBuilder _builder = new StringBuilder();

            public void Append(IString s) => _builder.Append(s);

            public void Append(string s) => _builder.Append(s);

            public void Append(char c, int count) => _builder.Append(c, count);

            public IString Generate() => (StringWrapper)_builder.ToString();
        }
    }
}
