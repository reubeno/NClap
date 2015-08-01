using System;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Abstract interface for interacting with an input console.
    /// </summary>
    public interface IConsoleInput
    {
        /// <summary>
        /// True if Control-C is treated as a normal input character; false if
        /// it's specially handled.
        /// </summary>
        bool TreatControlCAsInput { get; set; }

        /// <summary>
        /// Reads a key press from the console.
        /// </summary>
        /// <param name="suppressEcho">True to suppress auto-echoing the key's
        /// character; false to echo it as normal.</param>
        /// <returns>Info about the press.</returns>
        ConsoleKeyInfo ReadKey(bool suppressEcho);
    }
}
