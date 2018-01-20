using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using NClap.ConsoleInput;
using NClap.Metadata;
using NClap.Parser;
using NClap.Utilities;

namespace NClap.Repl
{
    /// <summary>
    /// An interactive REPL loop.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "[Legacy]")]
    public class Loop
    {
        private class TokenCompleter : ITokenCompleter
        {
            private readonly Loop _loop;

            public TokenCompleter(Loop loop) { _loop = loop; }

            public IEnumerable<string> GetCompletions(IEnumerable<string> tokens, int tokenIndex) =>
                _loop.GetCompletions(tokens, tokenIndex);
        }

        private readonly Type _commandType;
        private readonly ArgumentSetDefinition _argSet;
        private readonly ILoopClient _client;
        private readonly Func<object> _objectFactory;

        /// <summary>
        /// Constructor that requires an explicit implementation of
        /// <see cref="ILoopClient"/>.
        /// </summary>
        /// <param name="commandType">Type that defines syntax for commands.</param>
        /// <param name="loopClient">The client to use.</param>
        /// <param name="argSetAttribute">Optionally provides attribute info
        /// for the argument set that will be dynamically created for this loop.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandType" />
        /// is null.</exception>
        public Loop(Type commandType, ILoopClient loopClient, ArgumentSetAttribute argSetAttribute = null)
        {
            if (commandType == null) throw new ArgumentNullException(nameof(commandType));

            _client = loopClient ?? throw new ArgumentNullException(nameof(loopClient));
            _client.TokenCompleter = new TokenCompleter(this);

            _commandType = ConstructCommandType(commandType, out _objectFactory);
            _argSet = ReflectionBasedParser.CreateArgumentSet(_commandType, attribute: argSetAttribute);
        }

        /// <summary>
        /// Constructor that creates a loop with a default client.
        /// </summary>
        /// <param name="commandType">Type that defines syntax for commands.</param>
        /// <param name="parameters">Optionally provides parameters controlling
        /// the loop's input and output behaviors; if not provided, default
        /// parameters are used.</param>
        /// <param name="argSetAttribute">Optionally provides attribute info
        /// for the argument set that will be dynamically created for this loop.</param>
        public Loop(Type commandType, LoopInputOutputParameters parameters = null, ArgumentSetAttribute argSetAttribute = null)
            : this(commandType, CreateClient(parameters ?? new LoopInputOutputParameters()), argSetAttribute)
        {
        }

        /// <summary>
        /// Utility for constructing loop clients.
        /// </summary>
        /// <param name="parameters">I/O parameters for loop.</param>
        /// <returns>A constructed loop client.</returns>
        public static ILoopClient CreateClient(LoopInputOutputParameters parameters)
        {
            var consoleInput = parameters.ConsoleInput ?? BasicConsole.Default;
            var consoleOutput = parameters.ConsoleOutput ?? BasicConsole.Default;
            var keyBindingSet = parameters.KeyBindingSet ?? ConsoleKeyBindingSet.Default;

            var lineInput = parameters.LineInput ?? new ConsoleLineInput(
                consoleOutput,
                new ConsoleInputBuffer(),
                new ConsoleHistory());

            lineInput.Prompt = parameters.Prompt ?? Strings.DefaultPrompt;

            var consoleReader = new ConsoleReader(lineInput, consoleInput, consoleOutput, keyBindingSet);

            var consoleClient = new ConsoleLoopClient(consoleReader);

            consoleClient.Reader.CommentCharacter = parameters.EndOfLineCommentCharacter;

            return consoleClient;
        }

        /// <summary>
        /// Executes the loop.
        /// </summary>
        public void Execute()
        {
            while (ExecuteOnce() != CommandResult.Terminate)
            {
                // Nothing to do in body.
            }
        }

        /// <summary>
        /// Executes one iteration of the loop.
        /// </summary>
        /// <returns>The result of executing.</returns>
        public CommandResult ExecuteOnce()
        {
            _client.DisplayPrompt();

            var readResult = ReadInput();
            if (readResult.IsNone)
            {
                return CommandResult.Terminate;
            }

            var args = readResult.Value;
            if (args.Count == 0)
            {
                return CommandResult.Success;
            }

            var options = new CommandLineParserOptions
            {
                Reporter = error => _client.OnError(error.ToString().TrimEnd()),
                DisplayUsageInfoOnError = false
            };

            var parsedArgs = ConstructObject();
            var parseResult = CommandLineParser.TryParse(
                _argSet,
                args,
                options,
                parsedArgs);

            if (!parseResult)
            {
                _client.OnError(Strings.InvalidUsage);
                return CommandResult.UsageError;
            }

            return parsedArgs.ExecuteAsync(CancellationToken.None).Result;
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
        internal IEnumerable<string> GetCompletions(IEnumerable<string> tokens, int indexOfTokenToComplete) =>
            CommandLineParser.GetCompletions(
                _commandType,
                tokens,
                indexOfTokenToComplete,
                CommandLineParserOptions.Quiet(),
                _objectFactory);

        /// <summary>
        /// Reads and tokenizes a line of input.
        /// </summary>
        /// <returns>None if we're at the end of the input stream; otherwise, the
        /// possibly empty list of tokens.</returns>
        private Maybe<IReadOnlyList<string>> ReadInput()
        {
            var line = _client.ReadLine();

            // Return None if we're at the end of the input stream.
            if (line == null)
            {
                return new None();
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
                _client.OnError(string.Format(CultureInfo.CurrentCulture, Strings.ExceptionWasThrownParsingInputLine, ex));
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
            if (!_client.EndOfLineCommentCharacter.HasValue)
            {
                return input;
            }

            var commentStartIndex = input.IndexOf(_client.EndOfLineCommentCharacter.Value);
            if (commentStartIndex >= 0)
            {
                input = input.Substring(0, commentStartIndex);
            }

            return input;
        }

        private static Type ConstructCommandType(Type inputType, out Func<object> factory)
        {
            Type loopType;

            // See if it implements ICommand; if so, use it as is.
            if (typeof(ICommand).GetTypeInfo().IsAssignableFrom(inputType.GetTypeInfo()))
            {
                loopType = inputType;
            }

            // See if it is an enum; if so, use it as the inner type for a command group.
            else if (inputType.GetTypeInfo().IsEnum)
            {
                loopType = typeof(CommandGroup<>).MakeGenericType(new[] { inputType });
            }

            // Otherwise, we can't do anything with it.
            else
            {
                throw new NotSupportedException($"Type not supported as command for loop: {inputType.FullName}");
            }

            // Now make sure there's a parameterless constructor.
            var constructor = loopType.GetTypeInfo().GetConstructor(Array.Empty<Type>());
            if (constructor == null)
            {
                throw new NotSupportedException($"Type missing parameterless constructor, not usable for loop: {loopType.FullName}");
            }

            factory = () => constructor.Invoke(Array.Empty<object>());
            return loopType;
        }

        private ICommand ConstructObject() => (ICommand)_objectFactory.Invoke();
    }
}
