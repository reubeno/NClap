namespace NClap.Metadata
{
    /// <summary>
    /// Overall argument parsing style.
    /// </summary>
    public enum ArgumentSetStyle
    {
        /// <summary>
        /// No specific style is desired or specified.
        /// </summary>
        Unspecified,

        /// <summary>
        /// The style of simple Windows command-line tools.
        /// </summary>
        WindowsCommandLine,

        /// <summary>
        /// The style of PowerShell cmdlets.
        /// </summary>
        PowerShell,

        /// <summary>
        /// The style of apps and scripts implemented using getopt and its
        /// default formatting.
        /// </summary>
        GetOpt
    }
}
