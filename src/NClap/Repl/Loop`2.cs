using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
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
    /// <typeparam name="TContext">Type of the context object passed to
    /// all verbs.</typeparam>
    public class Loop<TVerbType, TContext> where TVerbType : struct
    {
        private readonly TContext _context;
        private readonly IReadOnlyDictionary<TVerbType, VerbAttribute> _verbMap;
        private readonly IReadOnlyList<string> _verbNames;

        /// <summary>
        /// Constructor that requires an explicit implementation of
        /// <see cref="ILoopClient"/>.
        /// </summary>
        /// <param name="loopClient">The client to use.</param>
        /// <param name="options">Options for loop.</param>
        /// <param name="context">Caller-provided context object.</param>
        public Loop(ILoopClient loopClient, LoopOptions options, TContext context) : this(options, context)
        {
            if (loopClient == null)
            {
                throw new ArgumentNullException(nameof(loopClient));
            }

            Client = loopClient;
        }

        /// <summary>
        /// Constructor that creates a loop with a default client.
        /// </summary>
        /// <param name="parameters">Optionally provides parameters controlling
        /// the loop's input and output behaviors; if not provided, default
        /// parameters are used.</param>
        /// <param name="options">Options for loop.</param>
        /// <param name="context">Caller-provided context object.</param>
        public Loop(LoopInputOutputParameters parameters, LoopOptions options, TContext context) : this(options, context)
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

            var reader = new ConsoleReader(lineInput, consoleInput, consoleOutput, keyBindingSet);

            var consoleClient = new ConsoleLoopClient(
                reader,
                parameters?.ErrorWriter ?? Console.Error);

            consoleClient.Reader.CommentCharacter = options?.EndOfLineCommentCharacter;

            Client = consoleClient;
        }

        /// <summary>
        /// Shared private constructor used internally by all public
        /// constructors.
        /// </summary>
        /// <param name="options">Options for loop.</param>
        /// <param name="context">Caller-provided context object.</param>
        private Loop(LoopOptions options, TContext context)
        {
            EndOfLineCommentCharacter = options?.EndOfLineCommentCharacter;

            _context = context;

            _verbMap = typeof(TVerbType).GetMembers().SelectMany(
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
        /// The character that starts a comment.
        /// </summary>
        public char? EndOfLineCommentCharacter { get; set; }

        /// <summary>
        /// Executes the loop.
        /// </summary>
        public void Execute()
        {
            while (ExecuteOnce())
            {
            }
        }

        private static bool TryParseVerb(string value, out TVerbType verbType)
        {
            return Enum.TryParse(value, true /* ignore case */, out verbType);
        }

        private bool ExecuteOnce()
        {
            Client.DisplayPrompt();

            var args = ReadInput();

            if (args == null)
            {
                return false;
            }

            if (args.Length == 0)
            {
                return true;
            }

            TVerbType verbType;
            if (!TryParseVerb(args[0], out verbType))
            {
                Client.OnError(string.Format(CultureInfo.CurrentCulture, Strings.UnrecognizedVerb, args[0]));
                return true;
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
                return new string[] { };
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

        private bool Execute(TVerbType verbType, IEnumerable<string> args)
        {
            VerbAttribute attrib;
            if (!_verbMap.TryGetValue(verbType, out attrib))
            {
                throw new NotSupportedException();
            }

            var implementingType = attrib.GetImplementingType(typeof(TVerbType));
            if (implementingType != null)
            {
                var constructor = implementingType.GetConstructor(new Type[] { });
                if (constructor == null)
                {
                    Client.OnError(string.Format(CultureInfo.CurrentCulture, Strings.NoAccessibleParameterlessConstructor, implementingType.FullName));
                    return true;
                }

                var verb = constructor.Invoke(new object[] { }) as IVerb<TContext>;
                if (verb == null)
                {
                    Client.OnError(string.Format(CultureInfo.CurrentCulture, Strings.ImplementingTypeNotIVerb, implementingType.FullName, typeof(IVerb).FullName));
                    return true;
                }

                var options = new CommandLineParserOptions
                {
                    Context = _context,
                    Reporter = error => Client.OnError(error.ToString().TrimEnd())
                };

                if (!CommandLineParser.Parse(args.ToList(), verb, options))
                {
                    Client.OnError(Strings.InvalidUsage);
                    return true;
                }

                verb.Execute(_context);
            }

            return !attrib.Exits;
        }

        internal IEnumerable<string> GenerateCompletions(IEnumerable<string> tokens, int indexOfTokenToComplete)
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
            TVerbType verbType;

            if (!TryParseVerb(verbToken, out verbType))
            {
                return emptyCompletions();
            }

            VerbAttribute attrib;
            if (!_verbMap.TryGetValue(verbType, out attrib))
            {
                return emptyCompletions();
            }

            var implementingType = attrib.GetImplementingType(typeof(TVerbType));
            if (implementingType == null)
            {
                return emptyCompletions();
            }

            var constructor = implementingType.GetConstructor(new Type[] { });

            Func<object> parsedObjectFactory = null;
            if (constructor != null)
            {
                parsedObjectFactory = () => constructor.Invoke(new object[] { });
            }

            var options = new CommandLineParserOptions { Context = _context };
            return CommandLineParser.GetCompletions(
                implementingType,
                tokenList.Skip(1),
                indexOfTokenToComplete - 1,
                options,
                parsedObjectFactory);
        }
    }
}
