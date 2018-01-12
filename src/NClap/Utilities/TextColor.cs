using System;

namespace NClap.Utilities
{
    /// <summary>
    /// Encapsulates text color.
    /// </summary>
    public struct TextColor : IEquatable<TextColor>
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

        /// <summary>
        /// Compares the specified object against this object.
        /// </summary>
        /// <param name="value">The object to compare.</param>
        /// <returns>True if the objects are equal; false otherwise.</returns>
        public override bool Equals(object value) => value is TextColor tc && Equals(tc);

        /// <summary>
        /// Compares the specified object against this object.
        /// </summary>
        /// <param name="value">The object to compare.</param>
        /// <returns>True if the objects are equal; false otherwise.</returns>
        public bool Equals(TextColor value) =>
            value.Foreground == Foreground &&
            value.Background == Background;

        /// <summary>
        /// Generate a hash code for the value.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() =>
            (Foreground?.GetHashCode() ?? 0) ^
            (Background?.GetHashCode() ?? 0);

        /// <summary>
        /// Compares the specified objects.
        /// </summary>
        /// <param name="left">The first object.</param>
        /// <param name="right">The second object.</param>
        /// <returns>True if the objects are equal; false otherwise.</returns>
        public static bool operator ==(TextColor left, TextColor right) => left.Equals(right);

        /// <summary>
        /// Compares the specified objects.
        /// </summary>
        /// <param name="left">The first object.</param>
        /// <param name="right">The second object.</param>
        /// <returns>True if the objects are not equal; false otherwise.</returns>
        public static bool operator !=(TextColor left, TextColor right) => !left.Equals(right);
    }
}
