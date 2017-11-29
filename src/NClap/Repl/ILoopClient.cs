using NClap.ConsoleInput;

namespace NClap.Repl
{
    /// <summary>
    /// Interface provided by REPL view.
    /// </summary>
    public interface ILoopClient
    {
        /// <summary>
        /// The loop prompt.
        /// </summary>
        string Prompt { get; set; }

        /// <summary>
        /// The character that starts a comment.
        /// </summary>
        char? EndOfLineCommentCharacter { get; }

        /// <summary>
        /// Optionally provides a token completer that the loop client may choose to use.
        /// </summary>
        ITokenCompleter TokenCompleter { get; set; }
        
        /// <summary>
        /// Displays the loop prompt.
        /// </summary>
        void DisplayPrompt();

        /// <summary>
        /// Reads a line of text input.
        /// </summary>
        /// <returns>The read line.</returns>
        string ReadLine();

        /// <summary>
        /// Notifies the client of a continuable error.
        /// </summary>
        /// <param name="message">The message if one is available, or null if
        /// there is no more input.</param>
        void OnError(string message);
    }
}
