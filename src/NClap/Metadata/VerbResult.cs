namespace NClap.Metadata
{
    /// <summary>
    /// Result from a verb's execution.
    /// </summary>
    public enum VerbResult
    {
        /// <summary>
        /// The verb completed successfully.
        /// </summary>
        Success,
        
        /// <summary>
        /// The verb requested the termination of the caller.
        /// </summary>
        Terminate,

        /// <summary>
        /// The verb detected a usage or syntax error.
        /// </summary>
        UsageError,

        /// <summary>
        /// The verb experienced a runtime failure.
        /// </summary>
        RuntimeFailure
    }
}