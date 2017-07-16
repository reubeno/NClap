using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NClap.Utilities
{
    /// <summary>
    /// Represents text comprised of strings of different colors.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
    public class ColoredMultistring
    {
        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="value"></param>
        public ColoredMultistring(IEnumerable<ColoredString> value)
        {
            Content = value.ToList();
        }

        /// <summary>
        /// The content of the multistring.
        /// </summary>
        public IReadOnlyList<ColoredString> Content { get; }

        /// <summary>
        /// Extract the uncolored string content from the provided multistring.
        /// </summary>
        /// <param name="value">The multistring to process.</param>
        public static implicit operator string(ColoredMultistring value) =>
            (value == null) ? null : string.Concat(value.Content.Select(str => str.Content));

        /// <summary>
        /// Wraps a string.
        /// </summary>
        /// <param name="value">The string to wrap.</param>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
        public static implicit operator ColoredMultistring(string value) => FromString(value);

        /// <summary>
        /// Wraps a string.
        /// </summary>
        /// <param name="value">The string to wrap.</param>
        public static ColoredMultistring FromString(string value) =>
            (value == null) ? null : new ColoredMultistring(new ColoredString[] { value });

        /// <summary>
        /// Extract the uncolored string content.
        /// </summary>
        /// <returns>The uncolored string content.</returns>
        public override string ToString() => this;
    }
}
