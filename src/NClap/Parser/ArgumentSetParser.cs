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
    /// Encapsulates the state of an in-progress parse of an argument set.
    /// </summary>
    internal partial class ArgumentSetParser
    {
        // Constants.
        private const string ArgumentAnswerFileCommentLinePrefix = "#";
        private static readonly ConsoleColor? ErrorForegroundColor = ConsoleColor.Yellow;

        private readonly Dictionary<ArgumentDefinition, ArgumentParser> _stateByArg =
            new Dictionary<ArgumentDefinition, ArgumentParser>();

        private readonly CommandLineParserOptions _options;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="argSet">Argument set to create parser for.</param>
        /// <param name="options">Parser options.</param>
        public ArgumentSetParser(ArgumentSetDefinition argSet, CommandLineParserOptions options)
        {
            if (argSet == null)
            {
                throw new ArgumentNullException(nameof(argSet));
            }

            // Clone the argument set definition, as parsing may mutate it.
            ArgumentSet = argSet.DeepClone();

            // Save off the options provided; if none were provided, construct some quiet defaults.
            _options = options?.DeepClone() ?? CommandLineParserOptions.Quiet();

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
        }

        /// <summary>
        /// Keeps track of the index of the next positional argument to parse.
        /// </summary>
        public int NextPositionalArgIndexToParse { get; set; }

        /// <summary>
        /// The argument set used by the parser.
        /// </summary>
        public ArgumentSetDefinition ArgumentSet { get; }

        /// <summary>
        /// Checks if this parser has seen a value provided for the given argument.
        /// </summary>
        /// <param name="arg">The argument to look for.</param>
        /// <returns>True if a value has been seen; false otherwise.</returns>
        public bool HasSeenValueFor(ArgumentDefinition arg)
        {
            if (!_stateByArg.TryGetValue(arg, out ArgumentParser parser))
            {
                return false;
            }

            return parser.SeenValue;
        }

        /// <summary>
        /// Tries to parse the given list of tokens.
        /// </summary>
        /// <param name="args">Argument tokens to parse.</param>
        /// <param name="destination">Destination object to store the parsed values.
        /// May be null.</param>
        /// <returns>Result of the parsing.</returns>
        public ArgumentSetParseResult ParseArgumentList(IEnumerable<string> args, object destination)
        {
            Debug.Assert(args != null);
            Debug.Assert(args.All(x => x != null));

            // Try to parse the argument list.
            var result = ParseTokens(args, destination);
            if (!result.IsReady)
            {
                return result;
            }

            // Finalize.
            return Finalize(destination);
        }

        /// <summary>
        /// Generates possible completions for the indicated argument token.
        /// </summary>
        /// <param name="tokens">Full list of tokens in context.</param>
        /// <param name="indexOfTokenToComplete">0-based index of token
        /// to complete; must either reference valid token in <paramref name="tokens"/>
        /// or the index of the next token that would follow the provided
        /// tokens.</param>
        /// <param name="destObjectFactory">Optionally provides a function
        /// that may be used to instantiate an object of the destination
        /// parse output type.</param>
        /// <returns>Possible completions for the token.</returns>
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
            if (parseResult.State == ArgumentSetParseResultType.RequiresOptionArgument &&
                ArgumentSet.Attribute.AllowNamedArgumentValueAsSucceedingToken)
            {
                var argParseState = new ArgumentParser(ArgumentSet, parseResult.Argument, _options, /*destination=*/null);
                return argParseState.GetCompletions(
                    tokenList,
                    indexOfTokenToComplete,
                    tokenToComplete,
                    inProgressParsedObject);
            }

            // See if the token to complete appears to be a named argument.
            var longNameArgumentPrefix = TryGetLongNameArgumentPrefix(tokenToComplete);
            var shortNameArgumentPrefix = TryGetShortNameArgumentPrefix(tokenToComplete);

            if (longNameArgumentPrefix != null || shortNameArgumentPrefix != null)
            {
                var completions = Enumerable.Empty<string>();

                if (longNameArgumentPrefix != null)
                {
                    var afterLongPrefix = tokenToComplete.Substring(longNameArgumentPrefix.Length);
                    completions = completions.Concat(
                        GetNamedArgumentCompletions(ArgumentNameType.LongName, tokenList, indexOfTokenToComplete, afterLongPrefix, inProgressParsedObject)
                               .Select(completion => longNameArgumentPrefix + completion));
                }

                if (shortNameArgumentPrefix != null)
                {
                    var afterShortPrefix = tokenToComplete.Substring(shortNameArgumentPrefix.Length);
                    completions = completions.Concat(
                        GetNamedArgumentCompletions(ArgumentNameType.ShortName, tokenList, indexOfTokenToComplete, afterShortPrefix, inProgressParsedObject)
                               .Select(completion => shortNameArgumentPrefix + completion));
                }

                return completions;
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
                    CaseSensitive = ArgumentSet.Attribute.CaseSensitive
                };

                var completionContext = new ArgumentCompletionContext
                {
                    ParseContext = parseContext,
                    TokenIndex = indexOfTokenToComplete,
                    Tokens = tokenList,
                    InProgressParsedObject = inProgressParsedObject,
                    CaseSensitive = ArgumentSet.Attribute.CaseSensitive
                };

                return ArgumentType.FileSystemPath.GetCompletions(completionContext, filePath)
                           .Select(completion => answerFileArgumentPrefix + completion);
            }

            // At this point, assume it must be a positional argument. If it's not, then there's not much
            // that we can do.
            if (!ArgumentSet.TryGetPositionalArgument(NextPositionalArgIndexToParse, out ArgumentDefinition positionalArg))
            {
                return emptyCompletions();
            }

            var parseState = new ArgumentParser(ArgumentSet, positionalArg, _options, /*destination=*/null);
            return parseState.GetCompletions(
                tokenList,
                indexOfTokenToComplete,
                tokenToComplete,
                inProgressParsedObject);
        }

        /// <summary>
        /// Retrieves the parse state for the given argument.  If no such state exists,
        /// then a state object is constructed and persisted.
        /// </summary>
        /// <param name="arg">The argument to look up.</param>
        /// <param name="destination">Optionally provides the destination object
        /// being parsed into.</param>
        /// <returns>The parse state for the given argument.</returns>
        private ArgumentParser GetStateForArgument(ArgumentDefinition arg, object destination)
        {
            ArgumentParser parser;
            if (_stateByArg.ContainsKey(arg))
            {
                parser = _stateByArg[arg];

                if (destination != null &&
                    parser.Argument.FixedDestination == null &&
                    parser.DestinationObject != destination)
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                ArgumentParser parent = null;
                if (arg.ContainingArgument != null)
                {
                    _stateByArg.TryGetValue(arg.ContainingArgument, out parent);
                }

                parser = _stateByArg[arg] = new ArgumentParser(ArgumentSet, arg, _options, destination, parent);
            }

            return parser;
        }

        private IEnumerable<string> GetNamedArgumentCompletions(ArgumentNameType nameType, IReadOnlyList<string> tokens, int indexOfTokenToComplete, string namedArgumentAfterPrefix, object inProgressParsedObject)
        {
            Func<IEnumerable<string>> emptyCompletions = Enumerable.Empty<string>;

            var separatorIndex = namedArgumentAfterPrefix.IndexOfAny(ArgumentTerminatorsAndSeparators.ToArray());
            if (separatorIndex < 0)
            {
                return ArgumentSet.NamedArguments
                           .Select(namedArg => namedArg.GetName(nameType))
                           .Where(argName => argName != null)
                           .OrderBy(argName => argName, StringComparerToUse)
                           .Where(candidateName => candidateName.StartsWith(namedArgumentAfterPrefix, StringComparisonToUse));
            }

            var separator = namedArgumentAfterPrefix[separatorIndex];
            if (!ArgumentSet.Attribute.ArgumentValueSeparators.Contains(separator))
            {
                return emptyCompletions();
            }

            var name = namedArgumentAfterPrefix.Substring(0, separatorIndex);
            var value = namedArgumentAfterPrefix.Substring(separatorIndex + 1);

            if (!ArgumentSet.TryGetNamedArgument(nameType, name, out ArgumentDefinition arg))
            {
                return emptyCompletions();
            }

            var parseState = new ArgumentParser(ArgumentSet, arg, _options, /*destination=*/null);
            return parseState
                    .GetCompletions(tokens, indexOfTokenToComplete, value, inProgressParsedObject)
                    .Select(completion => string.Concat(name, separator.ToString(), completion));
        }

        /// <summary>
        /// Parses an argument list into an object.
        /// </summary>
        /// <param name="args">String arguments to parse.</param>
        /// <param name="destination">Output arguments object.</param>
        /// <returns>Parse result.</returns>
        public ArgumentSetParseResult ParseTokens(IEnumerable<string> args, object destination)
        {
            var result = ArgumentSetParseResult.Ready;
            IReadOnlyList<string> argsList = args.ToList();

            for (var index = 0; index < argsList.Count;)
            {
                var currentResult = TryParseNextToken(argsList, index, destination, out int argsConsumed);
                if (currentResult != ArgumentSetParseResult.Ready)
                {
                    result = currentResult;
                }

                index += argsConsumed;
            }

            return result;
        }

        /// <summary>
        /// Tries to finalize parsing to the given output object.
        /// </summary>
        /// <param name="destination">Output object.</param>
        /// <returns>Parse result.</returns>
        public ArgumentSetParseResult Finalize(object destination)
        {
            var result = ArgumentSetParseResult.Ready;

            // Finalize all arguments: named args first, then positional default args.
            foreach (var arg in ArgumentSet.NamedArguments.Concat(ArgumentSet.PositionalArguments))
            {
                var argState = GetStateForArgument(arg, destination);
                if (!argState.TryFinalize(_options.FileSystemReader))
                {
                    result = ArgumentSetParseResult.FailedFinalizing;
                }
            }

            return result;
        }

        private ArgumentSetParseResult TryParseNextToken(IReadOnlyList<string> args, int index, object destination, out int argsConsumed)
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

        private ArgumentSetParseResult TryParseNextNamedArgument(IReadOnlyList<string> args, int index, string longNameArgumentPrefix, string shortNameArgumentPrefix, object destination, out int argsConsumed)
        {
            argsConsumed = 1;

            var argument = args[index];
            var result = ArgumentSetParseResult.UnknownNamedArgument();

            IReadOnlyList<ArgumentAndValue> parsedArgs = null;
            if (result.IsUnknown && longNameArgumentPrefix != null)
            {
                result = TryParseNamedArgument(argument, longNameArgumentPrefix, ArgumentNameType.LongName, out parsedArgs);
            }

            if (result.IsUnknown && shortNameArgumentPrefix != null)
            {
                result = TryParseNamedArgument(argument, shortNameArgumentPrefix, ArgumentNameType.ShortName, out parsedArgs);
            }

            // If our policy allows a named argument's value to be placed
            // in the following token, and if we're missing a required
            // value, and if there's at least one more token, then try
            // to parse the next token as the current argument's value.
            if (result.State == ArgumentSetParseResultType.RequiresOptionArgument &&
                ArgumentSet.Attribute.AllowNamedArgumentValueAsSucceedingToken &&
                index + 1 < args.Count)
            {
                var lastParsedArg = parsedArgs.GetLast();

                Debug.Assert(lastParsedArg.Arg.RequiresOptionArgument);
                Debug.Assert(!lastParsedArg.Arg.TakesRestOfLine);
                Debug.Assert(string.IsNullOrEmpty(parsedArgs.GetLast().Value));

                ++index;
                ++argsConsumed;

                lastParsedArg.Value = args[index];
                result = ArgumentSetParseResult.Ready;
            }

            if (!result.IsReady)
            {
                ReportUnrecognizedArgument(result, argument);
                return result;
            }

            Debug.Assert(parsedArgs != null);

            foreach (var parsedArg in parsedArgs)
            {
                // TODO: Obviate the need to use string.Empty here.
                var argValue = parsedArg.Value ?? string.Empty;
                var argState = GetStateForArgument(parsedArg.Arg, destination);
                if (parsedArg.Arg.TakesRestOfLine)
                {
                    if (!argState.TrySetRestOfLine(new[] { argValue }.Concat(args.Skip(index + 1))))
                    {
                        result = ArgumentSetParseResult.FailedParsing;
                        continue;
                    }

                    argsConsumed = args.Count - index; // skip the rest of the line
                }
                else
                {
                    if (!TryParseAndStore(argState, argValue))
                    {
                        result = ArgumentSetParseResult.FailedParsing;
                        continue;
                    }
                }
            }

            return result;
        }

        private ArgumentSetParseResult TryParseNextAnswerFileArgument(string argument, string answerFilePrefix, object destination)
        {
            var filePath = argument.Substring(answerFilePrefix.Length);
            if (!TryLexArgumentAnswerFile(filePath, out IEnumerable<string> nestedArgs))
            {
                ReportUnreadableFile(filePath);
                return ArgumentSetParseResult.InvalidAnswerFile;
            }

            var nestedArgsArray = nestedArgs.ToArray();
            return ParseTokens(nestedArgsArray, destination);
        }

        private ArgumentSetParseResult TryParseNextPositionalArgument(IReadOnlyList<string> args, int index, object destination, out int argsConsumed)
        {
            var argument = args[index];

            argsConsumed = 1;

            if (!ArgumentSet.TryGetPositionalArgument(NextPositionalArgIndexToParse, out ArgumentDefinition positionalArg))
            {
                var result = ArgumentSetParseResult.UnknownPositionalArgument;
                ReportUnrecognizedArgument(result, argument);

                return result;
            }

            if (!positionalArg.AllowMultiple)
            {
                NextPositionalArgIndexToParse += 1;
            }

            var argState = GetStateForArgument(positionalArg, destination);
            if (positionalArg.TakesRestOfLine)
            {
                if (!argState.TrySetRestOfLine(args.Skip(index)))
                {
                    return ArgumentSetParseResult.FailedParsing;
                }

                argsConsumed = args.Count - index; // skip the rest of the line
                return ArgumentSetParseResult.Ready;
            }
            else
            {
                Debug.Assert(argument != null);
                return TryParseAndStore(argState, argument)
                    ? ArgumentSetParseResult.Ready : ArgumentSetParseResult.FailedParsing;
            }
        }

        private ArgumentSetParseResult TryParseNamedArgument(
            string argument,
            string argumentPrefix,
            ArgumentNameType namedArgType,
            out IReadOnlyList<ArgumentAndValue> parsedArgs)
        {
            var prefixLength = argumentPrefix.Length;
            Debug.Assert(argument.Length >= prefixLength);

            // Figure out where the argument name ends.
            var endIndex = argument.IndexOfAny(ArgumentSet.Attribute.ArgumentValueSeparators, prefixLength);

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
                if (ArgumentSet.Attribute.ArgumentValueSeparators.Any(sep => argument[prefixLength + options.Length] == sep))
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
            if (namedArgType == ArgumentNameType.ShortName &&
                (ArgumentSet.Attribute.AllowMultipleShortNamesInOneToken || ArgumentSet.Attribute.AllowElidingSeparatorAfterShortName))
            {
                Debug.Assert(ArgumentSet.Attribute.ShortNamesAreOneCharacterLong);

                // Since short names are one character long, we parse them one at a
                // time, preparing for multiple arguments in this one token.
                var args = new List<ArgumentAndValue>();
                for (var index = 0; index < options.Length; ++index)
                {
                    // Try parsing it as a short name; bail immediately if we find an invalid
                    // one.
                    var possibleShortName = new string(options[index], 1);
                    if (!ArgumentSet.TryGetNamedArgument(ArgumentNameType.ShortName, possibleShortName, out ArgumentDefinition arg))
                    {
                        parsedArgs = null;
                        return ArgumentSetParseResult.UnknownNamedArgument(namedArgType, possibleShortName);
                    }

                    // If this parsed as a short name that takes a required option argument,
                    // and we didn't see an option argument, and we allow mushing together
                    // short names and their option arguments, then try parsing the rest of
                    // this token as an option argument.
                    var lastChar = index == options.Length - 1;
                    if (arg.RequiresOptionArgument &&
                        ArgumentSet.Attribute.AllowElidingSeparatorAfterShortName &&
                        optionArgument == null &&
                        !lastChar)
                    {
                        optionArgument = options.Substring(index + 1);
                        index = options.Length - 1;
                        lastChar = true;
                    }

                    if (!ArgumentSet.Attribute.AllowMultipleShortNamesInOneToken &&
                        args.Count > 0)
                    {
                        parsedArgs = null;
                        return ArgumentSetParseResult.UnknownNamedArgument();
                    }

                    args.Add(new ArgumentAndValue
                    {
                        Arg = arg,
                        Value = lastChar ? optionArgument : null
                    });
                }

                parsedArgs = args;
            }
            else
            {
                // Try to look up the argument by name.
                if (!ArgumentSet.TryGetNamedArgument(namedArgType, options, out ArgumentDefinition arg))
                {
                    parsedArgs = null;
                    return ArgumentSetParseResult.UnknownNamedArgument(namedArgType, options);
                }

                parsedArgs = new[]
                {
                    new ArgumentAndValue
                    {
                        Arg = arg,
                        Value = optionArgument
                    }
                };
            }

            // If the last named argument we saw in this token required an
            // option argument to go with it, then yield that information
            // so it can be used by the caller (e.g. in completion generation).
            var lastArg = parsedArgs.GetLastOrDefault();
            if (lastArg != null &&
                lastArg.Arg.RequiresOptionArgument &&
                string.IsNullOrEmpty(lastArg.Value))
            {
                return ArgumentSetParseResult.RequiresOptionArgument(lastArg.Arg);
            }

            return ArgumentSetParseResult.Ready;
        }

        private bool TryParseAndStore(ArgumentParser state, string value)
        {
            if (!state.TryParseAndStore(this, value, out object parsedValue))
            {
                return false;
            }

            // Inspect the parsed value.
            var argProvider = parsedValue as IArgumentProvider;
            if (argProvider == null)
            {
                argProvider = state.DestinationObject as IArgumentProvider;
            }

            if (argProvider != null)
            {
                var definingType = argProvider.GetTypeDefiningArguments();
                if (definingType != null)
                {
                    ReflectionBasedParser.AddToArgumentSet(ArgumentSet, definingType,
                        fixedDestination: argProvider.GetDestinationObject(),
                        containingArgument: state.Argument);
                }
            }

            return true;
        }

        private void ReportUnreadableFile(string filePath) =>
            ReportLine(Strings.UnreadableFile, filePath);

        private void ReportUnrecognizedArgument(ArgumentSetParseResult result, string argument)
        {
            switch (result.State)
            {
                case ArgumentSetParseResultType.UnknownNamedArgument:
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

                case ArgumentSetParseResultType.UnknownPositionalArgument:
                    ReportLine(Strings.UnrecognizedArgument, argument);
                    break;
                case ArgumentSetParseResultType.RequiresOptionArgument:
                    ReportLine(Strings.MissingRequiredOptionArgument, argument);
                    break;
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

        private IEnumerable<string> GetSimilarNamedArguments(ArgumentNameType? type, string name)
        {
            var candidates = ArgumentSet.GetAllArgumentNames();
            var prefix = string.Empty;

            if (type.HasValue)
            {
                switch (type.Value)
                {
                    case ArgumentNameType.LongName:
                        prefix = ArgumentSet.Attribute.NamedArgumentPrefixes.FirstOrDefault();
                        break;
                    case ArgumentNameType.ShortName:
                        prefix = ArgumentSet.Attribute.ShortNameArgumentPrefixes.FirstOrDefault();
                        break;
                }
            }

            return candidates.Where(c => IsUserProvidedStringSimilarTo(name, c))
                .Select(c => prefix + c);
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

        /// <summary>
        /// Tries to find an 'long name argument prefix' in the provided token.
        /// </summary>
        /// <param name="arg">The token to inspect.</param>
        /// <returns>The matching prefix on success; null otherwise.</returns>
        private string TryGetLongNameArgumentPrefix(string arg) =>
            ArgumentSet.Attribute.NamedArgumentPrefixes.FirstOrDefault(
                prefix => arg.StartsWith(prefix, StringComparisonToUse));

        /// <summary>
        /// Tries to find an 'short name argument prefix' in the provided token.
        /// </summary>
        /// <param name="arg">The token to inspect.</param>
        /// <returns>The matching prefix on success; null otherwise.</returns>
        private string TryGetShortNameArgumentPrefix(string arg) =>
            ArgumentSet.Attribute.ShortNameArgumentPrefixes.FirstOrDefault(
                prefix => arg.StartsWith(prefix, StringComparisonToUse));

        /// <summary>
        /// Tries to find an 'answer file prefix' in the provided token.
        /// </summary>
        /// <param name="arg">The token to inspect.</param>
        /// <returns>The matching prefix on success; null otherwise.</returns>
        private string TryGetAnswerFilePrefix(string arg)
        {
            if (ArgumentSet.Attribute.AnswerFileArgumentPrefix == null)
            {
                return null;
            }

            if (!arg.StartsWith(ArgumentSet.Attribute.AnswerFileArgumentPrefix, StringComparisonToUse))
            {
                return null;
            }

            return ArgumentSet.Attribute.AnswerFileArgumentPrefix;
        }

        /// <summary>
        /// String comparer to use for names in this argument set.
        /// </summary>
        private StringComparer StringComparerToUse =>
            ArgumentSet.Attribute.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// String comparison to use for names in this argument set.
        /// </summary>
        private StringComparison StringComparisonToUse =>
            ArgumentSet.Attribute.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        /// <summary>
        /// Valid terminator and separator chars for this argument set.
        /// </summary>
        private IEnumerable<char> ArgumentTerminatorsAndSeparators =>
            ArgumentNameTerminators.Concat(ArgumentSet.Attribute.ArgumentValueSeparators);

        /// <summary>
        /// Valid argument name terminators for this argument set.
        /// </summary>
        private static IEnumerable<char> ArgumentNameTerminators => new[] { '+', '-' };

        private class ArgumentAndValue
        {
            public ArgumentDefinition Arg { get; set; }

            public string Value { get; set; }
        }
    }
}