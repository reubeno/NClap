using System;
using System.IO;
using NClap.ConsoleInput;

namespace NClap.Repl
{
    /// <summary>
    /// Console implementation of <see cref="ILoopClient" />.
    /// </summary>
    internal class ConsoleLoopClient : ILoopClient
    {
        private readonly TextWriter _error;

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="reader">Console reader to use.</param>
        /// <param name="error">The text writer to receive error output.</param>
        public ConsoleLoopClient(IConsoleReader reader, TextWriter error = null)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            Reader = reader;
            _error = error;
        }

        /// <summary>
        /// The string to be displayed when prompting for input.
        /// </summary>
        public string Prompt
        {
            get { return Reader.LineInput.Prompt; }
            set { Reader.LineInput.Prompt = value; }
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
        public void OnError(string message) => _error?.WriteLine(message);

        /// <summary>
        /// Displays the input prompt.
        /// </summary>
        public void DisplayPrompt() => Reader.LineInput.DisplayPrompt();
    }
}
