using NClap.Utilities;
using System;

namespace NClap.Help
{
    /// <summary>
    /// Options for generating help for an argument set.
    /// </summary>
    public class ArgumentSetHelpOptions : IDeepCloneable<ArgumentSetHelpOptions>
    {
        /// <summary>
        /// Default indent width, expressed in characters.
        /// </summary>
        public const int DefaultBlockIndent = 4;

        /// <summary>
        /// Default hanging indent width, expressed in characters.
        /// </summary>
        public const int DefaultHangingIndent = 4;

        /// <summary>
        /// Default maximum width, expressed in characters.
        /// </summary>
        public const int DefaultMaxWidth = 80;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ArgumentSetHelpOptions()
        {
        }

        /// <summary>
        /// Deeply cloning constructor.
        /// </summary>
        /// <param name="other">Template for clone.</param>
        private ArgumentSetHelpOptions(ArgumentSetHelpOptions other)
        {
            MaxWidth = other.MaxWidth;
            UseColor = other.UseColor;
            SectionEntryBlockIndentWidth = other.SectionEntryBlockIndentWidth;
            SectionEntryHangingIndentWidth = other.SectionEntryHangingIndentWidth;
            BlankLinesBetweenSections = other.BlankLinesBetweenSections;
            Name = other.Name;
            SectionHeaders = other.SectionHeaders.DeepClone();
            Logo = other.Logo.DeepClone();
            Description = other.Description.DeepClone();
            Syntax = (ArgumentSyntaxHelpOptions)other.Syntax.DeepClone();
            EnumValues = other.EnumValues.DeepClone();
            Examples = other.Examples.DeepClone();
            Arguments = other.Arguments.DeepClone();
        }

        /// <summary>
        /// Indicates maximum width of help, expressed in characters.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Use multiple colors in the output.
        /// </summary>
        public bool UseColor { get; set; } = true;

        /// <summary>
        /// Number of characters to block-indent all entries directly under a section.
        /// </summary>
        public int SectionEntryBlockIndentWidth { get; set; } = DefaultBlockIndent;

        /// <summary>
        /// Number of characters to hanging-indent all entries directly under a section.
        /// </summary>
        public int SectionEntryHangingIndentWidth { get; set; } = DefaultHangingIndent;

        /// <summary>
        /// Number of lines left blank between major sections of the help information.
        /// </summary>
        public int BlankLinesBetweenSections { get; set; } = 1;

        /// <summary>
        /// Name to use for argument set (i.e. first token in syntax).
        /// </summary>
        public string Name { get; set; } = AssemblyUtilities.GetAssemblyFileName();

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
        public ArgumentMetadataHelpOptions Logo { get; set; } = new ArgumentMetadataHelpOptions
        {
            BlockIndent = 0,
            HangingIndent = 0
        };

        /// <summary>
        /// Description options.
        /// </summary>
        public ArgumentMetadataHelpOptions Description { get; set; } = new ArgumentMetadataHelpOptions
        {
            HangingIndent = 0
        };

        /// <summary>
        /// Basic syntax options.
        /// </summary>
        public ArgumentSyntaxHelpOptions Syntax { get; set; } = new ArgumentSyntaxHelpOptions
        {
            HeaderTitle = Strings.UsageInfoUsageHeader,
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
        /// Creates a separate clone of this object.
        /// </summary>
        /// <returns>Clone.</returns>
        public ArgumentSetHelpOptions DeepClone() => new ArgumentSetHelpOptions(this);
    }
}
