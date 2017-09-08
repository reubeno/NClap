using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using NClap.Exceptions;
using NClap.Parser;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Metadata
{
    /// <summary>
    /// A delegate used in error reporting.
    /// </summary>
    /// <param name="message">Message to report.</param>
    public delegate void ErrorReporter(string message);
    
    /// <summary>
    /// Describes a field in a .NET class that is bound to a command-line
    /// parameter.
    /// </summary>
    internal class Argument
    {
        // Parameters
        private readonly IReadOnlyList<ArgumentValidationAttribute> _validationAttributes;
        private readonly ArgumentSetAttribute _setAttribute;
        private readonly bool _isPositional;
        private readonly ColoredErrorReporter _reporter;
        private readonly IArgumentType _valueType;
        private readonly ICollectionArgumentType _collectionArgType;

        private readonly HashSet<Argument> _conflictingArgs = new HashSet<Argument>();
        private readonly ArgumentParseContext _argumentParseContext;

        // State
        private readonly ArrayList _collectionValues;

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="member">Field to describe.</param>
        /// <param name="attribute">Argument attribute on the field.</param>
        /// <param name="setAttribute">Attribute on the containing argument set.</param>
        /// <param name="options">Provides parser options.</param>
        /// <param name="defaultFieldValue">Default value for the field.</param>
        public Argument(IMutableMemberInfo member, ArgumentBaseAttribute attribute, ArgumentSetAttribute setAttribute, CommandLineParserOptions options, object defaultFieldValue = null)
        {
            Debug.Assert(attribute != null);
            Debug.Assert(member != null);

            Member = member;
            Attribute = attribute;
            _setAttribute = setAttribute;
            _isPositional = attribute is PositionalArgumentAttribute;
            _reporter = options?.Reporter ?? (s => { });
            ArgumentType = Attribute.GetArgumentType(member.MemberType);
            _collectionArgType = AsCollectionType(ArgumentType);
            HasDefaultValue = attribute.ExplicitDefaultValue || attribute.DynamicDefaultValue;
            _validationAttributes = GetValidationAttributes(ArgumentType, Member);
            _argumentParseContext = CreateParseContext(attribute, options);

            LongName = GetLongName(attribute, setAttribute, member.MemberInfo);
            ExplicitShortName = HasExplicitShortName(attribute);
            ShortName = GetShortName(attribute, setAttribute, member.MemberInfo);
            DefaultValue = GetDefaultValue(attribute, member, defaultFieldValue);

            var nullableBase = Nullable.GetUnderlyingType(member.MemberType);
            
            if (_collectionArgType != null)
            {
                _collectionValues = new ArrayList();
                _valueType = _collectionArgType.ElementType;
            }
            else if (nullableBase != null)
            {
                // For nullable arguments, we use the wrapped type (T in
                // Nullable<T>) as the value type. Parsing an enum or int is the
                // same as parsing an enum? or int?, for example, since null can
                // only arise if the value was not provided at all.
                _valueType = Attribute.GetArgumentType(nullableBase);
            }
            else
            {
                _valueType = ArgumentType;
            }

            Debug.Assert(_valueType != null);

            if (Unique && !IsCollection)
            {
                throw new InvalidArgumentSetException(member, Strings.UniqueUsedOnNonCollectionArgument);
            }

            Debug.Assert(!string.IsNullOrEmpty(LongName));
        }

        /// <summary>
        /// The argument's long form name, if one exists; null otherwise.
        /// </summary>
        public string LongName { get; }

        /// <summary>
        /// The argument's short form name, if one exists; null otherwise.
        /// </summary>
        public string ShortName { get; private set; }

        /// <summary>
        /// True indicates that the argument's short form name was explicitly
        /// specified; false indicates that the name was defaulted.
        /// </summary>
        public bool ExplicitShortName { get; }

        /// <summary>
        /// True if the argument has a default value, false otherwise.
        /// </summary>
        public bool HasDefaultValue { get; }

        /// <summary>
        /// The default value to associate with the argument if one is not
        /// specified on the command line being parsed.
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// The effective default value associated with the argument.  This
        /// is equivalent to DefaultValue if HasDefaultValue is true.
        /// </summary>
        public object EffectiveDefaultValue
        {
            get
            {
                var defaultValue = HasDefaultValue ? DefaultValue : null;
                if ((defaultValue == null) && !IsCollection)
                {
                    defaultValue = ArgumentType.Type.GetDefaultValue();
                }

                return defaultValue;
            }
        }

        /// <summary>
        /// True indicates that this argument consumes the remainder of the
        /// command line being parsed; false indicates that it only consumes
        /// a single command-line token.
        /// </summary>
        public bool TakesRestOfLine => Attribute.Flags.HasFlag(ArgumentFlags.RestOfLine);

        /// <summary>
        /// True indicates that this argument is required to be present; false
        /// indicates that it's optional.
        /// </summary>
        public bool IsRequired => Attribute.Flags.HasFlag(ArgumentFlags.Required);

        /// <summary>
        /// True indicates that this argument may be specified multiple times
        /// on the command line; false indicates that it may only be specified
        /// once.
        /// </summary>
        public bool AllowMultiple => Attribute.Flags.HasFlag(ArgumentFlags.Multiple);

        /// <summary>
        /// True indicates that each instance of this argument must be unique;
        /// false indicates that this restriction is not applicable.
        /// </summary>
        public bool Unique => Attribute.Flags.HasFlag(ArgumentFlags.Unique);

        /// <summary>
        /// True if the argument is a collection argument; false if it's a
        /// scalar.
        /// </summary>
        public bool IsCollection => _collectionArgType != null;

        /// <summary>
        /// True indicates that this argument should not be mentioned by
        /// usage help information; false indicates that it should be included.
        /// </summary>
        public bool Hidden => Attribute.Hidden;

        /// <summary>
        /// State variable indicating whether this argument has been seen in
        /// the currently-being-parsed command line.
        /// </summary>
        public bool SeenValue
        {
            get; private set;
        }

        /// <summary>
        /// The argument's static metadata.
        /// </summary>
        public ArgumentBaseAttribute Attribute { get; }

        /// <summary>
        /// The object member bound to this argument.
        /// </summary>
        public IMutableMemberInfo Member { get; }

        /// <summary>
        /// Type of the argument.
        /// </summary>
        public IArgumentType ArgumentType { get; }

        /// <summary>
        /// Registers an Argument that conflicts with the one described by this
        /// object.  If the specified argument has already previously been
        /// registered, then this operation is a no-op.
        /// </summary>
        /// <param name="arg">The conflicting argument.</param>
        public void AddConflictingArgument(Argument arg)
        {
            if (arg == this)
            {
                throw new ArgumentOutOfRangeException(nameof(arg));
            }

            _conflictingArgs.Add(arg);
        }

        /// <summary>
        /// Formats the argument into a string.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <param name="suppressArgNames">True to suppress argument names;
        /// false to leave them in.</param>
        /// <returns>The formatted string.</returns>
        public IEnumerable<string> Format(object value, bool suppressArgNames = false)
        {
            if (_collectionArgType != null)
            {
                foreach (var item in _collectionArgType.ToEnumerable(value))
                {
                    if (suppressArgNames)
                    {
                        yield return _collectionArgType.ElementType.Format(item);
                    }
                    else
                    {
                        yield return Format(_collectionArgType.ElementType, item);
                    }
                }
            }
            else if (suppressArgNames)
            {
                yield return ArgumentType.Format(value);
            }
            else
            {
                yield return Format(ArgumentType, value);
            }
        }

        /// <summary>
        /// Generates syntax help information for this argument.
        /// </summary>
        /// <param name="detailed">true to return detailed information,
        /// including full argument type information; false to return abridged
        /// information.</param>
        /// <returns>The help content in string form.</returns>
        public string GetSyntaxHelp(bool detailed = true)
        {
            var builder = new StringBuilder();

            if (!IsRequired)
            {
                builder.Append("[");
            }

            if (_isPositional)
            {
                builder.Append("<");
                builder.Append(LongName);

                if (detailed)
                {
                    builder.Append(" : ");
                    builder.Append(ArgumentType.SyntaxSummary);
                }

                builder.Append(">");

                if (TakesRestOfLine)
                {
                    builder.Append("...");
                }
            }
            else
            {
                if ((_setAttribute.NamedArgumentPrefixes.Length < 1) ||
                    (_setAttribute.ArgumentValueSeparators.Length < 1))
                {
                    throw new NotSupportedException();
                }

                builder.Append(_setAttribute.NamedArgumentPrefixes[0]);
                builder.Append(LongName);

                // We use a special hard-coded syntax if this argument consumes
                // the rest of the line.
                if (TakesRestOfLine)
                {
                    builder.Append("=<...>");
                }

                // We special-case bool arguments (switches) whose default value
                // is false; in such cases, we can get away with a shorter
                // syntax help that just indicates how to flip the switch on.
                else if ((ArgumentType.Type == typeof(bool)) && !((bool)EffectiveDefaultValue))
                {
                }

                // Otherwise, spell out the full syntax.
                else
                {
                    // Decide if the argument type supports empty strings.
                    var supportsEmptyStrings = IsEmptyStringValid();
                    if (supportsEmptyStrings)
                    {
                        builder.Append("[");
                    }

                    builder.Append(_setAttribute.ArgumentValueSeparators[0]);
                    builder.Append(ArgumentType.SyntaxSummary);

                    if (supportsEmptyStrings)
                    {
                        builder.Append("]");
                    }
                }
            }

            if (!IsRequired)
            {
                builder.Append("]");
            }

            if (AllowMultiple)
            {
                builder.Append(IsRequired ? "+" : "*");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Finalizes parsing of the argument, reporting any errors from policy
        /// violations (e.g. missing required arguments).
        /// </summary>
        /// <typeparam name="T">Type of the field associated with the argument.
        /// </typeparam>
        /// <param name="destination">The destination object being filled in.
        /// </param>
        /// <param name="fileSystemReader">File system reader to use.</param>
        /// <returns>True indicates that finalization completed successfully;
        /// false indicates that a failure occurred.</returns>
        public bool TryFinalize<T>(T destination, IFileSystemReader fileSystemReader)
        {
            if (!SeenValue && HasDefaultValue)
            {
                if (!TryValidateValue(DefaultValue, new ArgumentValidationContext(fileSystemReader)))
                {
                    return false;
                }

                if (destination != null)
                {
                    if (!TrySetValue(destination, DefaultValue))
                    {
                        return false;
                    }
                }
            }

            // For RestOfLine arguments, null means not seen, 0-length array means argument was given
            // but the rest of the line was empty, longer array contains rest of the line.
            if (IsCollection && (SeenValue || !TakesRestOfLine) && (destination != null))
            {
                if (!TryCreateCollection(_collectionArgType, _collectionValues, out object collection))
                {
                    return false;
                }

                if (!TrySetValue(destination, collection))
                {
                    return false;
                }
            }

            if (IsRequired && !SeenValue)
            {
                ReportMissingRequiredArgument();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves the value associated with this argument in the provided
        /// containing object.
        /// </summary>
        /// <param name="containingValue">The containing object.</param>
        /// <returns>The value associated with this argument's field.</returns>
        public object GetValue(object containingValue) => Member.GetValue(containingValue);

        /// <summary>
        /// Parses the provided value string using this object's value type,
        /// and stores the parsed value in the provided destination.
        /// </summary>
        /// <typeparam name="T">Type of the expected parsed value.</typeparam>
        /// <param name="value">The string to parse.</param>
        /// <param name="destination">The destination for the parsed value.
        /// </param>
        /// <returns>True on success; false otherwise.</returns>
        public bool SetValue<T>(string value, T destination)
        {
            // Check for disallowed duplicate arguments.
            if (SeenValue && !AllowMultiple)
            {
                ReportDuplicateArgumentValue(value);
                return false;
            }

            // Check for conflicting arguments that have already been specified.
            foreach (var arg in _conflictingArgs.Where(arg => arg.SeenValue))
            {
                ReportConflictingArgument(value, arg);
                return false;
            }

            // Note that we've now seen a value for this argument so we can
            // catch disallowed duplicates later.
            SeenValue = true;

            // Parse the string version of the value.
            if (!ParseValue(value, out object newValue))
            {
                return false;
            }

            if (!TryValidateValue(newValue, new ArgumentValidationContext(_argumentParseContext.FileSystemReader)))
            {
                return false;
            }

            if (IsCollection)
            {
                // Check for disallowed duplicate values in this argument.
                if (Unique && _collectionValues.Contains(newValue))
                {
                    ReportDuplicateArgumentValue(value);
                    return false;
                }

                // Add the value to the collection.
                _collectionValues.Add(newValue);
            }
            else if (IsObjectPresent(destination))
            {
                if (!TrySetValue(destination, newValue, value))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Fills out this argument with the remainder of the provided command
        /// line.
        /// </summary>
        /// <typeparam name="T">Type of the object being filled in.</typeparam>
        /// <param name="first">First command-line token.</param>
        /// <param name="restOfLine">Remainder of the command-line tokens.</param>
        /// <param name="destination">Object being filled in.</param>
        public bool TrySetRestOfLine<T>(string first, IEnumerable<string> restOfLine, T destination)
        {
            Debug.Assert(restOfLine != null);
            return TrySetRestOfLine(new[] { first }.Concat(restOfLine), destination);
        }

        /// <summary>
        /// Fills out this argument with the remainder of the provided command
        /// line.
        /// </summary>
        /// <typeparam name="T">Type of the object being filled in.</typeparam>
        /// <param name="restOfLine">Remainder of the command-line tokens.</param>
        /// <param name="destination">Object being filled in.</param>
        public bool TrySetRestOfLine<T>(IEnumerable<string> restOfLine, T destination)
        {
            Debug.Assert(restOfLine != null);
            Debug.Assert(SeenValue == false);

            SeenValue = true;

            if (IsCollection)
            {
                foreach (var arg in restOfLine)
                {
                    _collectionValues.Add(arg);
                }

                return true;
            }
            else if (IsObjectPresent(destination))
            {
                var restOfLineAsList = restOfLine.ToList();
                return TrySetValue(
                    destination,
                    CreateCommandLine(restOfLineAsList),
                    string.Join(" ", restOfLineAsList));
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Clears the short name associated with this argument.
        /// </summary>
        public void ClearShortName() => ShortName = null;

        /// <summary>
        /// Generate possible completions of this argument that start with the
        /// provided string prefix.
        /// </summary>
        /// <param name="tokens">The set of tokens in the input being completed.
        /// </param>
        /// <param name="indexOfTokenToComplete">The 0-based index of the token
        /// to complete.</param>
        /// <param name="valueToComplete">The prefix string.</param>
        /// <param name="inProgressParsedObject">Optionally, the object
        /// resulting from parsing and processing the tokens before the one
        /// being completed.</param>
        /// <returns>Possible completions.</returns>
        public IEnumerable<string> GetCompletions(IReadOnlyList<string> tokens, int indexOfTokenToComplete, string valueToComplete, object inProgressParsedObject)
        {
            var context = new ArgumentCompletionContext
            {
                ParseContext = _argumentParseContext,
                Tokens = tokens,
                TokenIndex = indexOfTokenToComplete,
                InProgressParsedObject = inProgressParsedObject
            };

            return ArgumentType.GetCompletions(context, valueToComplete);
        }

        /// <summary>
        /// Checks whether this argument requires an option argument.
        /// </summary>
        /// <returns>true if it's required, false if it's optional.</returns>
        public bool RequiresOptionArgument => !IsEmptyStringValid();

        private static ArgumentParseContext CreateParseContext(ArgumentBaseAttribute attribute, CommandLineParserOptions options) =>
            new ArgumentParseContext
            {
                NumberOptions = attribute.NumberOptions,
                AllowEmpty = attribute.AllowEmpty,
                FileSystemReader = options.FileSystemReader,
                ParserContext = options.Context
            };

        private static IReadOnlyList<ArgumentValidationAttribute> GetValidationAttributes(IArgumentType argType, IMutableMemberInfo memberInfo)
        {
            var member = memberInfo.MemberInfo;

            var collectionArgType = AsCollectionType(argType);
            var argTypeToCheck = (collectionArgType != null) ? collectionArgType.ElementType : argType;

            var attributes = member.GetCustomAttributes<ArgumentValidationAttribute>().ToList();
            foreach (var attrib in attributes.Where(attrib => !attrib.AcceptsType(argTypeToCheck)))
            {
                throw new InvalidArgumentSetException(memberInfo, string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.BadValidationAttribute,
                    attrib.GetType().Name,
                    argType.Type.Name));
            }

            return attributes;
        }

        private static ICollectionArgumentType AsCollectionType(IArgumentType type)
        {
            if (type is ArgumentTypeExtension extension)
            {
                type = extension.InnerType;
            }

            return type as ICollectionArgumentType;
        }

        private static string GetLongName(ArgumentBaseAttribute attribute, ArgumentSetAttribute setAttribute, MemberInfo member)
        {
            if (attribute.LongName != null)
            {
                return attribute.LongName;
            }

            var longName = member.Name;

            if (setAttribute.NameGenerationFlags.HasFlag(ArgumentNameGenerationFlags.GenerateHyphenatedLowerCaseLongNames))
            {
                longName = longName.ToHyphenatedLowerCase();
            }

            return longName;
        }

        private static string GetShortName(ArgumentBaseAttribute attribute, ArgumentSetAttribute setAttribute, MemberInfo member)
        {
            if (attribute is PositionalArgumentAttribute)
            {
                return null;
            }

            if (HasExplicitShortName(attribute))
            {
                var namedAttribute = (NamedArgumentAttribute)attribute;
                return namedAttribute.ShortName.Length == 0 ? null : namedAttribute.ShortName;
            }

            var longName = GetLongName(attribute, setAttribute, member);
            
            var shortName = longName.Substring(0, 1);

            if (setAttribute.NameGenerationFlags.HasFlag(ArgumentNameGenerationFlags.PreferLowerCaseForShortNames))
            {
                shortName = shortName.ToLower();
            }

            return shortName;
        }

        private static object GetDefaultValue(ArgumentBaseAttribute attribute, IMutableMemberInfo member, object defaultFieldValue)
        {
            object value;
            if (attribute.DynamicDefaultValue)
            {
                value = defaultFieldValue;
            }
            else if (attribute.ExplicitDefaultValue)
            {
                value = attribute.DefaultValue;
            }
            else
            {
                value = member.MemberType.GetDefaultValue();
            }

            // Validate the value's type.
            if ((value != null) && !member.MemberType.GetTypeInfo().IsAssignableFrom(value.GetType()))
            {
                // See if it's implicitly convertible.
                if (!member.MemberType.IsImplicitlyConvertibleFrom(value))
                {
                    throw new InvalidArgumentSetException(member, string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.DefaultValueIsOfWrongType,
                        member.MemberInfo.Name,
                        value.GetType().Name,
                        member.MemberType.Name));
                }
            }

            return value;
        }

        private static bool HasExplicitShortName(ArgumentBaseAttribute attribute)
        {
            var argAttrib = attribute as NamedArgumentAttribute;
            return argAttrib?.ShortName != null;
        }

        private static string CreateCommandLine(IEnumerable<string> arguments) =>
            string.Join(" ", arguments.Select(StringUtilities.QuoteIfNeeded));

        private static bool IsObjectPresent<T>(T value) =>
            typeof(T).GetTypeInfo().IsValueType ||
            (object)value != null;

        private bool IsEmptyStringValid() =>
            ArgumentType.TryParse(_argumentParseContext, string.Empty, out object parsedEmptyString) &&
            TryValidateValue(
                parsedEmptyString,
                new ArgumentValidationContext(_argumentParseContext.FileSystemReader),
                reportInvalidValue: false);

        private string Format(IObjectFormatter type, object value)
        {
            var formattedValue = type.Format(value);

            if (_isPositional)
            {
                return formattedValue;
            }

            return string.Concat(
                _setAttribute.NamedArgumentPrefixes.FirstOrDefault() ?? string.Empty,
                LongName,
                _setAttribute.ArgumentValueSeparators.FirstOrDefault(),
                formattedValue);
        }

        private bool TryValidateValue(object value, ArgumentValidationContext validationContext, bool reportInvalidValue = true) =>
            _validationAttributes.All(attrib =>
            {
                if (attrib.TryValidate(validationContext, value, out string reason))
                {
                    return true;
                }

                if (reportInvalidValue)
                {
                    ReportBadArgumentValue(_valueType.Format(value), reason);
                }

                return false;
            });

        private bool TrySetValue(object containingObject, object value, string valueString = null)
        {
            //
            // Try to set the value.  If the member is a property, it's
            // possible that there's a non-default implementation of its
            // 'set' method that further validates values.  Watch out for
            // that and surface any reported errors.
            //

            try
            {
                Member.SetValue(containingObject, value);
                return true;
            }
            catch (ArgumentException ex)
            {
                ReportBadArgumentValue(valueString ?? value.ToString(), ex);
                return false;
            }
        }

        private bool TryCreateCollection(ICollectionArgumentType argType, ArrayList values, out object collection)
        {
            try
            {
                collection = argType.ToCollection(values);
                return true;
            }
            catch (ArgumentException ex)
            {
                ReportBadArgumentValue(string.Join(", ", values.ToArray().Select(v => v.ToString())), ex);

                collection = null;
                return false;
            }
        }

        private bool ParseValue(string stringData, out object value)
        {
            if (_valueType.TryParse(_argumentParseContext, stringData, out value))
            {
                return true;
            }

            ReportBadArgumentValue(stringData);

            value = null;
            return false;
        }

        private void ReportMissingRequiredArgument()
        {
            if (_isPositional)
            {
                ReportLine(Strings.MissingRequiredPositionalArgument, LongName);
            }
            else
            {
                ReportLine(
                    Strings.MissingRequiredNamedArgument,
                    _setAttribute.NamedArgumentPrefixes.FirstOrDefault() ?? string.Empty,
                    LongName);
            }
        }

        private void ReportDuplicateArgumentValue(string value) =>
        ReportLine(Strings.DuplicateArgument, LongName, value);

        private void ReportConflictingArgument(string value, Argument conflictingArg) =>
            ReportLine(Strings.ConflictingArgument, LongName, value, conflictingArg.LongName);

        private void ReportBadArgumentValue(string value, ArgumentException exception) =>
            ReportBadArgumentValue(value, exception.Message);

        private void ReportBadArgumentValue(string value, string message) =>
            ReportLine(Strings.BadArgumentValueWithReason, value, LongName, message);

        private void ReportBadArgumentValue(string value) =>
            ReportLine(Strings.BadArgumentValue, value, LongName);

        private void ReportLine(string message, params object[] args)
        {
            Debug.Assert(_reporter != null);
            _reporter(string.Format(CultureInfo.CurrentCulture, message + Environment.NewLine, args));
        }
    }
}
