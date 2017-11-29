namespace NClap.Parser
{
    /// <summary>
    /// Describes a result state for parsing an argument set.
    /// </summary>
    internal enum ArgumentSetParseResultType
    {
        Ready,

        UnknownNamedArgument,
        UnknownPositionalArgument,
        FailedParsing,
        FailedFinalizing,
        InvalidAnswerFile,
        RequiresOptionArgument
    }
}