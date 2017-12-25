namespace NClap.Help
{
    /// <summary>
    /// Mode for including info about argument default values.
    /// </summary>
    internal enum ArgumentDefaultValueHelpMode
    {
        /// <summary>
        /// Do not include default values.
        /// </summary>
        Omit,

        /// <summary>
        /// Prepend to the beginning of the argument's description.
        /// </summary>
        PrependToDescription,

        /// <summary>
        /// Append to the end of the argument's description.
        /// </summary>
        AppendToDescription
    }
}
