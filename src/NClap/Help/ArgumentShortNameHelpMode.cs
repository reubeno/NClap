namespace NClap.Help
{
    /// <summary>
    /// Mode for including short name info in help.
    /// </summary>
    internal enum ArgumentShortNameHelpMode
    {
        /// <summary>
        /// Do not include short names.
        /// </summary>
        Omit,

        /// <summary>
        /// Include short names right alongside the long names.
        /// </summary>
        IncludeWithLongName,

        /// <summary>
        /// Append short names to the end of the description.
        /// </summary>
        AppendToDescription
    }
}
