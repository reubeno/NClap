using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using NClap.Exceptions;
using NClap.Types;

namespace NClap.Metadata
{
    /// <summary>
    /// Abstract base class for logic shared between NamedArgumentAttribute and
    /// PositionalArgumentAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class ArgumentBaseAttribute : Attribute
    {
        private object _defaultValue;
        private string _longName;

        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "[Legacy]")]
        private string[] _conflictingMemberNames = Array.Empty<string>();

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="flags">Specifies the error checking to be done on the
        /// argument.</param>
        protected ArgumentBaseAttribute(ArgumentFlags flags)
        {
            Flags = flags;
        }

        /// <summary>
        /// The error checking to be done on the argument.
        /// </summary>
        public ArgumentFlags Flags { get; internal set; }

        /// <summary>
        /// The long name of the argument; null indicates that the "default"
        /// long name should be used.  The long name for every argument must
        /// be unique.  It is an error to specify a long name of string.Empty.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "[Legacy]")]
        public string LongName
        {
            get
            {
                var value = _longName;

                if ((value != null) && (value.Length == 0))
                {
                    throw new InvalidArgumentSetException(string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.InvalidArgumentLongName));
                }

                return value;
            }

            set => _longName = value;
        }

        /// <summary>
        /// The default value of the argument.
        /// </summary>
        public object DefaultValue
        {
            get => _defaultValue;
            set
            {
                ExplicitDefaultValue = true;
                _defaultValue = value;
            }
        }

        /// <summary>
        /// Returns true if the argument has a dynamic default value.  If the
        /// argument has a dynamic default value, then during parsing, values
        /// already present in the corresponding field in the destination
        /// object will be treated as the "dynamic default" value for this
        /// argument.
        /// </summary>
        public bool DynamicDefaultValue { get; set; }

        /// <summary>
        /// The description of the argument, exposed via help/usage information.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// True indicates that this argument should be hidden from usage help
        /// text; false indicates that it should be included.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// The names of the members that this member conflicts with.  Command-
        /// line arguments will fail to parse if they specify a value for this
        /// member as well as for any of the members referenced by this
        /// property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Needs to be array so it functions as an attribute parameter")]
        public string[] ConflictsWith
        {
            get => _conflictingMemberNames;
            set => _conflictingMemberNames = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Options for parsing numeric arguments.
        /// </summary>
        public NumberOptions NumberOptions { get; set; }

        /// <summary>
        /// True to allow "empty" arguments (e.g. empty strings); false to
        /// consider them invalid.
        /// </summary>
        public bool AllowEmpty { get; set; }

        /// <summary>
        /// Optionally specifies the strings that may be used as element
        /// separators for multiple elements that have been expressed in the
        /// same token.  Only relevant for parsing collection types, and
        /// ignored otherwise.
        /// </summary>
        public string[] ElementSeparators { get; set; } = new[] { "," };

        /// <summary>
        /// Optionally provides a type that implements IStringParser, and which
        /// should be used for parsing strings for this argument instead of the
        /// default IArgumentType class associated with the field/property's
        /// type.
        /// </summary>
        public Type Parser { get; set; }

        /// <summary>
        /// Optionally provides a type that implements IObjectFormatter, and
        /// which should be used for formatting objects for this argument
        /// instead of the default IArgumentType class associated with the
        /// field/property's type.
        /// </summary>
        public Type Formatter { get; set; }

        /// <summary>
        /// Optionally provides a type that implements IStringCompleter,
        /// and which should be used for generating string completions for this
        /// argument instead of the default IArgumentType class associated with
        /// the field/property's type.
        /// </summary>
        public Type Completer { get; set; }

        /// <summary>
        /// Returns true if the argument has an explicit default value.
        /// </summary>
        internal bool ExplicitDefaultValue { get; private set; }

        /// <summary>
        /// Retrieves the IArgumentType type for the provided type.
        /// </summary>
        /// <param name="type">The type to look up.</param>
        /// <returns>The IArgumentType.</returns>
        internal IArgumentType GetArgumentType(Type type)
        {
            var argType = ArgumentType.GetType(type);
            if ((Parser == null) && (Formatter == null) && (Completer == null))
            {
                return argType;
            }

            var parser = InvokeParameterlessConstructorIfPresent<IStringParser>(Parser);
            var formatter = InvokeParameterlessConstructorIfPresent<IObjectFormatter>(Formatter);
            var completer = InvokeParameterlessConstructorIfPresent<IStringCompleter>(Completer);

            return new ArgumentTypeExtension(argType, parser, formatter, completer);
        }

        private static T InvokeParameterlessConstructorIfPresent<T>(Type type) =>
            (T)type?.GetTypeInfo().GetConstructor(
                Array.Empty<Type>())?.Invoke(Array.Empty<object>());
    }
}
