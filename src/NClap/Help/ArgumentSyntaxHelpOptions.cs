using NClap.Utilities;

namespace NClap.Help
{
    /// <summary>
    /// Help options for argument syntax summaries.
    /// </summary>
    public class ArgumentSyntaxHelpOptions : ArgumentMetadataHelpOptions
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ArgumentSyntaxHelpOptions()
        {
        }

        /// <summary>
        /// Deeply cloning constructor.
        /// </summary>
        /// <param name="other">Template for clone.</param>
        private ArgumentSyntaxHelpOptions(ArgumentSyntaxHelpOptions other) : base(other)
        {
            CommandNameColor = other.CommandNameColor;
        }

        /// <summary>
        /// Color of the command name.
        /// </summary>
        public TextColor CommandNameColor { get; set; }

        /// <summary>
        /// Create a separate clone of this object.
        /// </summary>
        /// <returns>Clone.</returns>
        public override ArgumentMetadataHelpOptions DeepClone() => new ArgumentSyntaxHelpOptions(this);
    }
}
