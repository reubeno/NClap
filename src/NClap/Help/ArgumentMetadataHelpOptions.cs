using NClap.Utilities;

namespace NClap.Help
{
    /// <summary>
    /// Options for a piece of argument metadata.
    /// </summary>
    internal class ArgumentMetadataHelpOptions
    {
        /// <summary>
        /// Should this piece of metadata be included?
        /// </summary>
        public bool Include { get; set; } = true;

        /// <summary>
        /// Optionally provides header text.
        /// </summary>
        public string HeaderTitle { get; set; }

        /// <summary>
        /// Color for this piece of metadata.
        /// </summary>
        public TextColor Color { get; set; }
    }
}
