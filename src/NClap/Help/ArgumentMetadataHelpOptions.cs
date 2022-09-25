using System;
using NClap.Utilities;

namespace NClap.Help
{
    /// <summary>
    /// Options for a piece of argument metadata.
    /// </summary>
    public class ArgumentMetadataHelpOptions : IDeepCloneable<ArgumentMetadataHelpOptions>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ArgumentMetadataHelpOptions()
        {
        }

        /// <summary>
        /// Deeply cloning constructor.
        /// </summary>
        /// <param name="other">Template for clone.</param>
        protected ArgumentMetadataHelpOptions(ArgumentMetadataHelpOptions other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            Include = other.Include;
            HeaderTitle = other.HeaderTitle;
            Color = other.Color;
            BlockIndent = other.BlockIndent;
            HangingIndent = other.HangingIndent;
        }

        /// <summary>
        /// Should this piece of metadata be included.
        /// </summary>
        public bool Include { get; set; } = true;

        /// <summary>
        /// Optionally provides header text; if left unspecified, default is used.
        /// </summary>
        public string HeaderTitle { get; set; }

        /// <summary>
        /// Color for this piece of metadata.
        /// </summary>
        public TextColor Color { get; set; }

        /// <summary>
        /// Number of characters to block-indent the body of this item; if present,
        /// will override the default specified in <see cref="ArgumentSetHelpOptions"/>.
        /// </summary>
        public int? BlockIndent { get; set; }

        /// <summary>
        /// Number of characters to hanging-indent the body of this item; if present,
        /// will override the default specified in <see cref="ArgumentSetHelpOptions"/>.
        /// </summary>
        public int? HangingIndent { get; set; }

        /// <summary>
        /// Create a separate clone of this object.
        /// </summary>
        /// <returns>Clone.</returns>
        public virtual ArgumentMetadataHelpOptions DeepClone() => new ArgumentMetadataHelpOptions(this);
    }
}
