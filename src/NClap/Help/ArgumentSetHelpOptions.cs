using NClap.Utilities;
using System;

namespace NClap.Help
{
    /// <summary>
    /// Options for generating help for an argument set.
    /// </summary>
    internal class ArgumentSetHelpOptions
    {
        /// <summary>
        /// Default indent width, expressed in characters.
        /// </summary>
        public const int DefaultIndent = 4;

        /// <summary>
        /// Default maximum width, expressed in characters.
        /// </summary>
        public const int DefaultMaxWidth = 80;

        /// <summary>
        /// Optionally indicates maximum width of help, expressed in characters.
        /// If left unspecified, the full width of the target output surface will
        /// be used.
        /// </summary>
        public int? MaxWidth { get; set; } = null;

        /// <summary>
        /// Use multiple colors in the output.
        /// </summary>
        public bool UseColor { get; set; } = true;

        /// <summary>
        /// Number of characters to indent all entries directly under a section.
        /// </summary>
        public int SectionEntryIndentWidth { get; set; } = DefaultIndent;

        /// <summary>
        /// Number of lines left blank between major sections of the help information.
        /// </summary>
        public int BlankLinesBetweenSections { get; set; } = 1;

        /// <summary>
        /// Options for section headers.
        /// </summary>
        public ArgumentMetadataHelpOptions SectionHeaders { get; set; } = new ArgumentMetadataHelpOptions
        {
            Color = new TextColor { Foreground = ConsoleColor.Cyan }
        };

        /// <summary>
        /// Logo options.
        /// </summary>
        public ArgumentMetadataHelpOptions Logo { get; set; } = new ArgumentMetadataHelpOptions();

        /// <summary>
        /// Name options.
        /// </summary>
        public ArgumentMetadataHelpOptions Name { get; set; } = new ArgumentMetadataHelpOptions();

        /// <summary>
        /// Description options.
        /// </summary>
        public ArgumentMetadataHelpOptions Description { get; set; } = new ArgumentMetadataHelpOptions();

        /// <summary>
        /// Basic syntax options.
        /// </summary>
        public ArgumentMetadataHelpOptions Syntax { get; set; } = new ArgumentMetadataHelpOptions
        {
            HeaderTitle = Strings.UsageInfoUsageHeader
        };

        /// <summary>
        /// Options for summaries of possible values for enum types.
        /// </summary>
        public ArgumentMetadataHelpOptions EnumValues { get; set; } = new ArgumentMetadataHelpOptions
        {
            HeaderTitle = Strings.UsageInfoEnumValueHeaderFormat
        };

        /// <summary>
        /// Example usage options.
        /// </summary>
        public ArgumentMetadataHelpOptions Examples { get; set; } = new ArgumentMetadataHelpOptions
        {
            HeaderTitle = Strings.UsageInfoExamplesHeader
        };

        /// <summary>
        /// Options for arguments.
        /// </summary>
        public ArgumentHelpOptions Arguments { get; set; } = new ArgumentHelpOptions();

        /// <summary>
        /// Mode for grouping arguments.
        /// </summary>
        public ArgumentGroupingMode ArgumentGroupingMode { get; set; } = ArgumentGroupingMode.RequiredVersusOptional;
    }
}
