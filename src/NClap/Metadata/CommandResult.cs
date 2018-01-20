namespace NClap.Metadata
{
    /// <summary>
    /// Result from a command's execution.
    /// </summary>
    public enum CommandResult
    {
        /// <summary>
        /// The command completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The command requested the termination of the caller.
        /// </summary>
        Terminate,

        /// <summary>
        /// The command detected a usage or syntax error.
        /// </summary>
        UsageError,

        /// <summary>
        /// The command experienced a runtime failure.
        /// </summary>
        RuntimeFailure
    }
}