using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using NClap.Metadata;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Parser
{
    /// <summary>
    /// <para>
    /// Parser for command line arguments. The parser specification is inferred
    /// from the instance fields and properties of the object specified as the
    /// destination of the parse.
    /// </para>
    /// <para>
    /// Valid argument types include: int, uint, float, double, string, bool,
    /// enum types.  Arrays of any of these types are also valid.
    /// </para>
    /// <para>
    /// Error checking options can be controlled by adding an ArgumentAttribute
    /// attribute to the instance fields/properties of the destination object.
    /// </para>
    /// <para>
    /// The default long name of the argument is the field or property name.
    /// The default short name is the first character of the long name. Long
    /// names and explicitly specified short names must be unique. Default
    /// short names will be used, provided that the default short name does
    /// not conflict with a long name or an explicitly specified short name.
    /// </para>
    /// <para>
    /// Arguments which are array types are collection arguments. Collection
    /// arguments can be specified multiple times.
    /// </para>
    /// </summary>
    internal sealed class CommandLineParserEngine
    {
        private enum NamedArgumentType
        {
            ShortName,
            LongName
        }

        private class ArgumentAndValue
        {
            public ArgumentDefinition Arg { get; set; }
            public string Value { get; set; }
        }

        private enum TokenParseResultState
        {
            Ready,

            UnknownNamedArgument,
            UnknownPositionalArgument,
            FailedParsing,
            FailedFinalizing,
            InvalidAnswerFile,
            RequiresOptionArgument
        }

        private class TokenParseResult
        {
            private TokenParseResult(TokenParseResultState state)
            {
                State = state;
            }

            public static TokenParseResult Ready { get; } =
                new TokenParseResult(TokenParseResultState.Ready);
            public static TokenParseResult UnknownNamedArgument(NamedArgumentType? namedArgType = null, string name = null) =>
                new TokenParseResult(TokenParseResultState.UnknownNamedArgument)
                {
                    NamedArgType = namedArgType,
                    NamedArg = name
                };
            public static TokenParseResult UnknownPositionalArgument { get; } =
                new TokenParseResult(TokenParseResultState.UnknownPositionalArgument);
            public static TokenParseResult FailedParsing { get; } =
                new TokenParseResult(TokenParseResultState.FailedParsing);
            public static TokenParseResult FailedFinalizing { get; } =
                new TokenParseResult(TokenParseResultState.FailedFinalizing);
            public static TokenParseResult InvalidAnswerFile { get; } =
                new TokenParseResult(TokenParseResultState.InvalidAnswerFile);
            public static TokenParseResult RequiresOptionArgument(ArgumentDefinition arg)
            {
                if (arg == null) throw new ArgumentNullException(nameof(arg));
                return new TokenParseResult(TokenParseResultState.RequiresOptionArgument)
                {
                    Argument = arg
                };
            }

            public TokenParseResultState State { get; }
            public ArgumentDefinition Argument { get; private set; }
            public NamedArgumentType? NamedArgType { get; private set; }
            public string NamedArg { get; private set; }
            public bool IsReady => State == TokenParseResultState.Ready;
            public bool IsUnknown =>
                State == TokenParseResultState.UnknownNamedArgument ||
                State == TokenParseResultState.UnknownPositionalArgument;
        }

        // Constants.
        private const string ArgumentAnswerFileCommentLinePrefix = "#";
        private readonly static ConsoleColor? ErrorForegroundColor = ConsoleColor.Yellow;

        // Options.
        private readonly CommandLineParserOptions _options;

        // Argument definitions.
        private readonly ArgumentSetDefinition _argumentSet;

        // Mutable parse state.
        private int _nextPositionalArgIndexToParse;

        /// <summary>
        /// Creates a new command-line argument parser.
        /// </summary>
        /// <param name="type">Destination object type.</param>
        /// <param name="defaultValues">Optionally provides an object with
        /// default values.</param>
        /// <param name="options">Optionally provides additional options
        /// controlling how parsing proceeds.</param>
        public CommandLineParserEngine(Type type, object defaultValues = null, CommandLineParserOptions options = null)
        {
            // Save off the options provided; if none were provided, construct
            // some defaults.
            _options = options?.Clone() ?? new CommandLineParserOptions();

            // If no reporter was provided, use a no-op one.
            if (_options.Reporter == null)
            {
                _options.Reporter = err => { };
            }

            // If no file-system reader was provided, use our default implementation.
            if (_options.FileSystemReader == null)
            {
                _options.FileSystemReader = FileSystemReader.Create();
            }

            // Define the argument set.
            _argumentSet = new ArgumentSetDefinition(type, defaultValues, _options);
        }

        /// <summary>
        /// Tokenizes the provided input text line, observing quotes.
        /// </summary>
        /// <param name="line">Input line to parse.</param>
        /// <param name="options">Options for tokenizing.</param>
        /// <returns>Enumeration of tokens.</returns>
        public static IEnumerable<Token> Tokenize(string line, CommandLineTokenizerOptions options) =>
            StringUtilities.Tokenize(
                line,
                options.HasFlag(CommandLineTokenizerOptions.AllowPartialInput));

        /// <summary>
        /// Parses an argument list.
        /// </summary>
        /// <param name="args">The arguments to parse.</param>
        /// <param name="destination">The destination of the parsed arguments.</param>
        /// <returns>True if no parse errors were encountered.</returns>
        public bool Parse(IEnumerable<string> args, object destination) =>
            ParseArgumentList(args, destination).IsReady;

        private TokenParseResult ParseArgumentList(IEnumerable<string> args, object destination)
        {
            Debug.Assert(args != null);
            Debug.Assert(args.All(x => x != null));

            // Try to parse the argument list.
            var result = ParseTokens(args, destination);
            if (!result.IsReady)
            {
                return result;
            }

            // Finalize all arguments: named args first, then positional default args.
            foreach (var arg in _argumentSet.NamedArguments.Concat(_argumentSet.PositionalArguments))
            {
                if (!arg.TryFinalize(destination, _options.FileSystemReader))
                {
                    result = TokenParseResult.FailedFinalizing;
                }
            }

            return result;
        }

        /// <summary>
        /// Constructs a user-friendly usage string describing the command-line
        /// argument syntax.
        /// </summary>
        /// <param name="maxUsageWidth">Maximum width in characters for the
        /// usage text; used to wrap it.</param>
        /// <param name="commandName">Command name to display in the usage
        /// information.</param>
        /// <param name="options">Options for generating info.</param>
        /// <param name="destination">Destination object, optionally.</param>
        /// <returns>The constructed usage information string.</returns>
        public ColoredMultistring GetUsageInfo(int maxUsageWidth, string commandName, UsageInfoOptions options, object destination = null)
        {
            // Construct info for argument set.
            var info = new ArgumentSetUsageInfo
            {
                Name = commandName ?? AssemblyUtilities.GetAssemblyFileName(),
                Description = _argumentSet.Attribute.AdditionalHelp,
                DefaultShortNamePrefix = _argumentSet.Attribute.ShortNameArgumentPrefixes.FirstOrDefault()
            };

            // Add parameters and examples.
            info.AddParameters(GetArgumentUsageInfo(destination));
            if (_argumentSet.Attribute.Examples != null)
            {
                info.AddExamples(_argumentSet.Attribute.Examples);
            }

            // Update logo, if one was provided.
            if (_argumentSet.Attribute.LogoString != null)
            {
                info.Logo = _argumentSet.Attribute.LogoString;
            }

            // Compose remarks, if any.
            const string defaultHelpArgumentName = "?";
            var namedArgPrefix = _argumentSet.Attribute.ShortNameArgumentPrefixes.FirstOrDefault();
            if (_argumentSet.TryGetNamedArgument(defaultHelpArgumentName, out ArgumentDefinition ignored) && namedArgPrefix != null)
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

        /// <summary>
        /// Formats a parsed set of arguments back into tokenized string form.
        /// </summary>
        /// <param name="value">The parsed argument set.</param>
        /// <returns>The tokenized string.</returns>
        public IEnumerable<string> Format(object value)
        {
            // First format named arguments, then positional default arguments.
            return _argumentSet.NamedArguments
                .Concat(_argumentSet.PositionalArguments)
                .Select(arg => new { Argument = arg, Value = arg.GetValue(value) })
                .Where(argAndValue => (argAndValue.Value != null) && !argAndValue.Value.Equals(argAndValue.Argument.DefaultValue))
                .SelectMany(argAndValue => argAndValue.Argument.Format(argAndValue.Value))
                .Where(formattedValue => !string.IsNullOrWhiteSpace(formattedValue));
        }

        /// <summary>
        /// Generate possible completions for the specified token in the
        /// provided set of input tokens.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="indexOfTokenToComplete">Index of the token to complete.
        /// </param>
        /// <param name="destObjectFactory">If non-null, provides a factory
        /// function that can be used to create an object suitable to being
        /// filled out by this parser instance.</param>
        /// <returns>The candidate completions for the specified token.
        /// </returns>
        public IEnumerable<string> GetCompletions(IEnumerable<string> tokens, int indexOfTokenToComplete, Func<object> destObjectFactory)
        {
            Func<IEnumerable<string>> emptyCompletions = Enumerable.Empty<string>;

            var tokenList = tokens.ToList();

            // Complain bitterly if we were asked to complete something far beyond
            // the token list we received.
            if (indexOfTokenToComplete > tokenList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(indexOfTokenToComplete));
            }

            // If we were asked to complete the token just after the list we received,
            // then this means we're generating completions to append to the full
            // token list.  Insert an empty token and use that as the basis for
            // completion.
            if (indexOfTokenToComplete == tokenList.Count)
            {
                tokenList = tokenList.Concat(new[] { string.Empty }).ToList();
            }

            // Figure out the token we're going to complete.
            var tokenToComplete = tokenList[indexOfTokenToComplete];

            // Create a destination object if provided with a factory.
            var inProgressParsedObject = destObjectFactory?.Invoke();

            // Parse what we've seen thus far (before the token /to complete).
            // Note that this parse attempt may fail; we ignore success/failure.
            var tokensToParse = tokenList.Take(indexOfTokenToComplete).ToList();
            var parseResult = ParseArgumentList(tokensToParse, inProgressParsedObject);

            // See if we're expecting this token to be an option argument; if
            // so, then we can generate completions based on that.
            if (parseResult.State == TokenParseResultState.RequiresOptionArgument &&
                _argumentSet.Attribute.AllowNamedArgumentValueAsSucceedingToken)
            {
                return parseResult.Argument.GetCompletions(
                    tokenList,
                    indexOfTokenToComplete,
                    tokenToComplete,
                    inProgressParsedObject);
            }

            // See if the token to complete appears to be a (long-)named argument.
            var longNameArgumentPrefix = TryGetLongNameArgumentPrefix(tokenToComplete);
            if (longNameArgumentPrefix != null)
            {
                var afterPrefix = tokenToComplete.Substring(longNameArgumentPrefix.Length);
                return GetNamedArgumentCompletions(tokenList, indexOfTokenToComplete, afterPrefix, inProgressParsedObject)
                           .Select(completion => longNameArgumentPrefix + completion);
            }

            // See if the token to complete appears to be a (short-)named argument.
            var shortNameArgumentPrefix = TryGetShortNameArgumentPrefix(tokenToComplete);
            if (shortNameArgumentPrefix != null)
            {
                var afterPrefix = tokenToComplete.Substring(shortNameArgumentPrefix.Length);
                return GetNamedArgumentCompletions(tokenList, indexOfTokenToComplete, afterPrefix, inProgressParsedObject)
                           .Select(completion => shortNameArgumentPrefix + completion);
            }

            // See if the token to complete appears to be the special answer-file argument.
            var answerFileArgumentPrefix = TryGetAnswerFilePrefix(tokenToComplete);
            if (answerFileArgumentPrefix != null)
            {
                var filePath = tokenToComplete.Substring(answerFileArgumentPrefix.Length);
                var parseContext = new ArgumentParseContext
                {
                    FileSystemReader = _options.FileSystemReader,
                    ParserContext = _options.Context,
                    CaseSensitive = _argumentSet.Attribute.CaseSensitive
                };

                var completionContext = new ArgumentCompletionContext
                {
                    ParseContext = parseContext,
                    TokenIndex = indexOfTokenToComplete,
                    Tokens = tokenList,
                    InProgressParsedObject = inProgressParsedObject,
                    CaseSensitive = _argumentSet.Attribute.CaseSensitive
                };

                return ArgumentType.FileSystemPath.GetCompletions(completionContext, filePath)
                           .Select(completion => answerFileArgumentPrefix + completion);
            }

            // At this point, assume it must be a positional argument. If it's not, then there's not much
            // that we can do.
            if (!_argumentSet.TryGetPositionalArgument(_nextPositionalArgIndexToParse, out ArgumentDefinition positionalArg))
            {
                return emptyCompletions();
            }

            return positionalArg.GetCompletions(
                tokenList,
                indexOfTokenToComplete,
                tokenToComplete,
                inProgressParsedObject);
        }

        private IEnumerable<char> ArgumentTerminatorsAndSeparators =>
            ArgumentNameTerminators.Concat(_argumentSet.Attribute.ArgumentValueSeparators);

        private static IEnumerable<char> ArgumentNameTerminators => new[] { '+', '-' };

        private void ReportUnreadableFile(string filePath) =>
            ReportLine(Strings.UnreadableFile, filePath);

        private void ReportUnrecognizedArgument(TokenParseResult result, string argument)
        {
            switch (result.State)
            {
                case TokenParseResultState.UnknownNamedArgument:
                    ReportLine(Strings.UnrecognizedArgument, argument);

                    if (!string.IsNullOrEmpty(result.NamedArg))
                    {
                        var possibleArgs = GetSimilarNamedArguments(result.NamedArgType, result.NamedArg).ToList();
                        if (possibleArgs.Count > 0)
                        {
                            ReportLine(
                                "  " + Strings.PossibleIntendedNamedArguments,
                                string.Join(", ", possibleArgs.Select(a => "'" + a + "'")));
                        }
                    }

                    break;

                case TokenParseResultState.UnknownPositionalArgument:
                    ReportLine(Strings.UnrecognizedArgument, argument);
                    break;
                case TokenParseResultState.RequiresOptionArgument:
                    ReportLine(Strings.MissingRequiredOptionArgument, argument);
                    break;
            }
        }

        /// <summary>
        /// Parses an argument list into an object.
        /// </summary>
        /// <param name="args">String arguments to parse.</param>
        /// <param name="destination">Output arguments object.</param>
        /// <returns>Parse result.</returns>
        private TokenParseResult ParseTokens(IEnumerable<string> args, object destination)
        {
            var result = TokenParseResult.Ready;
            IReadOnlyList<string> argsList = args.ToList();

            for (var index = 0; index < argsList.Count; )
            {
                var currentResult = TryParseNextToken(argsList, index, destination, out int argsConsumed);
                if (currentResult != TokenParseResult.Ready)
                {
                    result = currentResult;
                }

                index += argsConsumed;
            }

            return result;
        }

        private TokenParseResult TryParseNextToken(IReadOnlyList<string> args, int index, object destination, out int argsConsumed)
        {
            // Default to assuming we consume 1 arg.  Will override below as needed.
            argsConsumed = 1;

            // Note that we do *not* remove leading or trailing whitespace from the argument value; it might be meaningful.
            var argument = args[index];

            // See if a named arg prefix exists.
            var longNameArgumentPrefix = TryGetLongNameArgumentPrefix(argument);
            var shortNameArgumentPrefix = TryGetShortNameArgumentPrefix(argument);
            if (longNameArgumentPrefix != null || shortNameArgumentPrefix != null)
            {
                return TryParseNextNamedArgument(args, index, longNameArgumentPrefix, shortNameArgumentPrefix, destination, out argsConsumed);
            }

            // See if an answer file prefix exists.
            var answerFilePrefix = TryGetAnswerFilePrefix(argument);
            if (answerFilePrefix != null)
            {
                return TryParseNextAnswerFileArgument(argument, answerFilePrefix, destination);
            }

            // Otherwise, we assume it's a positional argument.
            return TryParseNextPositionalArgument(args, index, destination, out argsConsumed);
        }

        private TokenParseResult TryParseNextNamedArgument(IReadOnlyList<string> args, int index, string longNameArgumentPrefix, string shortNameArgumentPrefix, object destination, out int argsConsumed)
        {
            argsConsumed = 1;

            var argument = args[index];
            var result = TokenParseResult.UnknownNamedArgument();

            IReadOnlyList<ArgumentAndValue> parsedArgs = null;
            if (result.IsUnknown && longNameArgumentPrefix != null)
            {
                result = TryParseNamedArgument(argument, longNameArgumentPrefix, NamedArgumentType.LongName, out parsedArgs);
            }
            if (result.IsUnknown && shortNameArgumentPrefix != null)
            {
                result = TryParseNamedArgument(argument, shortNameArgumentPrefix, NamedArgumentType.ShortName, out parsedArgs);
            }

            // If our policy allows a named argument's value to be placed
            // in the following token, and if we're missing a required
            // value, and if there's at least one more token, then try
            // to parse the next token as the current argument's value.
            if (result.State == TokenParseResultState.RequiresOptionArgument &&
                _argumentSet.Attribute.AllowNamedArgumentValueAsSucceedingToken &&
                index + 1 < args.Count)
            {
                var lastParsedArg = parsedArgs.GetLast();

                Debug.Assert(lastParsedArg.Arg.RequiresOptionArgument);
                Debug.Assert(!lastParsedArg.Arg.TakesRestOfLine);
                Debug.Assert(string.IsNullOrEmpty(parsedArgs.GetLast().Value));

                ++index;
                ++argsConsumed;

                lastParsedArg.Value = args[index];
                result = TokenParseResult.Ready;
            }

            if (!result.IsReady)
            {
                ReportUnrecognizedArgument(result, argument);
                return result;
            }

            foreach (var parsedArg in parsedArgs)
            {
                // TODO: Obviate the need to use string.Empty here.
                var argValue = parsedArg.Value ?? string.Empty;

                if (parsedArg.Arg.TakesRestOfLine)
                {
                    if (!parsedArg.Arg.TrySetRestOfLine(argValue, args.Skip(index + 1), destination))
                    {
                        result = TokenParseResult.FailedParsing;
                        continue;
                    }

                    argsConsumed = args.Count - index; // skip the rest of the line
                }
                else
                {
                    if (!TryParseAndStore(parsedArg.Arg, argValue, destination))
                    {
                        result = TokenParseResult.FailedParsing;
                        continue;
                    }
                }
            }

            return result;
        }

        private TokenParseResult TryParseNextAnswerFileArgument(string argument, string answerFilePrefix, object destination)
        {
            var filePath = argument.Substring(answerFilePrefix.Length);
            if (!TryLexArgumentAnswerFile(filePath, out IEnumerable<string> nestedArgs))
            {
                ReportUnreadableFile(filePath);
                return TokenParseResult.InvalidAnswerFile;
            }

            var nestedArgsArray = nestedArgs.ToArray();
            return ParseTokens(nestedArgsArray, destination);
        }

        private TokenParseResult TryParseNextPositionalArgument(IReadOnlyList<string> args, int index, object destination, out int argsConsumed)
        {
            var argument = args[index];

            argsConsumed = 1;

            if (!_argumentSet.TryGetPositionalArgument(_nextPositionalArgIndexToParse, out ArgumentDefinition positionalArg))
            {
                var result = TokenParseResult.UnknownPositionalArgument;
                ReportUnrecognizedArgument(result, argument);

                return result;
            }

            if (!positionalArg.AllowMultiple)
            {
                ++_nextPositionalArgIndexToParse;
            }

            if (positionalArg.TakesRestOfLine)
            {
                if (!positionalArg.TrySetRestOfLine(args.Skip(index), destination))
                {
                    return TokenParseResult.FailedParsing;
                }

                argsConsumed = args.Count - index; // skip the rest of the line
                return TokenParseResult.Ready;
            }
            else
            {
                Debug.Assert(argument != null);
                return TryParseAndStore(positionalArg, argument, destination)
                    ? TokenParseResult.Ready : TokenParseResult.FailedParsing;
            }
        }

        private TokenParseResult TryParseNamedArgument(string argument, string argumentPrefix, NamedArgumentType namedArgType, out IReadOnlyList<ArgumentAndValue> parsedArgs)
        {
            var prefixLength = argumentPrefix.Length;
            Debug.Assert(argument.Length >= prefixLength);

            // Figure out where the argument name ends.
            var endIndex = argument.IndexOfAny(_argumentSet.Attribute.ArgumentValueSeparators, prefixLength);

            // Special case: check for '+' and '-' for booleans.
            if (endIndex < 0 && argument.Length >= 2)
            {
                var lastArgumentChar = argument[argument.Length - 1];
                if (ArgumentNameTerminators.Any(t => lastArgumentChar.Equals(t)))
                {
                    endIndex = argument.Length - 1;
                }
            }

            // If we don't have a separator or terminator, then consume the full string.
            if (endIndex < 0)
            {
                endIndex = argument.Length;
            }

            // Extract the argument name(s), separate from the prefix
            // or optional argument value.
            var options = argument.Substring(prefixLength, endIndex - prefixLength);

            // Extract the option argument (a.k.a. value), if there is one.
            string optionArgument = null;
            if (argument.Length > prefixLength + options.Length)
            {
                // If there's an argument value separator, then extract the value after the separator.
                if (_argumentSet.Attribute.ArgumentValueSeparators.Any(sep => argument[prefixLength + options.Length] == sep))
                {
                    optionArgument = argument.Substring(prefixLength + options.Length + 1);
                }

                // Otherwise, it might be a terminator; extract the rest of the string.
                else
                {
                    optionArgument = argument.Substring(prefixLength + options.Length);
                }
            }

            // Now try to figure out how many names are present.
            if (namedArgType == NamedArgumentType.ShortName &&
                (_argumentSet.Attribute.AllowMultipleShortNamesInOneToken || _argumentSet.Attribute.AllowElidingSeparatorAfterShortName))
            {
                Debug.Assert(_argumentSet.Attribute.ShortNamesAreOneCharacterLong);

                // Since short names are one character long, we parse them one at a
                // time, preparing for multiple arguments in this one token.
                var args = new List<ArgumentAndValue>();
                for (var index = 0; index < options.Length; ++index)
                {
                    // Try parsing it as a short name; bail immediately if we find an invalid
                    // one.
                    var possibleShortName = new string(options[index], 1);
                    if (!_argumentSet.TryGetNamedArgument(possibleShortName, out ArgumentDefinition arg))
                    {
                        parsedArgs = null;
                        return TokenParseResult.UnknownNamedArgument(namedArgType, possibleShortName);
                    }

                    // If this parsed as a short name that takes a required option argument,
                    // and we didn't see an option argument, and we allow mushing together
                    // short names and their option arguments, then try parsing the rest of
                    // this token as an option argument.
                    var lastChar = index == options.Length - 1;
                    if (arg.RequiresOptionArgument &&
                        _argumentSet.Attribute.AllowElidingSeparatorAfterShortName &&
                        optionArgument == null &&
                        !lastChar)
                    {
                        optionArgument = options.Substring(index + 1);
                        index = options.Length - 1;
                        lastChar = true;
                    }

                    if (!_argumentSet.Attribute.AllowMultipleShortNamesInOneToken &&
                        args.Count > 0)
                    {
                        parsedArgs = null;
                        return TokenParseResult.UnknownNamedArgument();
                    }

                    args.Add(new ArgumentAndValue
                    {
                        Arg = arg,
                        Value = lastChar ? optionArgument : null
                    } );
                }

                parsedArgs = args;
            }
            else
            {
                // Try to look up the argument by name.
                if (!_argumentSet.TryGetNamedArgument(options, out ArgumentDefinition arg))
                {
                    parsedArgs = null;
                    return TokenParseResult.UnknownNamedArgument(namedArgType, options);
                }

                parsedArgs = new[] { new ArgumentAndValue
                {
                    Arg = arg,
                    Value = optionArgument
                } };
            }

            // If the last named argument we saw in this token required an
            // option argument to go with it, then yield that information
            // so it can be used by the caller (e.g. in completion generation).
            var lastArg = parsedArgs.GetLastOrDefault();
            if (lastArg != null &&
                lastArg.Arg.RequiresOptionArgument &&
                string.IsNullOrEmpty(lastArg.Value))
            {
                return TokenParseResult.RequiresOptionArgument(lastArg.Arg);
            }

            return TokenParseResult.Ready;
        }

        private IEnumerable<ArgumentUsageInfo> GetArgumentUsageInfo(object destination)
        {
            // Enumerate positional arguments first, in position order.
            foreach (var arg in _argumentSet.PositionalArguments.Where(a => !a.Hidden))
            {
                var currentValue = (destination != null) ? arg.GetValue(destination) : null;
                yield return new ArgumentUsageInfo(arg, currentValue);
            }

            // Enumerate named arguments next, in case-insensitive sort order.
            foreach (var arg in _argumentSet.NamedArguments
                                    .Where(a => !a.Hidden)
                                    .OrderBy(a => a.LongName, StringComparerToUse))
            {
                var currentValue = (destination != null) ? arg.GetValue(destination) : null;
                yield return new ArgumentUsageInfo(arg, currentValue);
            }

            // Add an extra item for answer files, if that is supported on this
            // argument set.
            if (_argumentSet.Attribute.AnswerFileArgumentPrefix != null)
            {
                var pseudoArgLongName = Strings.AnswerFileArgumentName;

                if (_argumentSet.Attribute.NameGenerationFlags.HasFlag(ArgumentNameGenerationFlags.GenerateHyphenatedLowerCaseLongNames))
                {
                    pseudoArgLongName = pseudoArgLongName.ToHyphenatedLowerCase();
                }

                yield return new ArgumentUsageInfo(
                    $"[{_argumentSet.Attribute.AnswerFileArgumentPrefix}<{pseudoArgLongName}>]*",
                    Strings.AnswerFileArgumentDescription,
                    required: false);
            }
        }

        private IEnumerable<string> GetNamedArgumentCompletions(IReadOnlyList<string> tokens, int indexOfTokenToComplete, string namedArgumentAfterPrefix, object inProgressParsedObject)
        {
            Func<IEnumerable<string>> emptyCompletions = Enumerable.Empty<string>;

            var separatorIndex = namedArgumentAfterPrefix.IndexOfAny(ArgumentTerminatorsAndSeparators.ToArray());
            if (separatorIndex < 0)
            {
                return _argumentSet.NamedArguments
                           .Select(namedArg => namedArg.LongName)
                           .OrderBy(longName => longName, StringComparerToUse)
                           .Where(candidateName => candidateName.StartsWith(namedArgumentAfterPrefix, StringComparisonToUse));
            }

            var separator = namedArgumentAfterPrefix[separatorIndex];
            if (!_argumentSet.Attribute.ArgumentValueSeparators.Contains(separator))
            {
                return emptyCompletions();
            }

            var name = namedArgumentAfterPrefix.Substring(0, separatorIndex);
            var value = namedArgumentAfterPrefix.Substring(separatorIndex + 1);

            if (!_argumentSet.TryGetNamedArgument(name, out ArgumentDefinition arg))
            {
                return emptyCompletions();
            }

            return arg.GetCompletions(tokens, indexOfTokenToComplete, value, inProgressParsedObject)
                      .Select(completion => string.Concat(name, separator.ToString(), completion));
        }

        private bool TryParseAndStore(ArgumentDefinition arg, string value, object dest)
        {
            if (!arg.TryParseAndStore(value, dest, out object parsedValue))
            {
                return false;
            }

            // Inspect the parsed value.
            var argProvider = parsedValue as IArgumentProvider;
            if (argProvider == null)
            {
                argProvider = (arg.FixedDestination ?? dest) as IArgumentProvider;
            }

            if (argProvider != null)
            {
                var definingType = argProvider.GetTypeDefiningArguments();
                if (definingType != null)
                {
                    _argumentSet.AddArgumentsFromTypeWithAttributes(definingType,
                        fixedDestination: argProvider.GetDestinationObject());
                }
            }

            return true;
        }

        private string TryGetLongNameArgumentPrefix(string arg) =>
            _argumentSet.Attribute.NamedArgumentPrefixes.FirstOrDefault(
                prefix => arg.StartsWith(prefix, StringComparisonToUse));

        private string TryGetShortNameArgumentPrefix(string arg) =>
            _argumentSet.Attribute.ShortNameArgumentPrefixes.FirstOrDefault(
                prefix => arg.StartsWith(prefix, StringComparisonToUse));

        private string TryGetAnswerFilePrefix(string arg)
        {
            if (_argumentSet.Attribute.AnswerFileArgumentPrefix == null)
            {
                return null;
            }

            if (!arg.StartsWith(_argumentSet.Attribute.AnswerFileArgumentPrefix, StringComparisonToUse))
            {
                return null;
            }

            return _argumentSet.Attribute.AnswerFileArgumentPrefix;
        }

        private bool TryLexArgumentAnswerFile(string filePath, out IEnumerable<string> arguments)
        {
            try
            {
                //
                // Read through all non-empty lines in the file.
                //
                // NOTE: We are trimming the lines here; that means it's not
                // possible for an answer file to meaningfully use leading or
                // trailing whitespace.
                //

                arguments = _options.FileSystemReader
                    .GetLines(filePath)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrEmpty(line))
                    .Where(line => !line.StartsWith(ArgumentAnswerFileCommentLinePrefix, StringComparison.Ordinal));

                return true;
            }
            catch (IOException e)
            {
                ReportLine(Strings.CannotReadArgumentAnswerFile, filePath, e.Message);

                arguments = null;
                return false;
            }
        }

        private void ReportLine(string message, params object[] args)
        {
            Debug.Assert(_options != null);
            Debug.Assert(_options.Reporter != null);
            _options.Reporter(new ColoredMultistring(
                new[]
                {
                    new ColoredString(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            message + Environment.NewLine,
                            args),
                        ErrorForegroundColor)
                }));
        }

        private IEnumerable<string> GetSimilarNamedArguments(NamedArgumentType? type, string name)
        {
            var candidates = _argumentSet.ArgumentNames;
            var prefix = string.Empty;

            if (type.HasValue)
            {
                switch (type.Value)
                {
                    case NamedArgumentType.LongName:
                        prefix = _argumentSet.Attribute.NamedArgumentPrefixes.FirstOrDefault();
                        break;
                    case NamedArgumentType.ShortName:
                        prefix = _argumentSet.Attribute.ShortNameArgumentPrefixes.FirstOrDefault();
                        break;
                }
            }

            return candidates.Where(c => IsUserProvidedStringSimilarTo(name, c))
                .Select(c => prefix + c);
        }

        private static bool IsUserProvidedStringSimilarTo(string userValue, string candidate)
        {
            if (userValue.StartsWith(candidate, StringComparison.OrdinalIgnoreCase) ||
                candidate.StartsWith(userValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var editDistance = StringUtilities.GetDamerauLevenshteinEditDistance(
                candidate.ToUpperInvariant(),
                userValue.ToUpperInvariant());

            return editDistance <= userValue.Length / 2;
        }

        private StringComparer StringComparerToUse =>
            _argumentSet.Attribute.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

        private StringComparison StringComparisonToUse =>
            _argumentSet.Attribute.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    }
}
