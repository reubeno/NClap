using System.Linq;
using System.Reflection;
using NClap.Metadata;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Parser
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
        public ArgumentUsageInfo(ArgumentDefinition arg, object currentValue = null) : this(
            syntax: arg.GetSyntaxSummary(detailed: false),
            detailedSyntax: arg.GetSyntaxSummary(detailed: true),
            description: arg.Attribute.Description,
            required: arg.IsRequired,
            shortName: arg.ShortName,
            defaultValue: TryGetDefaultValueString(arg, onlyReturnExplicitDefaults: true),
            argType: arg.ArgumentType,
            currentValue: currentValue)
        {
        }

        /// <summary>
        /// Constructor that directly takes all required info.
        /// </summary>
        /// <param name="syntax">Argument syntax.</param>
        /// <param name="description">Argument description.</param>
        /// <param name="required">True if the argument is required;
        /// false if it's optional.</param>
        /// <param name="shortName">Argument's short form.</param>
        /// <param name="defaultValue">Argument's default value.</param>
        /// <param name="detailedSyntax">Argument detailed syntax.</param>
        /// <param name="argType">Argument type.</param>
        /// <param name="currentValue">Current value.</param>
        public ArgumentUsageInfo(
            string syntax,
            string description,
            bool required,
            string shortName = null,
            string defaultValue = null,
            string detailedSyntax = null,
            IArgumentType argType = null,
            object currentValue = null)
        {
            Syntax = syntax;
            DetailedSyntax = detailedSyntax ?? syntax;
            Description = description;
            Required = required;
            ShortName = shortName;
            DefaultValue = defaultValue;
            ArgumentType = argType;
            CurrentValue = currentValue;
        }

        /// <summary>
        /// Syntax information.
        /// </summary>
        public string Syntax { get; }

        /// <summary>
        /// Detailed syntax information.
        /// </summary>
        public string DetailedSyntax { get; }

        /// <summary>
        /// Help information.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// True if the argument is required; false otherwise.
        /// </summary>
        public bool Required { get; }

        /// <summary>
        /// Optionally indicates the argument's short name.
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Optionally indicates the argument's default value. Note that there
        /// may be a default value that's not indicated here, particularly if
        /// it was a defaulted default value.
        /// </summary>
        public string DefaultValue { get; }

        /// <summary>
        /// Type of the argument, if known; null otherwise.
        /// </summary>
        public IArgumentType ArgumentType { get; }

        /// <summary>
        /// Current value.
        /// </summary>
        public object CurrentValue { get; }

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

        public static bool IsCommandEnum(object value)
        {
            var ty = value.GetType();

            if (!ty.GetTypeInfo().IsEnum)
            {
                return false;
            }

            var fieldName = ty.GetTypeInfo().GetEnumName(value);
            var field = ty.GetTypeInfo().GetField(fieldName);

            return field.GetSingleAttribute<CommandAttribute>() != null;
        }
    }
}
