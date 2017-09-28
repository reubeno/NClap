namespace NClap.ConsoleInput
{
    /// <summary>
    /// Assorted console utilities exported for use outside this assembly.
    /// </summary>
    public static class ConsoleUtilities
    {
        /// <summary>
        /// Reads a line of input from the console.
        /// </summary>
        /// <returns>The read string.</returns>
        public static string ReadLine() => new ConsoleReader().ReadLine();
    }
}
