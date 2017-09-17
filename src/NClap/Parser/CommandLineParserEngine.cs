using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NClap.Exceptions;
using NClap.Metadata;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Parser
{
    /// <summary>
    /// A delegate used in error reporting.
    /// </summary>
    /// <param name="message">Message to report.</param>
    public delegate void ErrorReporter(string message);

    /// <summary>
    /// A delegate used in error reporting.
    /// </summary>
    /// <param name="message">Message to report.</param>
    public delegate void ColoredErrorReporter(ColoredMultistring message);

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
            public Argument Arg { get; set; }
            public string Value { get; set; }
        }

        private const string ArgumentAnswerFileCommentLinePrefix = "#";

        private readonly Dictionary<IMutableMemberInfo, Argument> _argumentsByMember = new Dictionary<IMutableMemberInfo, Argument>();
        private readonly List<Argument> _namedArguments = new List<Argument>();
        private readonly Dictionary<string, Argument> _namedArgumentsByName = new Dictionary<string, Argument>(StringComparer.OrdinalIgnoreCase);
        private readonly SortedList<int, Argument> _positionalArguments = new SortedList<int, Argument>();

        private readonly CommandLineParserOptions _options;
        private readonly ArgumentSetAttribute _setAttribute;

        private int _nextPositionalArgIndexToImport;
        private int _nextPositionalArgIndexToParse;

        /// <summary>
        /// Creates a new command-line argument parser.
        /// </summary>
        /// <param name="type">Type of the parsed arguments.</param>
        public CommandLineParserEngine(Type type) : this(type, null, null)
        {
        }

        /// <summary>
        /// Creates a new command-line argument parser.
        /// </summary>
        /// <param name="defaultValues">Optionally provides an object with default values.</param>
        public CommandLineParserEngine(object defaultValues) : this(defaultValues.GetType(), defaultValues, null)
        {
        }

        /// <summary>
        /// Creates a new command-line argument parser.
        /// </summary>
        /// <param name="type">Destination object type.</param>
        /// <param name="defaultValues">Optionally provides an object with
        /// default values.</param>
        /// <param name="options">Optionally provides additional options
        /// controlling how parsing proceeds.</param>
        public CommandLineParserEngine(Type type, object defaultValues, CommandLineParserOptions options)
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

            // Look for the optional ArgumentSetAttribute on the type; if there
            // isn't one, construct a default empty one.
            _setAttribute = type.GetTypeInfo().GetSingleAttribute<ArgumentSetAttribute>() ?? new ArgumentSetAttribute();

            // Scan the provided type for argument definitions.
            ImportArgumentDefinitionsFromType(type, defaultValues);
            _nextPositionalArgIndexToImport = _positionalArguments.Count;
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
        public bool Parse(IList<string> args, object destination)
        {
            Debug.Assert(args != null);
            Debug.Assert(args.All(x => x != null));

            var hasError = !ParseArgumentList(args, destination);

            // Finalize all arguments: named args first, then positional default
            // args.
            foreach (var arg in _namedArguments.Concat(_positionalArguments.Values))
            {
                hasError = !arg.TryFinalize(destination, _options.FileSystemReader) || hasError;
            }

            return !hasError;
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
        /// <returns>The constructed usage information string.</returns>
        public ColoredMultistring GetUsageInfo(int maxUsageWidth, string commandName, UsageInfoOptions options)
        {
            // Construct info for argument set.
            var info = new ArgumentSetUsageInfo
            {
                Name = commandName ?? AssemblyUtilities.GetAssemblyFileName(),
                Description = _setAttribute.AdditionalHelp,
                DefaultShortNamePrefix = _setAttribute.ShortNameArgumentPrefixes.FirstOrDefault()
            };

            // Add parameters and examples.
            info.AddParameters(GetArgumentUsageInfo());
            if (_setAttribute.Examples != null)
            {
                info.AddExamples(_setAttribute.Examples);
            }

            // Update logo, if one was provided.
            if (_setAttribute.LogoString != null)
            {
                info.Logo = _setAttribute.LogoString;
            }

            // Compose remarks, if any.
            const string defaultHelpArgumentName = "?";
            var namedArgPrefix = _setAttribute.ShortNameArgumentPrefixes.FirstOrDefault();
            if (_namedArgumentsByName.ContainsKey(defaultHelpArgumentName) && namedArgPrefix != null)
            {
                info.Remarks = string.Format(Strings.UsageInfoHelpAdvertisement, $"{info.Name} {namedArgPrefix}{defaultHelpArgumentName}");
            }

            // Construct formatter and use it.
            HelpFormatter formatter;
            if (options.HasFlag(UsageInfoOptions.CondenseOutput))
            {
                formatter = new CondensedHelpFormatter();
            }
            else
            {
                formatter = new PowershellStyleHelpFormatter();
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
            return _namedArguments
                       .Concat(_positionalArguments.Values)
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

            if (indexOfTokenToComplete > tokenList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(indexOfTokenToComplete));
            }

            if (indexOfTokenToComplete == tokenList.Count)
            {
                tokenList = tokenList.Concat(new[] { string.Empty }).ToList();
            }

            var tokenToComplete = tokenList[indexOfTokenToComplete];

            // Create a destination object if provided with a factory.
            var inProgressParsedObject = destObjectFactory?.Invoke();

            // Parse what we've seen thus far (before the token to complete).
            var tokensToParse = tokenList.Take(indexOfTokenToComplete).ToList();
            Parse(tokensToParse, inProgressParsedObject);

            // See where we are.
            var longNameArgumentPrefix = TryGetLongNameArgumentPrefix(tokenToComplete);
            if (longNameArgumentPrefix != null)
            {
                var afterPrefix = tokenToComplete.Substring(longNameArgumentPrefix.Length);
                return GetNamedArgumentCompletions(tokenList, indexOfTokenToComplete, afterPrefix, inProgressParsedObject)
                           .Select(completion => longNameArgumentPrefix + completion);
            }

            var shortNameArgumentPrefix = TryGetShortNameArgumentPrefix(tokenToComplete);
            if (shortNameArgumentPrefix != null)
            {
                var afterPrefix = tokenToComplete.Substring(shortNameArgumentPrefix.Length);
                return GetNamedArgumentCompletions(tokenList, indexOfTokenToComplete, afterPrefix, inProgressParsedObject)
                           .Select(completion => shortNameArgumentPrefix + completion);
            }

            var answerFileArgumentPrefix = TryGetAnswerFilePrefix(tokenToComplete);
            if (answerFileArgumentPrefix != null)
            {
                var filePath = tokenToComplete.Substring(answerFileArgumentPrefix.Length);
                var parseContext = new ArgumentParseContext
                {
                    FileSystemReader = _options.FileSystemReader,
                    ParserContext = _options.Context
                };

                var completionContext = new ArgumentCompletionContext
                {
                    ParseContext = parseContext,
                    TokenIndex = indexOfTokenToComplete,
                    Tokens = tokenList,
                    InProgressParsedObject = inProgressParsedObject
                };

                return ArgumentType.FileSystemPath.GetCompletions(completionContext, filePath)
                           .Select(completion => answerFileArgumentPrefix + completion);
            }

            // It must be a positional argument?
            if (!_positionalArguments.TryGetValue(_nextPositionalArgIndexToParse, out Argument positionalArg))
            {
                return emptyCompletions();
            }

            return positionalArg.GetCompletions(
                tokenList,
                indexOfTokenToComplete,
                tokenToComplete,
                inProgressParsedObject);
        }

        private void ImportArgumentDefinitionsFromType(Type defininingType, object defaultValues = null, object fixedDestination = null, int positionalIndexBias = 0)
        {
            // Extract argument descriptors from the defining type.
            var args = GetArgumentDescriptors(defininingType, _setAttribute, defaultValues, _options, fixedDestination).ToList();

            // Index the descriptors.
            foreach (var arg in args)
            {
                _argumentsByMember.Add(arg.Member, arg);
            }

            // Symmetrically reflect any conflicts.
            foreach (var arg in args)
            {
                foreach (var conflictingMemberName in arg.Attribute.ConflictsWith)
                {
                    var conflictingArgs =
                        args.Where(a => a.Member.MemberInfo.Name.Equals(conflictingMemberName, StringComparison.Ordinal))
                            .ToList();

                    if (conflictingArgs.Count != 1)
                    {
                        throw new InvalidArgumentSetException(arg, string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.ConflictingMemberNotFound,
                            conflictingMemberName,
                            arg.Member.MemberInfo.Name));
                    }

                    // Add the conflict both ways -- if only one says it
                    // conflicts with the other, there's still a conflict.
                    arg.AddConflictingArgument(conflictingArgs[0]);
                    conflictingArgs[0].AddConflictingArgument(arg);
                }
            }

            // Add arguments.
            foreach (var arg in args)
            {
                if (arg.Attribute is NamedArgumentAttribute)
                {
                    ImportNamedArgumentDefinition(arg);
                }
                else if (arg.Attribute is PositionalArgumentAttribute)
                {
                    ImportPositionalArgumentDefinition(arg, positionalIndexBias);
                }
            }

            // Re-validate positional arguments.
            ValidateThatPositionalArgumentsDoNotOverlap();
        }

        private void ImportNamedArgumentDefinition(Argument argument)
        {
            //
            // Validate and register the long name.
            //

            if (_namedArgumentsByName.ContainsKey(argument.LongName))
            {
                throw new InvalidArgumentSetException(argument, string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.DuplicateArgumentLongName,
                    argument.LongName));
            }

            _namedArgumentsByName.Add(argument.LongName, argument);

            //
            // Validate and register the short name.
            //

            if (!string.IsNullOrEmpty(argument.ShortName))
            {
                if (_namedArgumentsByName.TryGetValue(argument.ShortName, out Argument conflictingArg))
                {
                    Debug.Assert(conflictingArg != null);
                    if (argument.ExplicitShortName)
                    {
                        if (conflictingArg.ExplicitShortName)
                        {
                            throw new InvalidArgumentSetException(argument, string.Format(CultureInfo.CurrentCulture,
                                Strings.DuplicateArgumentShortName,
                                argument.ShortName));
                        }
                        else
                        {
                            // TODO: Decide whether this works for dynamically
                            // imported args.
                            _namedArgumentsByName.Remove(conflictingArg.ShortName);
                            conflictingArg.ClearShortName();
                        }
                    }
                    else
                    {
                        argument.ClearShortName();
                    }
                }
            }

            if (!string.IsNullOrEmpty(argument.ShortName))
            {
                if (_setAttribute.AllowMultipleShortNamesInOneToken &&
                    argument.ShortName.Length > 1)
                {
                    throw new InvalidArgumentSetException(argument, string.Format(CultureInfo.CurrentCulture,
                        Strings.ArgumentShortNameTooLong,
                        argument.ShortName));
                }

                _namedArgumentsByName.Add(argument.ShortName, argument);
            }

            // Add to unique list.
            _namedArguments.Add(argument);
        }

        private void ImportPositionalArgumentDefinition(Argument arg, int positionalIndexBias)
        {
            var attrib = (PositionalArgumentAttribute)arg.Attribute;
            var position = positionalIndexBias + attrib.Position;

            if (_positionalArguments.ContainsKey(position))
            {
                throw new InvalidArgumentSetException(arg, string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.DuplicatePositionArguments,
                    _positionalArguments[position].Member.MemberInfo.Name,
                    arg.Member.MemberInfo.Name,
                    position));
            }

            _positionalArguments.Add(position, arg);
            _nextPositionalArgIndexToImport = position + 1;
        }

        private IEnumerable<char> ArgumentTerminatorsAndSeparators =>
            ArgumentNameTerminators.Concat(_setAttribute.ArgumentValueSeparators);

        private static IEnumerable<char> ArgumentNameTerminators => new[] { '+', '-' };

        private static IEnumerable<IMutableMemberInfo> GetAllFieldsAndProperties(Type type, bool includeNonPublicMembers)
        {
            // Generate a list of the fields and properties declared on
            // 'argumentSpecification', and on all types in its inheritance
            // hierarchy.
            var members = new List<IMutableMemberInfo>();
            for (var currentType = type; currentType != null; currentType = currentType.GetTypeInfo().BaseType)
            {
                var bindingFlags =
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.DeclaredOnly;

                if (includeNonPublicMembers)
                {
                    bindingFlags |= BindingFlags.NonPublic;
                }

                members.AddRange(currentType.GetFieldsAndProperties(bindingFlags));
            }

            return members;
        }

        private static IEnumerable<Argument> GetArgumentDescriptors(Type type, ArgumentSetAttribute setAttribute, object defaultValues, CommandLineParserOptions options, object fixedDestination)
        {
            // Find all fields and properties that have argument attributes on
            // them. For each that we find, capture information about them.
            var argList = GetAllFieldsAndProperties(type, includeNonPublicMembers: true)
                .SelectMany(member => CreateArgumentDescriptorsIfApplicable(member, defaultValues, setAttribute, options, fixedDestination));

            // If the argument set attribute indicates that we should also
            // include un-attributed, public, writable members as named
            // arguments, then look for them now.
            if (setAttribute.PublicMembersAreNamedArguments)
            {
                argList = argList.Concat(GetAllFieldsAndProperties(type, includeNonPublicMembers: false)
                    .Where(member => member.IsWritable)
                    .Where(member => member.MemberInfo.GetSingleAttribute<ArgumentBaseAttribute>() == null)
                    .Where(member => member.MemberInfo.GetSingleAttribute<ArgumentGroupAttribute>() == null)
                    .Select(member => CreateArgumentDescriptor(member, new NamedArgumentAttribute(), defaultValues, setAttribute, options, fixedDestination)));
            }

            return argList;
        }

        private static IEnumerable<Argument> CreateArgumentDescriptorsIfApplicable(IMutableMemberInfo member, object defaultValues,
            ArgumentSetAttribute setAttribute, CommandLineParserOptions options, object fixedDestination)
        {
            var descriptors = Enumerable.Empty<Argument>();

            var argAttrib = member.MemberInfo.GetSingleAttribute<ArgumentBaseAttribute>();
            if (argAttrib != null)
            {
                descriptors = descriptors.Concat(new[] { CreateArgumentDescriptor(member, argAttrib, defaultValues, setAttribute, options, fixedDestination) });
            }

            var groupAttrib = member.MemberInfo.GetSingleAttribute<ArgumentGroupAttribute>();
            if (groupAttrib != null)
            {
                descriptors = descriptors.Concat(GetArgumentDescriptors(member.MemberType,
                    setAttribute,
                    /*defaultValues=*/null /*DBG:RRO*/,
                    options,
                    fixedDestination));
            }

            return descriptors;
        }

        private static Argument CreateArgumentDescriptor(
            IMutableMemberInfo member,
            ArgumentBaseAttribute attribute,
            object defaultValues,
            ArgumentSetAttribute setAttribute,
            CommandLineParserOptions options,
            object fixedDestination)
        {
            if (!member.IsReadable || !member.IsWritable)
            {
                var declaringType = member.MemberInfo.DeclaringType;

                throw new InvalidArgumentSetException(member, string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.MemberNotSupported,
                    member.MemberInfo.Name,
                    declaringType?.Name));
            }

            var defaultFieldValue = (defaultValues != null) ? member.GetValue(defaultValues) : null;
            return new Argument(member,
                attribute,
                setAttribute,
                options,
                defaultFieldValue,
                fixedDestination: fixedDestination);
        }

        private void ValidateThatPositionalArgumentsDoNotOverlap()
        {
            var namedArguments = _namedArguments;
            var positionalArguments = _positionalArguments;

            // Validate positional arguments.
            var lastIndex = -1;
            var allArgsConsumed = namedArguments.Any(a => a.TakesRestOfLine);
            foreach (var argument in positionalArguments)
            {
                if (allArgsConsumed || (argument.Key != lastIndex + 1))
                {
                    throw new InvalidArgumentSetException(
                        argument.Value,
                        Strings.NonConsecutivePositionalParameters);
                }

                lastIndex = argument.Key;
                allArgsConsumed = argument.Value.TakesRestOfLine || argument.Value.AllowMultiple;
            }
        }

        private void ReportUnreadableFile(string filePath) =>
            ReportLine(Strings.UnreadableFile, filePath);

        private void ReportUnrecognizedArgument(string argument) =>
            ReportLine(Strings.UnrecognizedArgument, argument);

        /// <summary>
        /// Parses an argument list into an object.
        /// </summary>
        /// <param name="args">String arguments to parse.</param>
        /// <param name="destination">Output arguments object.</param>
        /// <returns>True if no error occurred; false otherwise.</returns>
        private bool ParseArgumentList(IList<string> args, object destination)
        {
            Debug.Assert(args != null);
            Debug.Assert(args.All(x => x != null));

            var hasError = false;

            for (var index = 0; index < args.Count; ++index)
            {
                // Note that we do *not* remove leading or trailing whitespace
                // from the argument value; it might be meaningful.
                var argument = args[index];

                var longNameArgumentPrefix = TryGetLongNameArgumentPrefix(argument);
                var shortNameArgumentPrefix = TryGetShortNameArgumentPrefix(argument);
                var answerFilePrefix = TryGetAnswerFilePrefix(argument);

                IReadOnlyList<ArgumentAndValue> parsedArgs = null;
                if (longNameArgumentPrefix != null || shortNameArgumentPrefix != null)
                {
                    var success = false;

                    if (!success && longNameArgumentPrefix != null)
                    {
                        success = TryParseNamedArgument(argument, longNameArgumentPrefix, NamedArgumentType.LongName, out parsedArgs);
                    }

                    if (!success && shortNameArgumentPrefix != null)
                    {
                        success = TryParseNamedArgument(argument, shortNameArgumentPrefix, NamedArgumentType.ShortName, out parsedArgs);
                    }

                    if (!success)
                    {
                        ReportUnrecognizedArgument(argument);
                        hasError = true;
                        continue;
                    }

                    // If our policy allows a named argument's value to be placed
                    // in the following token, and if we're missing a required
                    // value, and if there's at least one more token, then try
                    // to parse the next token as the current argument's value.
                    if (_setAttribute.AllowNamedArgumentValueAsSucceedingToken &&
                        parsedArgs.GetLast().Arg.RequiresOptionArgument &&
                        !parsedArgs.GetLast().Arg.TakesRestOfLine &&
                        string.IsNullOrEmpty(parsedArgs.GetLast().Value) &&
                        index + 1 < args.Count)
                    {
                        ++index;
                        parsedArgs.GetLast().Value = args[index];
                    }


                    foreach (var parsedArg in parsedArgs)
                    {
                        if (parsedArg.Arg.TakesRestOfLine)
                        {
                            if (parsedArg.Arg.TrySetRestOfLine(parsedArg.Value, args.Skip(index + 1), destination))
                            {
                                index = args.Count; // skip the rest of the line
                            }
                            else
                            {
                                hasError = true;
                            }
                        }
                        else
                        {
                            Debug.Assert(parsedArg.Value != null);
                            hasError = !TryParseAndStore(parsedArg.Arg, parsedArg.Value, destination) || hasError;
                        }
                    }
                }
                else if (answerFilePrefix != null)
                {
                    var filePath = argument.Substring(answerFilePrefix.Length);

                    if (TryLexArgumentAnswerFile(filePath, out IEnumerable<string> nestedArgs))
                    {
                        var nestedArgsArray = nestedArgs.ToArray();

                        hasError = !ParseArgumentList(nestedArgsArray, destination) || hasError;
                    }
                    else
                    {
                        ReportUnreadableFile(filePath);
                        hasError = true;
                    }
                }
                else
                {
                    if (_positionalArguments.TryGetValue(_nextPositionalArgIndexToParse, out Argument positionalArg))
                    {
                        if (positionalArg.TakesRestOfLine)
                        {
                            if (positionalArg.TrySetRestOfLine(args.Skip(index), destination))
                            {
                                index = args.Count; // skip the rest of the line
                            }
                            else
                            {
                                hasError = true;
                            }
                        }
                        else
                        {
                            Debug.Assert(argument != null);
                            hasError = !TryParseAndStore(positionalArg, argument, destination) || hasError;
                        }

                        if (!positionalArg.AllowMultiple)
                        {
                            ++_nextPositionalArgIndexToParse;
                        }
                    }
                    else
                    {
                        ReportUnrecognizedArgument(argument);
                        hasError = true;
                    }
                }
            }

            return !hasError;
        }

        private bool TryParseNamedArgument(string argument, string argumentPrefix, NamedArgumentType namedArgType, out IReadOnlyList<ArgumentAndValue> parsedArgs)
        {
            var prefixLength = argumentPrefix.Length;
            Debug.Assert(argument.Length >= prefixLength);

            // Valid separators include all registered argument value
            // separators plus '+' and '-' for booleans.
            var separators = ArgumentTerminatorsAndSeparators.ToArray();

            // Figure out where the argument name ends.
            var endIndex = argument.IndexOfAny(separators, prefixLength);
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
                if (_setAttribute.ArgumentValueSeparators.Any(sep => argument[prefixLength + options.Length] == sep))
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
            if (_setAttribute.AllowMultipleShortNamesInOneToken &&
                namedArgType == NamedArgumentType.ShortName)
            {
                Debug.Assert(_setAttribute.ShortNamesAreOneCharacterLong);

                // Since short names are one character long, we parse them one at a
                // time, preparing for multiple arguments in this one token.
                var args = new List<ArgumentAndValue>();
                for (var index = 0; index < options.Length; ++index)
                {
                    // Try parsing it as a short name; bail immediately if we find an invalid
                    // one.
                    var possibleShortName = new string(options[index], 1);
                    if (!_namedArgumentsByName.TryGetValue(possibleShortName, out Argument arg))
                    {
                        parsedArgs = null;
                        return false;
                    }

                    // If this parsed as a short name that takes a required option argument,
                    // and we didn't see an option argument, and we allow mushing together
                    // short names and their option arguments, then try parsing the rest of
                    // this token as an option argument.
                    var lastChar = index == options.Length - 1;
                    if (arg.RequiresOptionArgument &&
                        _setAttribute.AllowElidingSeparatorAfterShortName &&
                        optionArgument == null &&
                        !lastChar)
                    {
                        optionArgument = options.Substring(index + 1);
                        index = options.Length - 1;
                        lastChar = true;
                    }

                    args.Add(new ArgumentAndValue
                    {
                        Arg = arg,
                        Value = lastChar ? (optionArgument ?? string.Empty) : string.Empty
                    } );
                }

                parsedArgs = args;
            }
            else
            {
                // Try to look up the argument by name.
                if (!_namedArgumentsByName.TryGetValue(options, out Argument arg))
                {
                    parsedArgs = null;
                    return false;
                }

                parsedArgs = new[] { new ArgumentAndValue { Arg = arg, Value = optionArgument ?? string.Empty } };
            }

            return true;
        }

        private IEnumerable<ArgumentUsageInfo> GetArgumentUsageInfo()
        {
            // Enumerate positional arguments first, in position order.
            foreach (var arg in _positionalArguments.Values.Where(a => !a.Hidden))
            {
                yield return new ArgumentUsageInfo(arg);
            }

            // Enumerate named arguments next, in case-insensitive sort order.
            foreach (var arg in _namedArguments.Where(a => !a.Hidden).OrderBy(a => a.LongName, StringComparer.OrdinalIgnoreCase))
            {
                yield return new ArgumentUsageInfo(arg);
            }

            // Add an extra item for answer files, if that is supported on this
            // argument set.
            if (_setAttribute.AnswerFileArgumentPrefix != null)
            {
                var pseudoArgLongName = Strings.AnswerFileArgumentName;

                if (_setAttribute.NameGenerationFlags.HasFlag(ArgumentNameGenerationFlags.GenerateHyphenatedLowerCaseLongNames))
                {
                    pseudoArgLongName = pseudoArgLongName.ToHyphenatedLowerCase();
                }

                yield return new ArgumentUsageInfo(
                    $"[{_setAttribute.AnswerFileArgumentPrefix}<{pseudoArgLongName}>]*",
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
                return _namedArguments
                           .Select(namedArg => namedArg.LongName)
                           .OrderBy(longName => longName, StringComparer.OrdinalIgnoreCase)
                           .Where(candidateName => candidateName.StartsWith(namedArgumentAfterPrefix, StringComparison.OrdinalIgnoreCase));
            }

            var separator = namedArgumentAfterPrefix[separatorIndex];
            if (!_setAttribute.ArgumentValueSeparators.Contains(separator))
            {
                return emptyCompletions();
            }

            var name = namedArgumentAfterPrefix.Substring(0, separatorIndex);
            var value = namedArgumentAfterPrefix.Substring(separatorIndex + 1);

            if (!_namedArgumentsByName.TryGetValue(name, out Argument arg))
            {
                return emptyCompletions();
            }

            return arg.GetCompletions(tokens, indexOfTokenToComplete, value, inProgressParsedObject)
                      .Select(completion => string.Concat(name, separator.ToString(), completion));
        }

        private bool TryParseAndStore(Argument arg, string value, object dest)
        {
            if (!arg.TryParseAndStore(value, dest, out object parsedValue))
            {
                return false;
            }

            // Inspect the parsed value.
            if (parsedValue is IArgumentProvider argProvider)
            {
                var definingType = argProvider.GetTypeDefiningArguments();
                if (definingType != null)
                {
                    ImportArgumentDefinitionsFromType(definingType,
                        fixedDestination: argProvider.GetDestinationObject(),
                        positionalIndexBias: _nextPositionalArgIndexToImport);
                }
            }

            return true;
        }

        private string TryGetLongNameArgumentPrefix(string arg) =>
            _setAttribute.NamedArgumentPrefixes.FirstOrDefault(
                prefix => arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        private string TryGetShortNameArgumentPrefix(string arg) =>
            _setAttribute.ShortNameArgumentPrefixes.FirstOrDefault(
                prefix => arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        private string TryGetAnswerFilePrefix(string arg)
        {
            if (_setAttribute.AnswerFileArgumentPrefix == null)
            {
                return null;
            }

            if (!arg.StartsWith(_setAttribute.AnswerFileArgumentPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return _setAttribute.AnswerFileArgumentPrefix;
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
            _options.Reporter(string.Format(CultureInfo.CurrentCulture, message + Environment.NewLine, args));
        }
    }
}
