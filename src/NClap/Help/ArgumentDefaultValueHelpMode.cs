namespace NClap.Help
{
    /// <summary>
    /// Mode for including info about argument default values.
    /// </summary>
    public enum ArgumentDefaultValueHelpMode
    {
        /// <summary>
        /// Do not include default values.
        /// </summary>
        Omit,

        /// <summary>
        /// Prepend to the start of the argument's description (but after the
        /// argument's syntax).
        /// </summary>
        PrependToDescription,

        /// <summary>
        /// Append to the end of the argument's description.
        /// </summary>
        AppendToDescription
    }
}
