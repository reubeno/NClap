namespace NClap.ConsoleInput
{
    /// <summary>
    /// The result of processing a console input operation.
    /// </summary>
    internal enum ConsoleInputOperationResult
    {
        /// <summary>
        /// The operation was handled, but more input is available.
        /// </summary>
        Normal,

        /// <summary>
        /// The event was handled, and the end of the input line was reached.
        /// </summary>
        EndOfInputLine,

        /// <summary>
        /// The event was handled, and the end of the input stream was reached.
        /// </summary>
        EndOfInputStream
    }
}
