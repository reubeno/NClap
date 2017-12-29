namespace NClap.Help
{
    /// <summary>
    /// Mode for including short name info in help.
    /// </summary>
    public enum ArgumentShortNameHelpMode
    {
        /// <summary>
        /// Do not include short names.
        /// </summary>
        Omit,

        /// <summary>
        /// Include short names together with long names at the beginning of the
        /// description.
        /// </summary>
        IncludeWithLongName,

        /// <summary>
        /// Append short names to the end of the description.
        /// </summary>
        AppendToDescription
    }
}
