using System.Linq;
using NClap.Metadata;
using NClap.Types;

namespace NClap.Parser
{
    /// <summary>
    /// Describes help information for a command-line argument.
    /// </summary>
    internal struct ArgumentUsageInfo
    {
        /// <summary>
        /// Constructor that forms the info from the argument's metadata.
        /// </summary>
        /// <param name="arg">Argument metadata.</param>
        public ArgumentUsageInfo(Argument arg) : this(
            syntax: arg.GetSyntaxHelp(detailed: false),
            detailedSyntax: arg.GetSyntaxHelp(detailed: true),
            description: arg.Attribute.HelpText,
            required: arg.IsRequired,
            shortName: arg.ShortName,
            defaultValue: TryGetDefaultValueString(arg, onlyReturnExplicitDefaults: true),
            argType: arg.ArgumentType)
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
        public ArgumentUsageInfo(
            string syntax,
            string description,
            bool required,
            string shortName = null,
            string defaultValue = null,
            string detailedSyntax = null,
            IArgumentType argType = null)
        {
            Syntax = syntax;
            DetailedSyntax = detailedSyntax ?? syntax;
            Description = description;
            Required = required;
            ShortName = shortName;
            DefaultValue = defaultValue;
            ArgumentType = argType;
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
        private static string TryGetDefaultValueString(Argument arg, bool onlyReturnExplicitDefaults = false)
        {
            // Firstly, if the argument is required, then there's no need to
            // indicate any default value.
            if (arg.IsRequired)
            {
                return null;
            }

            // Next, go check for the actual *effective* default value for the
            // argument; we may still receive back a value here even if one
            // wasn't explicitly declared, as we will have consulted with the
            // argument's type to determine its default value.
            if (onlyReturnExplicitDefaults && !arg.HasDefaultValue)
            {
                return null;
            }

            // If the default value is null, then that's not useful to show the
            // user.
            var defaultValue = arg.EffectiveDefaultValue;
            if (defaultValue == null)
            {
                return null;
            }

            // Special case: if the argument type is bool, then the argument
            // will be like a switch, and that's typically assumed to be false
            // if not present.  So if the default value is indeed 'false', then
            // don't bother displaying it; but if it's 'true', then it's
            // important to indicate that.
            if ((defaultValue is bool) && !((bool)defaultValue))
            {
                return null;
            }

            // Special case: if the argument type is string, then it's safe
            // to assume that its default value is an empty string.
            if ((defaultValue is string stringDefaultValue) &&
                string.IsNullOrEmpty(stringDefaultValue))
            {
                return null;
            }

            // At this point, it's probably important to display the default
            // value.
            var formattedArg = arg.Format(defaultValue, suppressArgNames: true).ToList();
            if (formattedArg.Count == 0)
            {
                return null;
            }

            return string.Join(" ", formattedArg);
        }
    }
}
