﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private const string ArgumentAnswerFileCommentLinePrefix = "#";

        private readonly IReadOnlyDictionary<string, Argument> _namedArgumentMap;
        private readonly IReadOnlyList<Argument> _namedArguments;
        private readonly SortedList<int, Argument> _positionalArguments;

        private readonly CommandLineParserOptions _options;
        private readonly ArgumentSetAttribute _setAttribute;

        private int _nextPositionalArgIndex;

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
            _options = options?.Clone() ?? new CommandLineParserOptions();

            if (_options.Reporter == null)
            {
                _options.Reporter = err => { };
            }

            if (_options.FileSystemReader == null)
            {
                _options.FileSystemReader = FileSystemReader.Create();
            }

            _setAttribute = type.GetSingleAttribute<ArgumentSetAttribute>() ?? new ArgumentSetAttribute();

            var allArguments = CreateArgumentDescriptors(type, defaultValues, _options).ToList();

            _namedArguments = allArguments.Where(arg => arg.Attribute is NamedArgumentAttribute).ToList();

            _positionalArguments = new SortedList<int, Argument>();
            foreach (var arg in allArguments.Where(arg => arg.Attribute is PositionalArgumentAttribute))
            {
                var attrib = (PositionalArgumentAttribute)arg.Attribute;
                if (_positionalArguments.ContainsKey(attrib.Position))
                {
                    throw new NotSupportedException(Strings.DuplicatePositionArguments);
                }

                _positionalArguments.Add(attrib.Position, arg);
            }

            _namedArgumentMap = CreateNamedArgumentMap(_namedArguments);

            ValidateArguments(_namedArguments, _positionalArguments);
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
            Contract.Requires(args != null, "args cannot be null");
            Contract.Requires(Contract.ForAll(args, x => x != null), "elements of args cannot be null");

            var hasError = !ParseArgumentList(args, destination);

            // Finalize all arguments: named args first, then positional default
            // args.
            foreach (var arg in _namedArguments.Concat(_positionalArguments.Values))
            {
                Contract.Assume(arg != null);
                hasError |= !arg.Finish(destination, _options.FileSystemReader);
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
            // Establish some defaults.
            const int indentWidth = 4;
            const ConsoleColor defaultHeaderFgColor = ConsoleColor.Yellow;
            const ConsoleColor defaultParamMetadataFgColor = ConsoleColor.DarkCyan;

            // Select the colors we'll use.
            var headerFgColor = options.HasFlag(UsageInfoOptions.UseColor) ? new ConsoleColor?(defaultHeaderFgColor) : null;
            ConsoleColor? paramSyntaxFgColor = null;
            var paramMetadataFgColor = options.HasFlag(UsageInfoOptions.UseColor) ? new ConsoleColor?(defaultParamMetadataFgColor) : null;

            // Gather info about the args.
            var allArgsInfo = GetArgumentUsageInfo();
            var requiredArgsInfo = allArgsInfo.Where(a => a.Required).ToList();
            var optionalArgsInfo = allArgsInfo.Where(a => !a.Required).ToList();

            var builder = new ColoredMultistringBuilder();
            var firstSection = true;

            // If requested, add a "logo" for the program.
            if (options.HasFlag(UsageInfoOptions.IncludeLogo))
            {
                var logo = AssemblyUtilities.GetLogo();
                if (logo != null)
                {
                    builder.Append(logo);
                    builder.AppendLine();
                }

                firstSection = false;
            }

            // Define a reusable lambda to append header text (and make sure it's the right color).
            Action<string> appendHeader =
                headerName => builder.AppendLine(new ColoredString(headerName, headerFgColor));

            // Define a reusable lambda to append content text (and make sure it's the right color).
            Action<ColoredString> appendContent =
                value => builder.AppendLine(new ColoredString(
                    StringUtilities.Wrap(value.Content, maxUsageWidth, indent: indentWidth),
                    value.ForegroundColor,
                    value.BackgroundColor));

            if (!firstSection)
            {
                builder.AppendLine();
            }

            // Append the "NAME" section: lists the program name.
            firstSection = false;
            appendHeader(Strings.UsageInfoNameHeader);
            var name = commandName ?? AssemblyUtilities.GetAssemblyFileName();
            appendContent(name);

            // Append the "SYNTAX" section: describes the basic syntax.
            builder.AppendLine();
            appendHeader(Strings.UsageInfoSyntaxHeader);
            var syntaxItems = new[] { name }.Concat(allArgsInfo.Select(h => h.Syntax));
            var syntax = string.Join(" ", syntaxItems);
            appendContent(syntax);

            // If present, display the "DESCRIPTION" for the program here.
            if (_setAttribute.AdditionalHelp != null)
            {
                builder.AppendLine();
                appendHeader(Strings.UsageInfoDescriptionHeader);
                appendContent(_setAttribute.AdditionalHelp);
            }

            // Define a private lambda that appends parameter info.
            Action<IEnumerable<ArgumentUsageInfo>> appendParametersSection = argsInfo =>
            {
                var firstArg = true;
                foreach (var argInfo in argsInfo)
                {
                    if (!firstArg)
                    {
                        builder.AppendLine();
                    }

                    // Append parameter syntax info.
                    appendContent(new ColoredString(argInfo.Syntax, paramSyntaxFgColor));

                    // Append parameter's short name (if it has one).
                    if (options.HasFlag(UsageInfoOptions.IncludeParameterShortNameAliases) &&
                        !string.IsNullOrEmpty(argInfo.ShortName))
                    {
                        appendContent(new ColoredString(string.Format(
                            CultureInfo.CurrentCulture,
                            "Short form: {0}{1}",
                            _setAttribute.NamedArgumentPrefixes[0],
                            argInfo.ShortName),
                            paramMetadataFgColor));
                    }

                    // Append the parameter's default value (if it has one).
                    if (options.HasFlag(UsageInfoOptions.IncludeParameterDefaultValues) &&
                        !string.IsNullOrEmpty(argInfo.DefaultValue))
                    {
                        appendContent(new ColoredString(string.Format(
                            CultureInfo.CurrentCulture,
                            "Default value: {0}",
                            argInfo.DefaultValue),
                            paramMetadataFgColor));
                    }

                    // Append the parameter's description (if it has one).
                    if (!string.IsNullOrEmpty(argInfo.Description))
                    {
                        builder.AppendLine(StringUtilities.Wrap(
                            argInfo.Description,
                            maxUsageWidth,
                            indent: indentWidth * 2));
                    }

                    firstArg = false;
                }
            };

            if (options.HasFlag(UsageInfoOptions.IncludeParameterDescriptions))
            {
                // Append "REQUIRED PARAMETERS" section.
                if (requiredArgsInfo.Count > 0)
                {
                    builder.AppendLine();
                    appendHeader(Strings.UsageInfoRequiredParametersHeader);
                    appendParametersSection(requiredArgsInfo);
                }

                // Append "OPTIONAL PARAMETERS" section.
                if (optionalArgsInfo.Count > 0)
                {
                    builder.AppendLine();
                    appendHeader(Strings.UsageInfoOptionalParametersHeader);
                    appendParametersSection(optionalArgsInfo);
                }
            }

            // If present, append "EXAMPLES" section.
            if (options.HasFlag(UsageInfoOptions.IncludeExamples) &&
                (_setAttribute.Examples != null) &&
                (_setAttribute.Examples.Length > 0))
            {
                builder.AppendLine();
                appendHeader(Strings.UsageInfoExamplesHeader);

                foreach (var example in _setAttribute.Examples)
                {
                    appendContent(example);
                }
            }

            return builder.ToMultistring();
        }

        /// <summary>
        /// Formats a parsed set of arguments back into tokenized string form.
        /// </summary>
        /// <param name="value">The parsed argument set.</param>
        /// <returns>The tokenized string.</returns>
        public IEnumerable<string> Format(object value)
        {
            // First format named arguments, then positional default arguments.
            return _namedArguments.Concat(_positionalArguments.Values)
                                  .Select(arg => new { Argument = arg, Value = arg.GetValue(value) })
                                  .Where(argAndValue => (argAndValue.Value != null) && !argAndValue.Value.Equals(argAndValue.Argument.DefaultValue))
                                  .SelectMany(argAndValue => argAndValue.Argument.Format(_setAttribute, argAndValue.Value))
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
            var namedArgumentPrefix = TryGetNamedArgumentPrefix(tokenToComplete);
            if (namedArgumentPrefix != null)
            {
                var afterPrefix = tokenToComplete.Substring(namedArgumentPrefix.Length);
                return GetNamedArgumentCompletions(tokenList, indexOfTokenToComplete, afterPrefix, inProgressParsedObject)
                           .Select(completion => namedArgumentPrefix + completion);
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
            Argument positionalArg;
            if (!_positionalArguments.TryGetValue(_nextPositionalArgIndex, out positionalArg))
            {
                return emptyCompletions();
            }

            return positionalArg.GetCompletions(
                tokenList,
                indexOfTokenToComplete,
                tokenToComplete,
                inProgressParsedObject);
        }

        /// <summary>
        /// Code contracts object invariant for this class.
        /// </summary>
        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1811: Avoid uncalled private code", Justification = "Invoked by Code Contracts")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "It does use local members in debug builds")]
        internal void ObjectInvariant()
        {
            Contract.Invariant(_namedArgumentMap != null);
            Contract.Invariant(_namedArguments != null);
            Contract.Invariant(_positionalArguments != null);
            Contract.Invariant(_options != null);
            Contract.Invariant(_setAttribute != null);
        }

        private IEnumerable<char> ArgumentValueSeparators =>
            _setAttribute.ArgumentValueSeparators;

        private IEnumerable<char> ArgumentTerminatorsAndSeparators =>
            ArgumentNameTerminators.Concat(ArgumentValueSeparators);

        private static IEnumerable<char> ArgumentNameTerminators => new[] { '+', '-' };

        private static IEnumerable<IMutableMemberInfo> GetAllFieldsAndProperties(Type type)
        {
            // Generate a list of the fields and properties declared on
            // 'argumentSpecification', and on all types in its inheritance
            // hierarchy.
            var members = new List<IMutableMemberInfo>();
            for (var currentType = type; currentType != null; currentType = currentType.BaseType)
            {
                const BindingFlags bindingFlags =
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly;

                members.AddRange(currentType.GetFieldsAndProperties(bindingFlags));
            }

            return members;
        }

        private static IEnumerable<Argument> CreateArgumentDescriptors(Type type, object defaultValues, CommandLineParserOptions options)
        {
            // Find all fields and properties that have argument attributes on
            // them. For each that we find, capture information about them.
            var args = GetAllFieldsAndProperties(type)
                           .Select(member => CreateArgumentDescriptor(member, defaultValues, options))
                           .Where(arg => arg != null)
                           .ToDictionary(arg => arg.Member, arg => arg);

            // Now connect up any conflicts, now that we've created all args.
            foreach (var arg in args.Values)
            {
                foreach (var conflictingMemberName in arg.Attribute.ConflictsWith)
                {
                    var conflictingArgs =
                        args.Where(pair => pair.Key.MemberInfo.Name.Equals(conflictingMemberName, StringComparison.Ordinal))
                            .ToList();

                    if (conflictingArgs.Count != 1)
                    {
                        throw new InvalidDataException(string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.ConflictingMemberNotFound,
                            conflictingMemberName,
                            arg.Member.MemberInfo.Name));
                    }

                    // Add the conflict both ways -- if only one says it
                    // conflicts with the other, there's still a conflict.
                    arg.AddConflictingArgument(conflictingArgs[0].Value);
                    conflictingArgs[0].Value.AddConflictingArgument(arg);
                }
            }

            return args.Values;
        }

        private static Argument CreateArgumentDescriptor(IMutableMemberInfo member, object defaultValues, CommandLineParserOptions options)
        {
            var attribute = member.MemberInfo.GetSingleAttribute<ArgumentBaseAttribute>();
            if (attribute == null)
            {
                return null;
            }

            if (!member.IsReadable || !member.IsWritable)
            {
                var declaringType = member.MemberInfo.DeclaringType;
                Contract.Assume(declaringType != null);

                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Strings.MemberNotSupported, member.MemberInfo.Name, declaringType.Name));
            }

            var defaultFieldValue = (defaultValues != null) ? member.GetValue(defaultValues) : null;
            return new Argument(member, attribute, options, defaultFieldValue);
        }

        private static IReadOnlyDictionary<string, Argument> CreateNamedArgumentMap(IReadOnlyList<Argument> arguments)
        {
            var argumentMap = new Dictionary<string, Argument>(StringComparer.OrdinalIgnoreCase);

            // Add explicit names to the map.
            foreach (var argument in arguments)
            {
                Contract.Assume(argument != null);

                if (argumentMap.ContainsKey(argument.LongName))
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Strings.DuplicateArgumentLongName, argument.LongName));
                }

                argumentMap.Add(argument.LongName, argument);

                if (!argument.ExplicitShortName || string.IsNullOrEmpty(argument.ShortName))
                {
                    continue;
                }

                if (argumentMap.ContainsKey(argument.ShortName))
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Strings.DuplicateArgumentShortName, argument.ShortName));
                }

                argumentMap.Add(argument.ShortName, argument);
            }

            // Add implicit short names that don't collide to the map.
            foreach (var argument in arguments.Where(a => !a.ExplicitShortName))
            {
                if (!string.IsNullOrEmpty(argument.ShortName) &&
                    !argumentMap.ContainsKey(argument.ShortName))
                {
                    argumentMap[argument.ShortName] = argument;
                }
                else
                {
                    argument.ClearShortName();
                }
            }

            return argumentMap;
        }

        private static void ValidateArguments(IEnumerable<Argument> namedArguments, SortedList<int, Argument> positionalArguments)
        {
            // Validate positional arguments.
            var lastIndex = -1;
            var allArgsConsumed = namedArguments.Any(a => a.TakesRestOfLine);
            foreach (var argument in positionalArguments)
            {
                if (allArgsConsumed || (argument.Key != lastIndex + 1))
                {
                    throw new NotSupportedException(Strings.NonConsecutivePositionalParameters);
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
            Contract.Requires((args != null) && Contract.ForAll(args, x => x != null));

            var hasError = false;

            for (var index = 0; index < args.Count; ++index)
            {
                // Remove leading and trailing whitespace from the argument
                // value.
                var argument = args[index].Trim();

                var namedArgumentPrefix = TryGetNamedArgumentPrefix(argument);
                var answerFilePrefix = TryGetAnswerFilePrefix(argument);

                if (namedArgumentPrefix != null)
                {
                    var prefixLength = namedArgumentPrefix.Length;
                    Contract.Assert(argument.Length >= prefixLength, "Domain knowledge");

                    // Valid separators include all registered argument value
                    // separators plus '+' and '-' for booleans.
                    var separators = ArgumentTerminatorsAndSeparators.ToArray();

                    var endIndex = argument.IndexOfAny(separators, prefixLength);
                    Contract.Assume(endIndex != 0, "Missing postcondition");

                    // Extract the argument's name, separate from its prefix
                    // or optional argument.
                    var option = argument.Substring(
                        prefixLength,
                        endIndex < 0 ? argument.Length - prefixLength : endIndex - prefixLength);

                    string optionArgument;
                    if (argument.Length > prefixLength + option.Length &&
                        _setAttribute.ArgumentValueSeparators.Any(sep => argument[prefixLength + option.Length] == sep))
                    {
                        optionArgument = argument.Substring(prefixLength + option.Length + 1);
                    }
                    else
                    {
                        optionArgument = argument.Substring(prefixLength + option.Length);
                    }

                    Argument arg;
                    if (!_namedArgumentMap.TryGetValue(option, out arg))
                    {
                        ReportUnrecognizedArgument(argument);
                        hasError = true;
                    }
                    else if (arg.TakesRestOfLine)
                    {
                        arg.SetRestOfLine(optionArgument, args.Skip(index + 1), destination);
                        index = args.Count; // skip the rest of the line
                    }
                    else
                    {
                        hasError |= !arg.SetValue(optionArgument, destination);
                    }
                }
                else if (answerFilePrefix != null)
                {
                    var filePath = argument.Substring(answerFilePrefix.Length);

                    IEnumerable<string> nestedArgs;
                    if (LexArgumentAnswerFile(filePath, out nestedArgs))
                    {
                        var nestedArgsArray = nestedArgs.ToArray();
                        Contract.Assume((nestedArgs == null) || Contract.ForAll(nestedArgsArray, x => x != null));

                        hasError |= !ParseArgumentList(nestedArgsArray, destination);
                    }
                    else
                    {
                        ReportUnreadableFile(filePath);
                        hasError = true;
                    }
                }
                else
                {
                    Argument positionalArg;
                    if (_positionalArguments.TryGetValue(_nextPositionalArgIndex, out positionalArg))
                    {
                        if (positionalArg.TakesRestOfLine)
                        {
                            positionalArg.SetRestOfLine(args.Skip(index), destination);
                            index = args.Count; // skip the rest of the line
                        }
                        else
                        {
                            hasError |= !positionalArg.SetValue(argument, destination);
                        }

                        if (!positionalArg.AllowMultiple)
                        {
                            ++_nextPositionalArgIndex;
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

        private ArgumentUsageInfo[] GetArgumentUsageInfo()
        {
            var help = new ArgumentUsageInfo[NumberOfArgumentsToDisplay()];

            var index = 0;

            // Enumerate named arguments, in case-insensitive sort order.
            foreach (var arg in _namedArguments.Where(a => a.Hidden == false).OrderBy(a => a.LongName, StringComparer.OrdinalIgnoreCase))
            {
                Contract.Assume(arg != null);
                Contract.Assume(index < help.Length, "Because of NumberOfParametersToDisplay()");
                help[index++] = new ArgumentUsageInfo(_setAttribute, arg);
            }

            // Enumerate positional arguments, in position order.
            foreach (var arg in _positionalArguments.Values.Where(a => a.Hidden == false))
            {
                Contract.Assume(arg != null);
                Contract.Assume(index < help.Length, "Because of NumberOfParametersToDisplay()");
                help[index++] = new ArgumentUsageInfo(_setAttribute, arg);
            }

            if ((index > 0) && (_setAttribute.AnswerFileArgumentPrefix != null))
            {
                Contract.Assume(index < help.Length, "Because of NumberOfParametersToDisplay()");
                help[index++] = new ArgumentUsageInfo(
                    string.Format(CultureInfo.CurrentCulture, "[{0}<file>]*", _setAttribute.AnswerFileArgumentPrefix),
                    "Read response file for more options.",
                    null,
                    null,
                    false);
            }

            Debug.Assert(index == help.Length);

            return help;
        }

        private IEnumerable<string> GetNamedArgumentCompletions(IReadOnlyList<string> tokens, int indexOfTokenToComplete, string namedArgumentAfterPrefix, object inProgressParsedObject)
        {
            Func<IEnumerable<string>> emptyCompletions = Enumerable.Empty<string>;

            var separatorIndex = namedArgumentAfterPrefix.IndexOfAny(ArgumentTerminatorsAndSeparators.ToArray());
            if (separatorIndex < 0)
            {
                return _namedArguments.Select(namedArg => namedArg.LongName)
                                      .OrderBy(longName => longName, StringComparer.OrdinalIgnoreCase)
                                      .Where(candidateName => candidateName.StartsWith(namedArgumentAfterPrefix, StringComparison.OrdinalIgnoreCase));
            }

            var separator = namedArgumentAfterPrefix[separatorIndex];
            if (!ArgumentValueSeparators.Contains(separator))
            {
                return emptyCompletions();
            }

            var name = namedArgumentAfterPrefix.Substring(0, separatorIndex);
            var value = namedArgumentAfterPrefix.Substring(separatorIndex + 1);

            Argument arg;
            if (!_namedArgumentMap.TryGetValue(name, out arg))
            {
                return emptyCompletions();
            }

            return arg.GetCompletions(tokens, indexOfTokenToComplete, value, inProgressParsedObject)
                      .Select(completion => string.Concat(name, separator.ToString(), completion));
        }

        private string TryGetNamedArgumentPrefix(string arg)
        {
            return _setAttribute.NamedArgumentPrefixes.FirstOrDefault(
                       prefix => arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

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

        private int NumberOfArgumentsToDisplay()
        {
            var count =
                _namedArguments.Count(a => a.Hidden == false) +
                _positionalArguments.Count(a => a.Value.Hidden == false);

            // Add one more for the answer file syntax if there are other
            // arguments and if the answer file syntax has been enabled.
            if ((count > 0) && (_setAttribute.AnswerFileArgumentPrefix != null))
            {
                ++count;
            }

            return count;
        }

        private bool LexArgumentAnswerFile(string filePath, out IEnumerable<string> arguments)
        {
            try
            {
                // Read through all non-empty lines in the file.
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
            Contract.Requires(_options != null);
            Contract.Requires(_options.Reporter != null);
            _options.Reporter(string.Format(CultureInfo.CurrentCulture, message + Environment.NewLine, args));
        }
    }
}