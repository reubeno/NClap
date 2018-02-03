using System;
using System.Linq;
using System.Reflection;
using NClap.Metadata;
using NClap.Parser;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Help
{
    /// <summary>
    /// Describes help information for a command-line argument.
    /// </summary>
    internal class ArgumentUsageInfo
    {
        /// <summary>
        /// Constructor that forms the info from the argument's metadata.
        /// </summary>
        /// <param name="arg">Argument metadata.</param>
        /// <param name="currentValue">Current value.</param>
        public ArgumentUsageInfo(ArgumentDefinition arg, object currentValue = null)
        {
            Arg = arg;
            CurrentValue = currentValue;
        }

        /// <summary>
        /// Argument.
        /// </summary>
        public ArgumentDefinition Arg { get; }

        /// <summary>
        /// Current value.
        /// </summary>
        public object CurrentValue { get; }

        /// <summary>
        /// Help information.
        /// </summary>
        public string Description => Arg.Attribute.Description;

        /// <summary>
        /// True if the argument is required; false otherwise.
        /// </summary>
        public bool Required => Arg.IsRequired;

        /// <summary>
        /// True if the argument is positional; false otherwise.
        /// </summary>
        public bool IsPositional => Arg.IsPositional;

        /// <summary>
        /// Optionally indicates the argument's long name.
        /// </summary>
        public string LongName => Arg.LongName;

        /// <summary>
        /// Optionally indicates the argument's short name.
        /// </summary>
        public string ShortName => Arg.ShortName;

        /// <summary>
        /// Optionally indicates the argument's default value. Note that there
        /// may be a default value that's not indicated here, particularly if
        /// it was a defaulted default value.
        /// </summary>
        public string DefaultValue => TryGetDefaultValueString(Arg, onlyReturnExplicitDefaults: true);

        /// <summary>
        /// Type of the argument, if known; null otherwise.
        /// </summary>
        public IArgumentType ArgumentType => Arg.ArgumentType;

        /// <summary>
        /// Gets the syntax for this argument.
        /// </summary>
        /// <param name="options">Options for generating the syntax.</param>
        /// <param name="enumsDocumentedInline">true if enums will be documented
        /// inline for this argument; false otherwise.</param>
        /// <returns>Syntax.</returns>
        public string GetSyntax(ArgumentHelpOptions options, bool enumsDocumentedInline = false)
        {
            var suppressTypeInfo = enumsDocumentedInline;

            if (!options.IncludePositionalArgumentTypes && IsPositional)
            {
                suppressTypeInfo = true;
            }

            if (options.IncludeSyntaxWithArgumentEntry)
            {
                return SimplifyParameterSyntax(Arg.GetSyntaxSummary(detailed: !suppressTypeInfo));
            }
            else if (IsPositional)
            {
                return SimplifyParameterSyntax(Arg.GetSyntaxSummary(detailed: false));
            }
            else
            {
                return Arg.ContainingSet.Attribute.NamedArgumentPrefixes.First() +
                    LongName;
            }
        }

        /// <summary>
        /// Checks if the argument is a selected command.
        /// </summary>
        /// <returns>True if the argument is a selected command; false otherwise.
        /// </returns>
        public bool IsSelectedCommand() =>
            CurrentValue != null &&
            ArgumentType != null &&
            (ArgumentType.Type is ICommand || ArgumentType is CommandGroupArgumentType || IsCommandEnum(CurrentValue));

        /// <summary>
        /// Tries to find the argument's default value.
        /// </summary>
        /// <param name="arg">The argument to retrieve a default value from.</param>
        /// <param name="onlyReturnExplicitDefaults">True to only return
        /// a default if it was explicitly specified; false to report on
        /// the default, even if it was defaulted itself.</param>
        /// <param name="value">On success, receives the default value
        /// for this argument; otherwise, receives null.</param>
        /// <returns>True on success, false otherwise.</returns>
        public static bool TryGetDefaultValue(ArgumentDefinition arg, bool onlyReturnExplicitDefaults, out object value)
        {
            // Firstly, if the argument is required, then there's no need to
            // indicate any default value.
            if (arg.IsRequired)
            {
                value = null;
                return false;
            }

            // Next, go check for the actual *effective* default value for the
            // argument; we may still receive back a value here even if one
            // wasn't explicitly declared, as we will have consulted with the
            // argument's type to determine its default value.
            if (onlyReturnExplicitDefaults && !arg.HasDefaultValue)
            {
                value = null;
                return false;
            }

            // If the default value is null, then that's not useful to show the
            // user.
            var defaultValue = arg.EffectiveDefaultValue;
            if (defaultValue == null)
            {
                value = null;
                return false;
            }

            // Special case: if the argument type is bool, then the argument
            // will be like a switch, and that's typically assumed to be false
            // if not present.  So if the default value is indeed 'false', then
            // don't bother displaying it; but if it's 'true', then it's
            // important to indicate that.
            if ((defaultValue is bool) && !((bool)defaultValue))
            {
                value = null;
                return false;
            }

            // Special case: if the argument type is string, then it's safe
            // to assume that its default value is an empty string.
            if ((defaultValue is string stringDefaultValue) &&
                string.IsNullOrEmpty(stringDefaultValue))
            {
                value = null;
                return false;
            }

            // Okay, we have the value.
            value = defaultValue;
            return true;
        }

        /// <summary>
        /// Tries to construct a string describing the argument's default value.
        /// </summary>
        /// <param name="arg">The argument to retrieve a default value string
        /// from.</param>
        /// <param name="onlyReturnExplicitDefaults">True to only return
        /// a default if it was explicitly specified; false to report on
        /// the default, even if it was defaulted itself.</param>
        /// <returns>If one should be advertised, returns the string version of
        /// the default value for this argument; otherwise, returns null.
        /// </returns>
        public static string TryGetDefaultValueString(ArgumentDefinition arg, bool onlyReturnExplicitDefaults = false)
        {
            // Try to get the default value.
            if (!TryGetDefaultValue(arg, onlyReturnExplicitDefaults, out object defaultValue))
            {
                return null;
            }

            // Now turn the value into a string.
            var formattedArg = arg.Format(defaultValue, suppressArgNames: true).ToList();
            if (formattedArg.Count == 0)
            {
                return null;
            }

            return string.Join(" ", formattedArg);
        }

        /// <summary>
        /// Utility for checking if the given object is an instance of a command
        /// enum type.
        /// </summary>
        /// <param name="value">Object to inspect.</param>
        /// <returns>true if it is an instance of such a type; false otherwise.</returns>
        public static bool IsCommandEnum(object value) => IsCommandEnum(value.GetType());

        /// <summary>
        /// Utility for checking if the given type is a command enum type.
        /// </summary>
        /// <param name="type">Type to inspect.</param>
        /// <returns>true if it is such a type; false otherwise.</returns>
        public static bool IsCommandEnum(Type type)
        {
            if (!type.GetTypeInfo().IsEnum)
            {
                return false;
            }

            // We check if any of the fields on this type have the expected
            // command attribute.
            return type.GetTypeInfo().GetFields().Any(field =>
                field.GetSingleAttribute<CommandAttribute>() != null);
        }

        // We add logic here to trim out a single pair of enclosing
        // square brackets if it's present -- it's just noisy here.
        // The containing section already makes it sufficiently clear
        // whether the parameter is required or optional.
        //
        // TODO: Make this logic more generic, and put it elsewhere.
        private static string SimplifyParameterSyntax(string s) =>
            s.TrimStart('[')
             .TrimEnd(']', '*', '+');
    }
}
