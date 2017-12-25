namespace NClap.Help
{
    /// <summary>
    /// Grouping mode for arguments.
    /// </summary>
    internal enum ArgumentGroupingMode
    {
        /// <summary>
        /// Do not group arguments.
        /// </summary>
        None,

        /// <summary>
        /// Group required arguments vs. optional arguments.
        /// </summary>
        RequiredVersusOptional,

        /// <summary>
        /// Group named arguments vs. positional arguments.
        /// </summary>
        NamedVersusPositional
    }
}
