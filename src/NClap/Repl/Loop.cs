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

namespace NClap.Repl
{
    /// <summary>
    /// An interactive REPL loop.
    /// </summary>
    /// <typeparam name="TVerbType">Enum type that defines possible verbs.
    /// </typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
    public class Loop<TVerbType> where TVerbType : struct
    {
        private readonly IReadOnlyDictionary<TVerbType, VerbAttribute> _verbMap;
        private readonly IReadOnlyList<string> _verbNames;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Loop() : this(null)
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

            _verbMap = typeof(TVerbType).GetTypeInfo().GetMembers().SelectMany(
                member => member.GetCustomAttributes(typeof(VerbAttribute), false)
                                .Cast<VerbAttribute>()
                                .Select(attrib => Tuple.Create((TVerbType)Enum.Parse(typeof(TVerbType), member.Name), attrib)))
                .ToDictionary(pair => pair.Item1, pair => pair.Item2);

            _verbNames = _verbMap.Keys.Select(verb => verb.ToString()).ToList();
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
            while (ExecuteOnce() != VerbResult.Terminate)
            {
            }
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
        public IEnumerable<string> GenerateCompletions(IEnumerable<string> tokens, int indexOfTokenToComplete)
        {
            Func<IEnumerable<string>> emptyCompletions = Enumerable.Empty<string>;

            var tokenList = tokens.ToList();

            var tokenToComplete = string.Empty;
            if (indexOfTokenToComplete < tokenList.Count)
            {
                tokenToComplete = tokenList[indexOfTokenToComplete];
            }

            if (indexOfTokenToComplete == 0)
            {
                return _verbNames.Where(command => command.StartsWith(tokenToComplete, StringComparison.CurrentCultureIgnoreCase))
                                 .OrderBy(command => command, StringComparer.OrdinalIgnoreCase);
            }

            if (tokenList.Count < 1)
            {
                return emptyCompletions();
            }

            var verbToken = tokenList[0];

            if (!TryParseVerb(verbToken, out TVerbType verbType))
            {
                return emptyCompletions();
            }

            if (!_verbMap.TryGetValue(verbType, out VerbAttribute attrib))
            {
                return emptyCompletions();
            }

            var implementingType = attrib.GetImplementingType(typeof(TVerbType));
            if (implementingType == null)
            {
                return emptyCompletions();
            }

            var constructor = implementingType.GetTypeInfo().GetConstructor(Array.Empty<Type>());

            Func<object> parsedObjectFactory = null;
            if (constructor != null)
            {
                parsedObjectFactory = () => constructor.Invoke(Array.Empty<object>());
            }

            var options = new CommandLineParserOptions();
            return CommandLineParser.GetCompletions(
                implementingType,
                tokenList.Skip(1),
                indexOfTokenToComplete - 1,
                options,
                parsedObjectFactory);
        }

        private static bool TryParseVerb(string value, out TVerbType verbType) =>
            Enum.TryParse(value, true /* ignore case */, out verbType);

        private VerbResult ExecuteOnce()
        {
            Client.DisplayPrompt();

            var args = ReadInput();

            if (args == null)
            {
                return VerbResult.Terminate;
            }

            if (args.Length == 0)
            {
                return VerbResult.Success;
            }

            if (!TryParseVerb(args[0], out TVerbType verbType))
            {
                Client.OnError(string.Format(CultureInfo.CurrentCulture, Strings.UnrecognizedVerb, args[0]));
                return VerbResult.UsageError;
            }

            return Execute(verbType, args.Skip(1));
        }

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

        private VerbResult Execute(TVerbType verbType, IEnumerable<string> args)
        {
            if (!_verbMap.TryGetValue(verbType, out VerbAttribute attrib))
            {
                throw new NotSupportedException();
            }

            var result = VerbResult.Success;

            var implementingType = attrib.GetImplementingType(typeof(TVerbType));
            if (implementingType != null)
            {
                var constructor = implementingType.GetTypeInfo().GetConstructor(Array.Empty<Type>());
                if (constructor == null)
                {
                    Client.OnError(string.Format(CultureInfo.CurrentCulture, Strings.NoAccessibleParameterlessConstructor, implementingType.FullName));
                    return VerbResult.RuntimeFailure;
                }

                var verb = constructor.Invoke(Array.Empty<object>()) as IVerb;
                if (verb == null)
                {
                    Client.OnError(string.Format(CultureInfo.CurrentCulture, Strings.ImplementingTypeNotIVerb, implementingType.FullName, typeof(IVerb).FullName));
                    return VerbResult.RuntimeFailure;
                }

                var options = new CommandLineParserOptions
                {
                    Reporter = error => Client.OnError(error.ToString().TrimEnd())
                };

                if (!CommandLineParser.Parse(args.ToList(), verb, options))
                {
                    Client.OnError(Strings.InvalidUsage);
                    return VerbResult.UsageError;
                }

                result = verb.ExecuteAsync(CancellationToken.None).Result;
            }

            return result;
        }
    }
}
