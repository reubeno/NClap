namespace NClap.Help
{
    /// <summary>
    /// Type of section in argument help output.
    /// </summary>
    internal enum ArgumentSetHelpSectionType
    {
        /// <summary>
        /// Argument set description.
        /// </summary>
        ArgumentSetDescription,

        /// <summary>
        /// Summary of separately documented enum values.
        /// </summary>
        EnumValues,

        /// <summary>
        /// Example usage information.
        /// </summary>
        Examples,

        /// <summary>
        /// Argument set logo.
        /// </summary>
        Logo,

        /// <summary>
        /// Optional parameters.
        /// </summary>
        OptionalParameters,

        /// <summary>
        /// Required parameters.
        /// </summary>
        RequiredParameters,

        /// <summary>
        /// Syntax summary.
        /// </summary>
        Syntax,
    }
}
