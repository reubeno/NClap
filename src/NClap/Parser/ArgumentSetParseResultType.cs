namespace NClap.Parser
{
    /// <summary>
    /// Describes a result state for parsing an argument set.
    /// </summary>
    internal enum ArgumentSetParseResultType
    {
        /// <summary>
        /// Parser is ready to parse more arguments.
        /// </summary>
        Ready,

        /// <summary>
        /// Parser has encountered an unknown named argument.
        /// </summary>
        UnknownNamedArgument,

        /// <summary>
        /// Parser has encountered an unknown positional argument.
        /// </summary>
        UnknownPositionalArgument,

        /// <summary>
        /// Parser has failed to parse an argument.
        /// </summary>
        FailedParsing,

        /// <summary>
        /// Parser has failed to finalize an argument set.
        /// </summary>
        FailedFinalizing,

        /// <summary>
        /// Parser has encountered an invalid answer file.
        /// </summary>
        InvalidAnswerFile,

        /// <summary>
        /// Parser is waiting for an option argument.
        /// </summary>
        RequiresOptionArgument
    }
}