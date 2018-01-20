namespace NClap.Help
{
    /// <summary>
    /// Help options for enum value help content.
    /// </summary>
    public class ArgumentEnumValueHelpOptions : ArgumentMetadataHelpOptions
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ArgumentEnumValueHelpOptions()
        {
        }

        /// <summary>
        /// Deeply cloning constructor.
        /// </summary>
        /// <param name="other">Template for clone.</param>
        private ArgumentEnumValueHelpOptions(ArgumentEnumValueHelpOptions other) : base(other)
        {
            Flags = other.Flags;
        }

        /// <summary>
        /// Whether or not enum value summaries should be fully promoted to their
        /// own section, etc.
        /// </summary>
        public ArgumentEnumValueHelpFlags Flags { get; set; } =
            ArgumentEnumValueHelpFlags.SingleSummaryOfEnumsWithMultipleUses;

        /// <summary>
        /// Create a separate clone of this object.
        /// </summary>
        /// <returns>Clone.</returns>
        public override ArgumentMetadataHelpOptions DeepClone() => new ArgumentEnumValueHelpOptions(this);
    }
}
