using NClap.Utilities;
using System;

namespace NClap.Help
{
    /// <summary>
    /// Options for generating help for arguments.
    /// </summary>
    public class ArgumentHelpOptions : IDeepCloneable<ArgumentHelpOptions>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ArgumentHelpOptions()
        {
        }

        /// <summary>
        /// Deeply cloning constructor.
        /// </summary>
        /// <param name="other">Template for clone.</param>
        private ArgumentHelpOptions(ArgumentHelpOptions other)
        {
            RequiredArguments = other.RequiredArguments.DeepClone();
            OptionalArguments = other.OptionalArguments.DeepClone();
            Layout = other.Layout.DeepClone();
            HangingIndentWidth = other.HangingIndentWidth;
            BlankLinesBetweenArguments = other.BlankLinesBetweenArguments;
            IncludeDescription = other.IncludeDescription;
            DefaultValue = other.DefaultValue;
            ShortName = other.ShortName;
            Ordering = other.Ordering;
            MetadataColor = other.MetadataColor;
        }

        /// <summary>
        /// Required arguments options.
        /// </summary>
        public ArgumentMetadataHelpOptions RequiredArguments { get; set; } = new ArgumentMetadataHelpOptions
        {
            HeaderTitle = Strings.UsageInfoRequiredParametersHeader
        };

        /// <summary>
        /// Optional arguments options.
        /// </summary>
        public ArgumentMetadataHelpOptions OptionalArguments { get; set; } = new ArgumentMetadataHelpOptions
        {
            HeaderTitle = Strings.UsageInfoOptionalParametersHeader
        };

        /// <summary>
        /// Layout of argument help.
        /// </summary>
        public ArgumentHelpLayout Layout { get; set; } = new SingleColumnArgumentHelpLayout();

        /// <summary>
        /// Width, in characters, of hanging indent.
        /// </summary>
        public int HangingIndentWidth { get; set; } = ArgumentSetHelpOptions.DefaultBlockIndent;

        /// <summary>
        /// Number of lines left blank between two adjacent arguments.
        /// </summary>
        public int BlankLinesBetweenArguments { get; set; } = 0;

        /// <summary>
        /// Include argument descriptions.
        /// </summary>
        public bool IncludeDescription { get; set; } = true;

        /// <summary>
        /// Include argument default values.
        /// </summary>
        public ArgumentDefaultValueHelpMode DefaultValue { get; set; } =
            ArgumentDefaultValueHelpMode.AppendToDescription;

        /// <summary>
        /// Include information about arguments' short name aliases.
        /// </summary>
        public ArgumentShortNameHelpMode ShortName { get; set; } =
            ArgumentShortNameHelpMode.AppendToDescription;

        /// <summary>
        /// Ordering of arguments.
        /// </summary>
        public ArgumentSortOrder Ordering { get; set; } =
            ArgumentSortOrder.Lexicographic;

        /// <summary>
        /// Color of argument metadata.
        /// </summary>
        public TextColor MetadataColor { get; set; } = new TextColor
        {
            Foreground = ConsoleColor.DarkCyan
        };

        /// <summary>
        /// Color of argument name.
        /// </summary>
        public TextColor ArgumentNameColor { get; set; } = new TextColor
        {
            Foreground = ConsoleColor.White
        };

        /// <summary>
        /// Creates a separate clone of this object.
        /// </summary>
        /// <returns>Clone.</returns>
        public ArgumentHelpOptions DeepClone() => new ArgumentHelpOptions(this);
    }
}
