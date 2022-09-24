using System;
using System.Collections.Generic;
using System.Linq;
using NClap.ConsoleInput;
using NClap.Help;
using NClap.Metadata;
using NClap.Parser;
using NClap.Utilities;

namespace NClap
{
    /// <summary>
    /// A delegate used in error reporting.
    /// </summary>
    /// <param name="message">Message to report.</param>
    public delegate void ErrorReporter(ColoredMultistring message);

    /// <summary>
    /// Command-line parser.
    /// </summary>
    public static class CommandLineParser
    {
        /// <summary>
        /// Default <see cref="ErrorReporter" /> used by this class.
        /// </summary>
        public static ErrorReporter DefaultReporter { get; } = BasicConsole.Default.Write;

        /// <summary>
        /// Tries to parse the given string arguments into a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the destination object; this type should use
        /// appropriate NClap attributes to annotate and define options.</typeparam>
        /// <param name="arguments">The string arguments to parse.</param>
        /// <param name="result">On success, returns the constructed result object.</param>
        /// <returns>True on success; false otherwise.</returns>
        public static bool TryParse<T>(IEnumerable<string> arguments, out T result)
            where T : class, new() =>
            TryParse(arguments, null, out result);

        /// <summary>
        /// Tries to parse the given string arguments into a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the destination object; this type should use
        /// appropriate NClap attributes to annotate and define options.</typeparam>
        /// <param name="arguments">The string arguments to parse.</param>
        /// <param name="options">Options describing how to parse.</param>
        /// <param name="result">On success, returns the constructed result object.</param>
        /// <returns>True on success; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="arguments"/>
        /// is null.</exception>
        public static bool TryParse<T>(IEnumerable<string> arguments, CommandLineParserOptions options, out T result)
            where T : class, new()
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            var destination = new T();
            var argSet = AttributeBasedArgumentDefinitionFactory.CreateArgumentSet(
                destination.GetType(),
                attribute: null,
                defaultValues: destination,
                serviceConfigurer: options?.ServiceConfigurer);

            if (!TryParse(argSet, arguments, options, destination))
            {
                result = null;
                return false;
            }

            result = destination;
            return true;
        }

        /// <summary>
        /// Tries to parse the given string arguments into the provided instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the destination object; this type should use
        /// appropriate NClap attributes to annotate and define options.</typeparam>
        /// <param name="arguments">The string arguments to parse.</param>
        /// <param name="destination">The object to parse into.</param>
        /// <param name="options">Options describing how to parse.</param>
        /// <returns>True on success; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="arguments"/> or
        /// <paramref name="destination" /> is null.</exception>
        public static bool TryParse<T>(IEnumerable<string> arguments, T destination, CommandLineParserOptions options)
            where T : class
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (destination == null) throw new ArgumentNullException(nameof(arguments));

