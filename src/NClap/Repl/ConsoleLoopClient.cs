using System;
using NClap.ConsoleInput;
using NClap.Utilities;

namespace NClap.Repl
{
    /// <summary>
    /// Console implementation of <see cref="ILoopClient" />.
    /// </summary>
    internal class ConsoleLoopClient : ILoopClient
    {
        private readonly IConsoleOutput _output;
        private readonly ConsoleColor? _warningForegroundColor = ConsoleColor.Yellow;

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="reader">Console reader to use.</param>
        public ConsoleLoopClient(IConsoleReader reader)
        {
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _output = Reader.ConsoleOutput;
        }

        /// <summary>
        /// The string to be displayed when prompting for input.
        /// </summary>
        public string Prompt
        {
            get => Reader.LineInput.Prompt;
            set => Reader.LineInput.Prompt = value;
        }

        /// <summary>
        /// The loop prompt (with color).
        /// </summary>
        public ColoredString? PromptWithColor
        {
            get => Reader.LineInput.Prompt;
            set => Reader.LineInput.Prompt = value.GetValueOrDefault(ColoredString.Empty);
        }

        /// <summary>
        /// The character that starts a comment.
        /// </summary>
        public char? EndOfLineCommentCharacter { get; set; }

        /// <summary>
        /// Optionally provides a token completer that may be used.
        /// </summary>
        public ITokenCompleter TokenCompleter
        {
            get => Reader.LineInput.TokenCompleter;
            set => Reader.LineInput.TokenCompleter = value;
        }

        /// <summary>
        /// The client's console reader.
        /// </summary>
        public IConsoleReader Reader { get; }

        /// <summary>
        /// Reads a line of text input.
        /// </summary>
        /// <returns>The read line.</returns>
        public string ReadLine() => Reader.ReadLine();

        /// <summary>
        /// Notifies the client of a continuable error.
        /// </summary>
        /// <param name="message">The message if one is available, or null if
        /// there is no more input.</param>
        public void OnError(string message) =>
            _output?.Write(new ColoredString(message + Environment.NewLine, _warningForegroundColor));

        /// <summary>
        /// Displays the input prompt.
        /// </summary>
        public void DisplayPrompt() => Reader.LineInput.DisplayPrompt();
    }
}
