using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NClap.Utilities
{
    /// <summary>
    /// Represents text comprised of strings of different colors.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
    public class ColoredMultistring : IString
    {
        /// <summary>
        /// Simple constructor.
        /// </summary>
        /// <param name="value">String content.</param>
        public ColoredMultistring(ColoredString value)
        {
            Content = new[] { value };
        }

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="values">Pieces of the multistring.</param>
        public ColoredMultistring(IEnumerable<ColoredString> values)
        {
            Content = values.ToList();
        }

        /// <summary>
        /// Empty multistring.
        /// </summary>
        public static ColoredMultistring Empty { get; } = new ColoredMultistring(Array.Empty<ColoredString>());

        /// <summary>
        /// The content of the multistring.
        /// </summary>
        public IReadOnlyList<ColoredString> Content { get; }

        /// <summary>
        /// The length of the content of the multistring.
        /// </summary>
        public int Length => Content.Sum(s => s.Length);

        /// <summary>
        /// Accesses a character in the string.
        /// </summary>
        /// <param name="index">Zero-based index of the character.</param>
        /// <returns>The character at the specified index.</returns>
        public char this[int index]
        {
            [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            get
            {
                var currentCount = 0;
                foreach (var piece in Content)
                {
                    if (index - currentCount < piece.Length)
                    {
                        return piece[index - currentCount];
                    }

                    currentCount += piece.Length;
                }

                throw new IndexOutOfRangeException($"Accessing index {index} in a multistring with {Length} characters.");
            }
        }

        /// <summary>
        /// Extract the uncolored string content from the provided multistring.
        /// </summary>
        /// <param name="value">The multistring to process.</param>
        public static explicit operator string(ColoredMultistring value) => value?.ToString();

        /// <summary>
        /// Wraps a string.
        /// </summary>
        /// <param name="value">The string to wrap.</param>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static explicit operator ColoredMultistring(string value) =>
            (value == null) ? null : FromString(value);

        /// <summary>
        /// Wraps a single colored string.
        /// </summary>
        /// <param name="value">The string to wrap.</param>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static explicit operator ColoredMultistring(ColoredString value) =>
            new ColoredMultistring(new[] { value });

        /// <summary>
        /// Wraps a string.
        /// </summary>
        /// <param name="value">The string to wrap.</param>
        public static ColoredMultistring FromString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new ColoredMultistring(new ColoredString[] { value });
        }

        /// <summary>
        /// Extract the uncolored string content.
        /// </summary>
        /// <returns>The uncolored string content.</returns>
        public override string ToString() => string.Concat(Content.Select(str => str.Content));

        /// <summary>
        /// Checks if the multistring is empty.
        /// </summary>
        /// <returns>True if empty, false otherwise.</returns>
        public bool IsEmpty() => Content.All(piece => piece.IsEmpty());

        /// <summary>
        /// Generate a new string, with existing instances of <paramref name="pattern"/>
        /// with <paramref name="replacement"/>.
        /// </summary>
        /// <param name="pattern">The pattern to search for.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns>The new string, with replacements made.</returns>
        public IString Replace(string pattern, string replacement) =>
            // N.B. We do not consider matches across pieces.
            new ColoredMultistring(Content.Select(piece =>
                piece.Transform(content => content.Replace(pattern, replacement))));

        /// <summary>
        /// Split the string by the indicated separator.
        /// </summary>
        /// <param name="separator">Separator for splitting.</param>
        /// <returns>The split pieces of the string.</returns>
        public IEnumerable<IString> Split(char separator)
        {
            ColoredMultistringBuilder builder = null;
            foreach (var piece in Content)
            {
                var s = piece.Content;
                int index;
                while ((index = s.IndexOf(separator)) >= 0)
                {
                    if (index > 0)
                    {
                        if (builder == null)
                        {
                            builder = new ColoredMultistringBuilder();
                        }

                        var firstPart = s.Substring(0, index);
                        builder.Append(new ColoredString(firstPart, piece.ForegroundColor, piece.BackgroundColor));

                        yield return builder.ToMultistring();
                        builder = null;
                    }

                    s = s.Substring(index + 1);
                }

                if (!string.IsNullOrEmpty(s))
                {
                    if (builder == null)
                    {
                        builder = new ColoredMultistringBuilder();
                    }

                    builder.Append(new ColoredString(s, piece.ForegroundColor, piece.BackgroundColor));
                }
            }

            if (builder != null)
            {
                yield return builder.ToMultistring();
            }
        }

        /// <summary>
        /// Extract a substring.
        /// </summary>
        /// <param name="startIndex">Index to start from.</param>
        /// <param name="length">Count of characters to extract.</param>
        /// <returns>The substring.</returns>
        public IString Substring(int startIndex, int length)
        {
            var currentIndex = 0;
            var lengthLeft = length;
            var builder = new ColoredMultistringBuilder();

            foreach (var piece in Content)
            {
                if (currentIndex + piece.Length <= startIndex)
                {
                    // Do nothing.
                }
                else
                {
                    var offsetIntoPiece = (startIndex <= currentIndex) ? 0 : startIndex - currentIndex;
                    var charsToCopy = Math.Min(lengthLeft, piece.Length - offsetIntoPiece);
                    Debug.Assert(charsToCopy > 0);

                    if (charsToCopy == piece.Length)
                    {
                        builder.Append(piece);
                    }
                    else
                    {
                        builder.Append(piece.Substring(offsetIntoPiece, charsToCopy));
                    }

                    lengthLeft -= charsToCopy;
                    Debug.Assert(lengthLeft >= 0);

                    if (lengthLeft == 0)
                    {
                        break;
                    }
                }

                currentIndex += piece.Length;
            }

            if (lengthLeft > 0)
            {
                throw new IndexOutOfRangeException($"Invalid substring extraction");
            }

            return builder.ToMultistring();
        }

        /// <summary>
        /// Generate a new string from this one, with any trailing whitespace
        /// removed.
        /// </summary>
        /// <returns>The new string.</returns>
        public IString TrimEnd()
        {
            var pieces = Content.ToList();

            // Trim off all 0-length substrings.
            while (pieces.Count > 0 && pieces.Last().Length == 0)
            {
                pieces.RemoveAt(pieces.Count - 1);
            }

            // Trim.
            while (pieces.Count > 0 && char.IsWhiteSpace(pieces.Last().Content.Last()))
            {
                var updatedPiece = pieces.Last();
                pieces.RemoveAt(pieces.Count - 1);

                updatedPiece = updatedPiece.Transform(content => content.TrimEnd());
                if (updatedPiece.Length > 0)
                {
                    pieces.Add(updatedPiece);
                }
            }

            return new ColoredMultistring(pieces);
        }

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
        // TODO: Make this less painfully slow.
        public int LastIndexOfAny(char[] chars, int startIndex, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var length = Length;

            if (startIndex > length - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (startIndex - count + 1 < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            for (int i = startIndex; i > startIndex - count; --i)
            {
                var currentChar = this[i];
                foreach (var c in chars)
                {
                    if (c == currentChar)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Construct a builder that can generate a string of this type.
        /// </summary>
        /// <returns>A new builder.</returns>
        public IStringBuilder CreateNewBuilder() => new ColoredMultistringBuilder();
    }
}