            var argSet = AttributeBasedArgumentDefinitionFactory.CreateArgumentSet(
                destination.GetType(),
                attribute: null,
                defaultValues: destination,
                serviceConfigurer: options?.ServiceConfigurer);
            return TryParse(argSet, arguments, options, destination);
        }

        /// <summary>
        /// Tries to parse the given string arguments into the provided instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the destination object.</typeparam>
        /// <param name="argSet">Definition of the argument set to be parsing.</param>
        /// <param name="arguments">The string arguments to parse.</param>
        /// <param name="options">Options describing how to parse.</param>
        /// <param name="destination">The object to parse into.</param>
        /// <returns>True on success; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="arguments"/> or
        /// <paramref name="destination" /> is null.</exception>
        internal static bool TryParse<T>(ArgumentSetDefinition argSet, IEnumerable<string> arguments, CommandLineParserOptions options, T destination)
        {
            if (options == null)
            {
                options = new CommandLineParserOptions();
            }

            //
            // Buffer output to the reporter; suppress it if we find afterwards
            // that the user just wanted to see help information.
            //

            var reportedLines = new List<ColoredMultistring>();
            var actualReporter = options.Reporter;

            options = options.DeepClone();
            options.Reporter = s => reportedLines.Add(s);

            //
            // Parse!
            //

            var parser = new ArgumentSetParser(argSet, options);

            var parseResult = parser.ParseArgumentList(arguments, destination).IsReady;

            var parserArgSet = parser.ArgumentSet;

            //
            // See if the user requested help output; if so, then suppress any errors.
            //

            if ((destination is IArgumentSetWithHelp helpArgs) && helpArgs.Help && actualReporter != null)
            {
                actualReporter(GetUsageInfo(parserArgSet, options.HelpOptions, destination));
                return false;
            }

            //
            // Okay, now flush any reported output.
            //

            if (actualReporter != null)
            {
                foreach (var line in reportedLines)
                {
                    actualReporter.Invoke(line);
                }
            }

            //
            // If we failed to parse and if the caller requested it, then display usage information.
            //

            if (!parseResult && options.DisplayUsageInfoOnError && actualReporter != null)
            {
                actualReporter(ColoredMultistring.FromString(Environment.NewLine));
                actualReporter(GetUsageInfo(parserArgSet, options.HelpOptions, destination));
            }

            return parseResult;
        }

        /// <summary>
        /// Formats a parsed set of arguments back into tokenized string form.
        /// </summary>
        /// <typeparam name="T">Type of the parsed arguments object.</typeparam>
        /// <param name="value">The parsed argument set.</param>
        /// <returns>The tokenized string.</returns>
        public static IEnumerable<string> Format<T>(T value) => Format(value, null);

        /// <summary>
        /// Formats a parsed set of arguments back into tokenized string form.
        /// </summary>
        /// <typeparam name="T">Type of the parsed arguments object.</typeparam>
        /// <param name="value">The parsed argument set.</param>
        /// <param name="options">Optionally provides parser options.</param>
        /// <returns>The tokenized string.</returns>
        public static IEnumerable<string> Format<T>(T value, CommandLineParserOptions options)
        {
            var argSet = AttributeBasedArgumentDefinitionFactory.CreateArgumentSet(
                typeof(T),
                attribute: null,
                serviceConfigurer: options?.ServiceConfigurer);

            // N.B. We intentionally convert the arguments enumeration to a list,
            // as we're expecting to mutate it in the loop.
            foreach (var arg in argSet.AllArguments.ToList())
            {
                if (arg.GetValue(value) is IArgumentProvider argProvider)
                {
                    var definingType = argProvider.GetTypeDefiningArguments();
                    if (definingType != null)
                    {
                        AttributeBasedArgumentDefinitionFactory.AddToArgumentSet(argSet, definingType,
                            fixedDestination: argProvider.GetDestinationObject(),
                            containingArgument: arg,
                            serviceConfigurer: options?.ServiceConfigurer);
                    }
                }
            }

            return OrderArgumentsByContainer(argSet.AllArguments)
                .Select(arg => new { Argument = arg, Value = arg.GetValue(value) })
                .Where(argAndValue => (argAndValue.Value != null) && !argAndValue.Value.Equals(argAndValue.Argument.DefaultValue))
                .SelectMany(argAndValue => argAndValue.Argument.Format(argAndValue.Value))
                .Where(formattedValue => !string.IsNullOrWhiteSpace(formattedValue));
        }

        /// <summary>
        /// Returns a usage string for command line argument parsing. Use
        /// argument attributes to control parsing behavior.
        /// </summary>
        /// <param name="type">Type of the parsed arguments object.</param>
        /// <param name="options">Options for generating usage info.</param>
        /// <param name="defaultValues">Optionally provides an object with
        /// default values.</param>
        /// <param name="serviceConfigurer">Optionally provides a service
        /// configurer to use.</param>
        /// <returns>Printable string containing a user friendly description of
        /// command line arguments.</returns>
        [CLSCompliant(false)]
        public static ColoredMultistring GetUsageInfo(
            Type type,
            ArgumentSetHelpOptions options = null,
            object defaultValues = null,
            ServiceConfigurer serviceConfigurer = null)
        {
            var argSet = AttributeBasedArgumentDefinitionFactory.CreateArgumentSet(
                type,
                attribute: null,
                defaultValues: defaultValues,
                serviceConfigurer: serviceConfigurer);

            return GetUsageInfo(argSet, options, null);
        }

        /// <summary>
        /// Generates a logo string for the application's entry assembly, or
        /// the assembly containing this method if no entry assembly could
        /// be found.
        /// </summary>
        /// <returns>The logo string.</returns>
        public static string GetLogo() => ArgumentSetUsageInfo.GetLogo(null);

        /// <summary>
        /// Generate possible completions for the specified set of command-line
        /// tokens.
        /// </summary>
        /// <param name="type">Type of the parsed arguments object.</param>
        /// <param name="tokens">The tokens.</param>
        /// <param name="indexOfTokenToComplete">Index of the token to complete.
        /// </param>
        /// <returns>The candidate completions for the specified token.
        /// </returns>
        public static IEnumerable<string> GetCompletions(Type type, IEnumerable<string> tokens, int indexOfTokenToComplete) =>
            GetCompletions(type, tokens, indexOfTokenToComplete, null /* options */);

        /// <summary>
        /// Generate possible completions for the specified set of command-line
        /// tokens.
        /// </summary>
        /// <param name="type">Type of the parsed arguments object.</param>
        /// <param name="tokens">The tokens.</param>
        /// <param name="indexOfTokenToComplete">Index of the token to complete.
        /// </param>
        /// <param name="options">Parsing options.</param>
        /// <returns>The candidate completions for the specified token.
        /// </returns>
        public static IEnumerable<string> GetCompletions(Type type, IEnumerable<string> tokens, int indexOfTokenToComplete, CommandLineParserOptions options) =>
            GetCompletions(type, tokens, indexOfTokenToComplete, options, null /* object factory */);

        /// <summary>
        /// Generate possible completions for the specified command line.
        /// </summary>
        /// <param name="type">Type of the parsed arguments object.</param>
        /// <param name="commandLineToComplete">The command line to complete.</param>
        /// <param name="charIndexOfCursor">Character index of the completion cursor.
        /// </param>
        /// <returns>The candidate completions for the specified token.
        /// </returns>
        public static IEnumerable<string> GetCompletions(Type type, string commandLineToComplete, int charIndexOfCursor) =>
            GetCompletions(type, commandLineToComplete, charIndexOfCursor, 0, null);

        /// <summary>
        /// Generate possible completions for the specified command line.
        /// </summary>
        /// <param name="type">Type of the parsed arguments object.</param>
        /// <param name="commandLineToComplete">The command line to complete. Both single and double quote
        /// characters are interpreted as denoting single tokens that may contain embedded whitespace.</param>
        /// <param name="charIndexOfCursor">Character index of the completion cursor.
        /// </param>
        /// <param name="tokensToSkip">Number of tokens to skip from start of command
        /// line.</param>
        /// <param name="options">Parsing options.</param>
        /// <returns>The candidate completions for the specified token.
        /// </returns>
        public static IEnumerable<string> GetCompletions(Type type, string commandLineToComplete, int charIndexOfCursor, int tokensToSkip, CommandLineParserOptions options)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (commandLineToComplete == null) throw new ArgumentNullException(nameof(commandLineToComplete));

            const TokenizerOptions tokenizerOptions =
                TokenizerOptions.AllowPartialInput |
                TokenizerOptions.HandleDoubleQuoteAsTokenDelimiter |
                TokenizerOptions.HandleSingleQuoteAsTokenDelimiter;

            var tokens = StringUtilities.Tokenize(commandLineToComplete, tokenizerOptions).ToList();

            int index;
            for (index = 0; index < tokens.Count; ++index)
            {
                var token = tokens[index];
                if (charIndexOfCursor >= token.OuterStartingOffset &&
                    charIndexOfCursor <= token.OuterEndingOffset)
                {
                    break;
                }
            }

            if (index < tokensToSkip)
            {
                return Enumerable.Empty<string>();
            }

            return GetCompletions(
                type,
                tokens.Skip(tokensToSkip).Select(t => t.ToString()),
                index - tokensToSkip,
                options);
        }

        /// <summary>
        /// Generate possible completions for the specified set of command-line
        /// tokens.
        /// </summary>
        /// <param name="type">Type of the parsed arguments object.</param>
        /// <param name="tokens">The tokens.</param>
        /// <param name="indexOfTokenToComplete">Index of the token to complete.
        /// </param>
        /// <param name="options">Parsing options.</param>
        /// <param name="destObjectFactory">If non-null, provides a factory
        /// function that can be used to create an object suitable to being
        /// filled out by this parser instance.</param>
        /// <returns>The candidate completions for the specified token.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/>
        /// or <paramref name="tokens"/> is null.</exception>
        public static IEnumerable<string> GetCompletions(Type type, IEnumerable<string> tokens, int indexOfTokenToComplete, CommandLineParserOptions options, Func<object> destObjectFactory)
        {
            var argSet = AttributeBasedArgumentDefinitionFactory.CreateArgumentSet(
                type,
                attribute: null,
                serviceConfigurer: options?.ServiceConfigurer);

            return GetCompletions(argSet, tokens, indexOfTokenToComplete, options, destObjectFactory);
        }

        /// <summary>
        /// Generate possible completions for the specified set of command-line
        /// tokens.
        /// </summary>
        /// <param name="argSet">Argument set definition.</param>
        /// <param name="tokens">The tokens.</param>
        /// <param name="indexOfTokenToComplete">Index of the token to complete.
        /// </param>
        /// <param name="options">Parsing options.</param>
        /// <param name="destObjectFactory">If non-null, provides a factory
        /// function that can be used to create an object suitable to being
        /// filled out by this parser instance.</param>
        /// <returns>The candidate completions for the specified token.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="argSet"/>
        /// or <paramref name="tokens"/> is null.</exception>
        internal static IEnumerable<string> GetCompletions(ArgumentSetDefinition argSet, IEnumerable<string> tokens, int indexOfTokenToComplete, CommandLineParserOptions options, Func<object> destObjectFactory)
        {
            if (argSet == null) throw new ArgumentNullException(nameof(argSet));
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));

            var parser = new ArgumentSetParser(argSet, options ?? CommandLineParserOptions.Quiet());
            return parser.GetCompletions(tokens, indexOfTokenToComplete, destObjectFactory);
        }

        /// <summary>
        /// Returns a usage string for command line argument parsing.
        /// </summary>
        /// <param name="argSet">Definition of argument set.</param>
        /// <param name="options">Options for generating usage info.</param>
        /// <param name="destination">Optionally provides an object with
        /// default values.</param>
        /// <returns>Printable string containing a user friendly description of
        /// command line arguments.</returns>
        internal static ColoredMultistring GetUsageInfo(
            ArgumentSetDefinition argSet,
            ArgumentSetHelpOptions options,
            object destination)
        {
            // Default options.
            if (options == null)
            {
                options = new ArgumentSetHelpOptions();
            }

            // Default max width.
            if (!options.MaxWidth.HasValue)
            {
                options = options.With().MaxWidth(GetCurrentConsoleWidth());
            }

            // Construct renderer.
            var renderer = new ArgumentSetHelpRenderer(options);

            // Render.
            return renderer.Format(argSet, destination);
        }

        private static int GetCurrentConsoleWidth()
        {
            // N.B. Leave room so that we don't cycle over to next line.
            var columns = BasicConsole.Default.WindowWidth;
            if (columns > 0)
            {
                --columns;
            }

            return columns;
        }

        private static IEnumerable<ArgumentDefinition> OrderArgumentsByContainer(IEnumerable<ArgumentDefinition> args)
        {
            var unsortedGroups = args.GroupBy(arg => arg.ContainingArgument).ToList();
            var sorted = new List<ArgumentDefinition>();
            var current = new List<ArgumentDefinition> { null };

            while (unsortedGroups.Count > 0)
            {
                if (current.Count == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(args));
                }

                var last = current;

                current = new List<ArgumentDefinition>();

                foreach (var item in last)
                {
                    var next = unsortedGroups.SingleOrDefault(group => group.Key == item);
                    if (next != null)
                    {
                        unsortedGroups.Remove(next);
                        current.AddRange(next);
                    }
                }

                sorted.AddRange(current);
            }

            return sorted;
        }
    }
}
