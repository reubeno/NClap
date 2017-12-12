using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using NClap.ConsoleInput;
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
        /// Retrieves the current console's width in characters.
        /// </summary>
        internal static Func<int> GetConsoleWidth { get; set; } = () => Console.WindowWidth;

        /// <summary>
        /// Default console width in characters.
        /// </summary>
        private const int DefaultConsoleWidth = 80;

        /// <summary>
        /// Default <see cref="ErrorReporter" /> used by this class.
        /// </summary>
        public static ErrorReporter DefaultReporter { get; } = BasicConsoleInputAndOutput.Default.Write;

        /// <summary>
        /// Tries to parse the given string arguments into a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the destination object; this type should use
        /// appropriate NClap attributes to annotate and define options.</typeparam>
        /// <param name="arguments">The string arguments to parse.</param>
        /// <param name="result">On success, returns the constructed result object.</param>
        /// <returns>True on success; false otherwise.</returns>
        public static bool TryParse<T>(IEnumerable<string> arguments, out T result) where T : class, new() =>
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
        public static bool TryParse<T>(IEnumerable<string> arguments, CommandLineParserOptions options, out T result)
            where T : class, new()
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            var destination = new T();
            var argSet = ReflectionBasedParser.CreateArgumentSet(destination.GetType(), defaultValues: destination);

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
        public static bool TryParse<T>(IEnumerable<string> arguments, T destination, CommandLineParserOptions options)
            where T : class
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            if (destination == null) throw new ArgumentNullException(nameof(arguments));

            var argSet = ReflectionBasedParser.CreateArgumentSet(destination.GetType(), defaultValues: destination);
            return TryParse(argSet, arguments, options, destination);
        }

        internal static bool TryParse<T>(ArgumentSetDefinition argSet, IEnumerable<string> arguments, CommandLineParserOptions options, T destination)
        {
            if (options == null)
            {
                options = new CommandLineParserOptions { Reporter = DefaultReporter };
            }

            //
            // Buffer output to the reporter; suppress it if we find afterwards
            // that the user just wanted to see help information.
            //

            var reportedLines = new List<ColoredMultistring>();
            var actualReporter = options.Reporter;

            options = options.Clone();
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

            if ((destination is IHelpArguments helpArgs) && helpArgs.Help && actualReporter != null)
            {
                actualReporter(GetUsageInfo(parserArgSet, null, null, options.UsageInfoOptions, destination));
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
                actualReporter(GetUsageInfo(parserArgSet, null, null, options.UsageInfoOptions, destination));
            }

            return parseResult;
        }

        /// <summary>
        /// Formats a parsed set of arguments back into tokenized string form.
        /// </summary>
        /// <typeparam name="T">Type of the parsed arguments object.</typeparam>
        /// <param name="value">The parsed argument set.</param>
        /// <returns>The tokenized string.</returns>
        public static IEnumerable<string> Format<T>(T value)
        {
            var argSet = ReflectionBasedParser.CreateArgumentSet(typeof(T));
            foreach (var arg in argSet.AllArguments.ToList())
            {
                if (arg.GetValue(value) is IArgumentProvider argProvider)
                {
                    var definingType = argProvider.GetTypeDefiningArguments();
                    if (definingType != null)
                    {
                        ReflectionBasedParser.AddToArgumentSet(argSet, definingType,
                            fixedDestination: argProvider.GetDestinationObject(),
                            containingArgument: arg);
                    }
                }
            }

            return argSet.AllArguments
                .Select(arg => new { Argument = arg, Value = arg.GetValue(value) })
                .Where(argAndValue => (argAndValue.Value != null) && !argAndValue.Value.Equals(argAndValue.Argument.DefaultValue))
                .SelectMany(argAndValue => argAndValue.Argument.Format(argAndValue.Value))
                .Where(formattedValue => !string.IsNullOrWhiteSpace(formattedValue));
        }

        /// <summary>
        /// Returns a usage string for command line argument parsing. Use
        /// ArgumentAttributes to control parsing behavior. Formats the output
        /// to the width of the current console window.
        /// </summary>
        /// <param name="type">Type of the parsed arguments object to get usage
        /// info for.</param>
        /// <returns>Printable string containing a user friendly description of
        /// command line arguments.</returns>
        public static ColoredMultistring GetUsageInfo(Type type) => 
            GetUsageInfo(type, type.GetDefaultValue());

        /// <summary>
        /// Returns a usage string for command line argument parsing. Use
        /// ArgumentAttributes to control parsing behavior. Formats the output
        /// to the width of the current console window.
        /// </summary>
        /// <param name="type">Type of the parsed arguments object to get usage
        /// info for.</param>
        /// <param name="options">Options for generating usage info.</param>
        /// <returns>Printable string containing a user friendly description of
        /// command line arguments.</returns>
        public static ColoredMultistring GetUsageInfo(Type type, UsageInfoOptions options) =>
            GetUsageInfo(type, type.GetDefaultValue(), null, null, options);

        /// <summary>
        /// Returns a usage string for command line argument parsing. Use
        /// ArgumentAttributes to control parsing behavior. Formats the output
        /// to the width of the current console window.
        /// </summary>
        /// <param name="type">Type of the parsed arguments object.</param>
        /// <param name="defaultValues">Optionally provides an object with
        /// default values.</param>
        /// <returns>Printable string containing a user friendly description of
        /// command line arguments.</returns>
        public static ColoredMultistring GetUsageInfo(Type type, object defaultValues) =>
            GetUsageInfo(type, defaultValues, null);

        /// <summary>
        /// Returns a usage string for command line argument parsing. Use
        /// ArgumentAttributes to control parsing behavior. Formats the output
        /// to the width of the current console window.
        /// </summary>
        /// <param name="type">Type of the parsed arguments object.</param>
        /// <param name="defaultValues">Optionally provides an object with
        /// default values.</param>
        /// <param name="options">Options for generating usage info.</param>
        /// <returns>Printable string containing a user friendly description of
        /// command line arguments.</returns>
        public static ColoredMultistring GetUsageInfo(Type type, object defaultValues, UsageInfoOptions options) =>
            GetUsageInfo(type, defaultValues, null, null, options);

        /// <summary>
        /// Returns a Usage string for command line argument parsing. Use
        /// ArgumentAttributes to control parsing behavior.
        /// </summary>
        /// <param name="type">Type of the parsed arguments object.</param>
        /// <param name="defaultValues">Optionally provides an object with
        /// default values.</param>
        /// <param name="columns">The number of columns to format the output to.
        /// </param>
        /// <returns>Printable string containing a user friendly description of
        /// command-line arguments.</returns>
        public static ColoredMultistring GetUsageInfo(Type type, object defaultValues, int? columns) =>
            GetUsageInfo(type, defaultValues, columns, null, UsageInfoOptions.Default);

        /// <summary>
        /// Returns a usage string for command line argument parsing. Use
        /// argument attributes to control parsing behavior.
        /// </summary>
        /// <param name="type">Type of the parsed arguments object.</param>
        /// <param name="defaultValues">Optionally provides an object with
        /// default values.</param>
        /// <param name="columns">The number of columns to format the output to.
        /// </param>
        /// <param name="commandName">Command name to display in the usage
        /// information.</param>
        /// <param name="options">Options for generating usage info.</param>
        /// <returns>Printable string containing a user friendly description of
        /// command line arguments.</returns>
        public static ColoredMultistring GetUsageInfo(
            Type type,
            object defaultValues,
            int? columns,
            string commandName,
            UsageInfoOptions options) =>

            GetUsageInfo(ReflectionBasedParser.CreateArgumentSet(type, defaultValues: defaultValues),
                columns, commandName, options);

        /// <summary>
        /// Generates a logo string for the application's entry assembly, or
        /// the assembly containing this method if no entry assembly could
        /// be found.
        /// </summary>
        /// <returns>The logo string.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "It's not appropriate")]
        public static string GetLogo() => AssemblyUtilities.GetLogo();

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
        public static IEnumerable<string> GetCompletions(Type type, IEnumerable<string> tokens, int indexOfTokenToComplete, CommandLineParserOptions options, Func<object> destObjectFactory)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            var parser = new ArgumentSetParser(ReflectionBasedParser.CreateArgumentSet(type), options);
            return parser.GetCompletions(tokens, indexOfTokenToComplete, destObjectFactory);
        }

        /// <summary>
        /// Tokenizes the provided input text line, observing quotes.
        /// </summary>
        /// <param name="line">Input line to parse.</param>
        /// <param name="options">Options for tokenizing.</param>
        /// <returns>Enumeration of tokens.</returns>
        internal static IEnumerable<Token> Tokenize(string line, CommandLineTokenizerOptions options = CommandLineTokenizerOptions.None) =>
            StringUtilities.Tokenize(line, options.HasFlag(CommandLineTokenizerOptions.AllowPartialInput));

        internal static ColoredMultistring GetUsageInfo(
            ArgumentSetDefinition argSet,
            int? columns,
            string commandName,
            UsageInfoOptions options,
            object destination = null)
        {
            if (!columns.HasValue)
            {
                columns = GetCurrentConsoleWidth();
            }

            var maxUsageWidth = columns.Value;

            // Construct info for argument set.
            var info = new ArgumentSetUsageInfo
            {
                Name = commandName ?? AssemblyUtilities.GetAssemblyFileName(),
                Description = argSet.Attribute.AdditionalHelp,
                DefaultShortNamePrefix = argSet.Attribute.ShortNameArgumentPrefixes.FirstOrDefault()
            };

            // Add parameters and examples.
            info.AddParameters(GetArgumentUsageInfo(argSet, destination));
            if (argSet.Attribute.Examples != null)
            {
                info.AddExamples(argSet.Attribute.Examples);
            }

            // Update logo, if one was provided.
            if (argSet.Attribute.LogoString != null)
            {
                info.Logo = argSet.Attribute.LogoString;
            }

            // Compose remarks, if any.
            const string defaultHelpArgumentName = "?";
            var namedArgPrefix = argSet.Attribute.ShortNameArgumentPrefixes.FirstOrDefault();
            if (argSet.TryGetNamedArgument(defaultHelpArgumentName, out ArgumentDefinition ignored) && namedArgPrefix != null)
            {
                info.Remarks = string.Format(Strings.UsageInfoHelpAdvertisement, $"{info.Name} {namedArgPrefix}{defaultHelpArgumentName}");
            }

            // Construct formatter and use it.
            HelpFormatter formatter;
            if (options.HasFlag(UsageInfoOptions.VerticallyExpandedOutput))
            {
                formatter = new PowershellStyleHelpFormatter();
            }
            else
            {
                formatter = new CondensedHelpFormatter();
            }

            formatter.MaxWidth = maxUsageWidth;
            formatter.Options = options;

            return formatter.Format(info);
        }

        private static int GetCurrentConsoleWidth()
        {
            int columns;

            try
            {
                columns = GetConsoleWidth?.Invoke() ?? DefaultConsoleWidth;
            }
            catch (IOException)
            {
                // If can't determine the console's width, then default it.
                columns = DefaultConsoleWidth;
            }

            // N.B. Leave room so that we don't cycle over to next line.
            if (columns > 0)
            {
                --columns;
            }

            return columns;
        }

        private static IEnumerable<ArgumentUsageInfo> GetArgumentUsageInfo(ArgumentSetDefinition argSet, object destination)
        {
            // Enumerate positional arguments first, in position order.
            foreach (var arg in argSet.PositionalArguments.Where(a => !a.Hidden))
            {
                var currentValue = (destination != null) ? arg.GetValue(destination) : null;
                yield return new ArgumentUsageInfo(arg, currentValue);
            }

            var stringComparer = argSet.Attribute.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            // Enumerate named arguments next, in case-insensitive sort order.
            foreach (var arg in argSet.NamedArguments
                                    .Where(a => !a.Hidden)
                                    .OrderBy(a => a.LongName, stringComparer))
            {
                var currentValue = (destination != null) ? arg.GetValue(destination) : null;
                yield return new ArgumentUsageInfo(arg, currentValue);
            }

            // Add an extra item for answer files, if that is supported on this
            // argument set.
            if (argSet.Attribute.AnswerFileArgumentPrefix != null)
            {
                var pseudoArgLongName = Strings.AnswerFileArgumentName;

                if (argSet.Attribute.NameGenerationFlags.HasFlag(ArgumentNameGenerationFlags.GenerateHyphenatedLowerCaseLongNames))
                {
                    pseudoArgLongName = pseudoArgLongName.ToHyphenatedLowerCase();
                }

                yield return new ArgumentUsageInfo(
                    $"[{argSet.Attribute.AnswerFileArgumentPrefix}<{pseudoArgLongName}>]*",
                    Strings.AnswerFileArgumentDescription,
                    required: false);
            }
        }
    }
}
