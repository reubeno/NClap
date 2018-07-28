using NClap.ConsoleInput;
using NClap.Utilities;

namespace NClap.Repl
{
    /// <summary>
    /// Interface provided by REPL view.
    /// </summary>
    public interface ILoopClient
    {
        /// <summary>
        /// The loop prompt. If you wish to use a <see cref="ColoredString"/> as your
        /// prompt, you should use the <see cref="PromptWithColor"/> property instead.
        /// </summary>
        string Prompt { get; set; }

        /// <summary>
        /// The loop prompt (with color).
        /// </summary>
        ColoredString? PromptWithColor { get; set; }

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
