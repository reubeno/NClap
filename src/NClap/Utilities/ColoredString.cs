using System;
using System.Diagnostics.CodeAnalysis;

namespace NClap.Utilities
{
    /// <summary>
    /// A colored string intended for display on a console.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
    public struct ColoredString : IEquatable<ColoredString>
    {
        /// <summary>
        /// Convenience constructor that defaults the foreground and background
        /// colors.
        /// </summary>
        /// <param name="content">The string content.</param>
        public ColoredString(string content) : this(content, null, null)
        {
        }

        /// <summary>
        /// Convenience constructor that defaults the background color.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <param name="foregroundColor">If present, the foreground color for
        /// the text; otherwise, the existing foreground color should be used
        /// to display the text.</param>
        public ColoredString(string content, ConsoleColor? foregroundColor)
            : this(content, foregroundColor, null)
        {
        }

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <param name="foregroundColor">If present, the foreground color for
        /// the text; otherwise, the existing foreground color should be used
        /// to display the text.</param>
        /// <param name="backgroundColor">If present, the background color for
        /// the text; otherwise, the existing background color should be used
        /// to display the text.</param>
        public ColoredString(string content, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        /// <summary>
        /// Empty string.
        /// </summary>
        public static ColoredString Empty { get; } = new ColoredString(string.Empty);

        /// <summary>
        /// The string's content.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// If present, the foreground color for the text; otherwise, the
        /// existing foreground color should be used to display the text.
        /// </summary>
        public ConsoleColor? ForegroundColor { get; }

        /// <summary>
        /// If present, the background color for the text; otherwise, the
        /// existing background color should be used to display the text.
        /// </summary>
        public ConsoleColor? BackgroundColor { get; }

        /// <summary>
        /// Length of the content of the string.
        /// </summary>
        public int Length => Content.Length;

        /// <summary>
        /// Checks if the string is empty. Note that a string with no characters
        /// is considered empty, even if it contains color information.
        /// </summary>
        /// <returns>True if empty, false otherwise.</returns>
        public bool IsEmpty() => string.IsNullOrEmpty(Content);

        /// <summary>
        /// Accesses a character in the string.
        /// </summary>
        /// <param name="index">Zero-based index of the character.</param>
        /// <returns>The character at the specified index.</returns>
        /// <returns></returns>
        public char this[int index] => Content[index];

        /// <summary>
        /// Implicitly converts a ColoredString to a string by stripping
        /// color information.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static implicit operator string(ColoredString value) => value.Content;

        /// <summary>
        /// Wraps an uncolored string.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator ColoredString(string value) => FromString(value);

        /// <summary>
        /// Wraps an uncolored string.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        public static ColoredString FromString(string value) =>
            new ColoredString(value, null, null);

        /// <summary>
        /// Compares two ColoredString objects.
        /// </summary>
        /// <param name="value">The first value.</param>
        /// <param name="otherValue">The second value.</param>
        /// <returns>True if they are equal; false otherwise.</returns>
        public static bool operator ==(ColoredString value, ColoredString otherValue) =>
            value.Equals(otherValue, StringComparison.Ordinal);

        /// <summary>
        /// Compares two ColoredString objects.
        /// </summary>
        /// <param name="value">The first value.</param>
        /// <param name="otherValue">The second value.</param>
        /// <returns>True if they are not equal; false otherwise.</returns>
        public static bool operator !=(ColoredString value, ColoredString otherValue) =>
            !value.Equals(otherValue, StringComparison.Ordinal);

        /// <summary>
        /// Checks if this string has the same color as the provided string.
        /// </summary>
        /// <param name="value">The string to compare against.</param>
        /// <returns>True if the two strings have the same color; false otherwise.
        /// Does not otherwise compare contents.</returns>
        public bool IsSameColorAs(ColoredString value) =>
            value.ForegroundColor == ForegroundColor &&
            value.BackgroundColor == BackgroundColor;

        /// <summary>
        /// The string's content.
        /// </summary>
        /// <returns>The uncolored string.</returns>
        public override string ToString() => Content;

        /// <summary>
        /// Computes the hash code for this object.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            // Overflow is fine, just wrap
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + Content.GetHashCode();
                hash = hash * 23 + ForegroundColor.GetHashCode();
                hash = hash * 23 + BackgroundColor.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Compares the specified object against this object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if the objects are equal; false otherwise.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison")]
        public override bool Equals(object obj) =>
            obj is ColoredString && Equals((ColoredString)obj);

        /// <summary>
        /// Compares the specified object against this object.
        /// </summary>
        /// <param name="value">The object to compare.</param>
        /// <returns>True if the objects are equal; false otherwise.</returns>
        public bool Equals(ColoredString value) => Equals(value, StringComparison.Ordinal);

        /// <summary>
        /// Compares the specified object against this object.
        /// </summary>
        /// <param name="value">The object to compare.</param>
        /// <param name="comparisonType">Type of comparison to perform.</param>
        /// <returns>True if the objects are equal; false otherwise.</returns>
        public bool Equals(ColoredString value, StringComparison comparisonType)
        {
            if ((Content == null) != (value.Content == null))
            {
                return false;
            }

            if (Content != null && !Content.Equals(value.Content, comparisonType))
            {
                return false;
            }

            return IsSameColorAs(value);
        }
    }
}
