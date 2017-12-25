namespace NClap.Help
{
    /// <summary>
    /// Options for generating help for arguments.
    /// </summary>
    internal class ArgumentHelpOptions
    {
        /// <summary>
        /// Should these arguments be included at all?
        /// </summary>
        public bool Include { get; set; } = true;

        /// <summary>
        /// Layout of argument help.
        /// </summary>
        public ArgumentHelpLayout Layout { get; set; } = new SingleColumnArgumentHelpLayout();

        /// <summary>
        /// Width, in characters, of hanging indent.
        /// </summary>
        public int HangingIndentWidth { get; set; } = ArgumentSetHelpOptions.DefaultIndent;

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
            ArgumentDefaultValueHelpMode.PrependToDescription;

        /// <summary>
        /// Include information about arguments' short name aliases.
        /// </summary>
        public ArgumentShortNameHelpMode ShortName { get; set; } =
            ArgumentShortNameHelpMode.IncludeWithLongName;

        /// <summary>
        /// Ordering of arguments.
        /// </summary>
        public ArgumentSortOrder Ordering { get; set; } =
            ArgumentSortOrder.Lexicographic;
    }
}
