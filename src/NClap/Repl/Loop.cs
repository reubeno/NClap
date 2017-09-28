using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using NClap.ConsoleInput;
using NClap.Metadata;

namespace NClap.Repl
{
    /// <summary>
    /// An interactive REPL loop.
    /// </summary>
    /// <typeparam name="TCommandType">Enum type that defines possible commands.
    /// </typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
    public class Loop<TCommandType> where TCommandType : struct
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Loop() : this((LoopInputOutputParameters)null)
        {
        }

        /// <summary>
        /// Constructor that requires an explicit implementation of
        /// <see cref="ILoopClient"/>.
        /// </summary>
        /// <param name="loopClient">The client to use.</param>
        /// <param name="options">Options for loop.</param>
        public Loop(ILoopClient loopClient, LoopOptions options = null) : this(options)
        {
            Client = loopClient ?? throw new ArgumentNullException(nameof(loopClient));
        }

        /// <summary>
        /// Constructor that creates a loop with a default client.
        /// </summary>
        /// <param name="parameters">Optionally provides parameters controlling
        /// the loop's input and output behaviors; if not provided, default
        /// parameters are used.</param>
        /// <param name="options">Options for loop.</param>
        public Loop(LoopInputOutputParameters parameters, LoopOptions options = null) : this(options)
        {
            var consoleInput = parameters?.ConsoleInput ?? BasicConsoleInputAndOutput.Default;
            var consoleOutput = parameters?.ConsoleOutput ?? BasicConsoleInputAndOutput.Default;
            var keyBindingSet = parameters?.KeyBindingSet ?? ConsoleKeyBindingSet.Default;

            var lineInput = parameters?.LineInput ?? new ConsoleLineInput(
                consoleOutput,
                new ConsoleInputBuffer(),
                new ConsoleHistory(),
                GenerateCompletions);

            lineInput.Prompt = parameters?.Prompt ?? Strings.DefaultPrompt;

            ConsoleReader = new ConsoleReader(lineInput, consoleInput, consoleOutput, keyBindingSet);

            var consoleClient = new ConsoleLoopClient(
                ConsoleReader,
                parameters?.ErrorWriter ?? Console.Error);

            consoleClient.Reader.CommentCharacter = options?.EndOfLineCommentCharacter;

            Client = consoleClient;
        }

        /// <summary>
        /// Shared private constructor used internally by all public
        /// constructors.
        /// </summary>
        /// <param name="options">Options for loop.</param>
        private Loop(LoopOptions options)
        {
            EndOfLineCommentCharacter = options?.EndOfLineCommentCharacter;
        }

        /// <summary>
        /// The client associated with this loop.
        /// </summary>
        public ILoopClient Client { get; }

        /// <summary>
        /// The console reader used by this loop, or null if none is present.
        /// </summary>
        public IConsoleReader ConsoleReader { get; }

        /// <summary>
        /// The character that starts a comment.
        /// </summary>
        public char? EndOfLineCommentCharacter { get; set; }

        /// <summary>
        /// Executes the loop.
        /// </summary>
        public void Execute()
        {
            while (ExecuteOnce() != CommandResult.Terminate)
            {
            }
        }

        /// <summary>
        /// Executes one iteration of the loop.
        /// </summary>
        /// <returns>The result of executing.</returns>
        public CommandResult ExecuteOnce()
        {
            Client.DisplayPrompt();

            var args = ReadInput();
            if (args == null)
            {
                return CommandResult.Terminate;
            }
            if (args.Length == 0)
            {
                return CommandResult.Success;
            }

            var options = new CommandLineParserOptions
            {
                Reporter = error => Client.OnError(error.ToString().TrimEnd())
            };

            var commandGroup = new CommandGroup<TCommandType>();
            if (!CommandLineParser.Parse(args, commandGroup, options))
            {
                Client.OnError(Strings.InvalidUsage);
                return CommandResult.UsageError;
            }

            return commandGroup.Execute();
        }

        /// <summary>
        /// Generates possible string completions for an input line to the
        /// loop.
        /// </summary>
        /// <param name="tokens">The tokens presently in the input line.</param>
        /// <param name="indexOfTokenToComplete">The 0-based index of the token
        /// from the input line to be completed.</param>
        /// <returns>An enumeration of the possible completions for the
        /// indicated token.</returns>
        internal static IEnumerable<string> GenerateCompletions(IEnumerable<string> tokens, int indexOfTokenToComplete) =>
            CommandLineParser.GetCompletions(
                typeof(CommandGroup<TCommandType>),
                tokens,
                indexOfTokenToComplete,
                null,
                () => new CommandGroup<TCommandType>());

        private string[] ReadInput()
        {
            var line = Client.ReadLine();

            // Return null if we're at the end of the input stream.
            if (line == null)
            {
                return null;
            }

            // Preprocess the line.
            line = Preprocess(line);

            try
            {
                // Parse the string into tokens.
                return CommandLineParser.Tokenize(line).Select(token => token.ToString()).ToArray();
            }
            catch (ArgumentException ex)
            {
                Client.OnError(string.Format(CultureInfo.CurrentCulture, Strings.ExceptionWasThrownParsingInputLine, ex));
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Preprocesses a line of input, primarily to remove comments.
        /// </summary>
        /// <param name="input">The line of input to preprocess.</param>
        /// <returns>The preprocessed result.</returns>
        private string Preprocess(string input)
        {
            if (!EndOfLineCommentCharacter.HasValue)
            {
                return input;
            }

            var commentStartIndex = input.IndexOf(EndOfLineCommentCharacter.Value);
            if (commentStartIndex >= 0)
            {
                input = input.Substring(0, commentStartIndex);
            }

            return input;
        }
    }
}
