namespace NClap.ConsoleInput
{
    /// <summary>
    /// An advanced console reader.
    /// </summary>
    public interface IConsoleReader
    {
        /// <summary>
        /// The beginning-of-line comment character.
        /// </summary>
        char? CommentCharacter { get; set; }

        /// <summary>
        /// The console being used for input.
        /// </summary>
        IConsoleInput ConsoleInput { get; }

        /// <summary>
        /// The console being used for output.
        /// </summary>
        IConsoleOutput ConsoleOutput { get; }

        /// <summary>
        /// The inner line input object.
        /// </summary>
        IConsoleLineInput LineInput { get; }

        /// <summary>
        /// The console key bindings used by this console reader.
        /// </summary>
        IReadOnlyConsoleKeyBindingSet KeyBindingSet { get; }

        /// <summary>
        /// Reads a line of input text from the underlying console.
        /// </summary>
        /// <returns>The line of text, or null if the end of input was
        /// encountered.</returns>
        string ReadLine();
    }
}