using System.Linq;
using NClap.Metadata;

namespace NClap.Parser
{
    /// <summary>
    /// Describes help information for a command-line argument.
    /// </summary>
    internal struct ArgumentUsageInfo
    {
        /// <summary>
        /// True if the argument is required; false otherwise.
        /// </summary>
        public readonly bool Required;

        /// <summary>
        /// Help information.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Syntax information.
        /// </summary>
        public readonly string Syntax;

        /// <summary>
        /// Optionally indicates the argument's short name.
        /// </summary>
        public readonly string ShortName;

        /// <summary>
        /// Optionally indicates the argument's default value.
        /// </summary>
        public readonly string DefaultValue;

        /// <summary>
        /// Constructor that forms the info from the argument's metadata.
        /// </summary>
        /// <param name="setAttribute">Argument set attribute.</param>
        /// <param name="arg">Argument metadata.</param>
        public ArgumentUsageInfo(ArgumentSetAttribute setAttribute, Argument arg)
        {
            Syntax = arg.GetSyntaxHelp(setAttribute);
            Description = arg.Attribute.HelpText;
            ShortName = arg.ShortName;
            DefaultValue = TryGetDefaultValueString(setAttribute, arg);
            Required = arg.IsRequired;
        }

        /// <summary>
        /// Constructor that directly takes all required info.
        /// </summary>
        /// <param name="syntax">Argument syntax.</param>
        /// <param name="description">Argument description.</param>
        /// <param name="shortName">Argument's short form.</param>
        /// <param name="defaultValue">Argument's default value.</param>
        /// <param name="required">True if the argument is required;
        /// false if it's optional.</param>
        public ArgumentUsageInfo(
            string syntax,
            string description,
            string shortName,
            string defaultValue,
            bool required)
        {
            Syntax = syntax;
            Description = description;
            ShortName = shortName;
            DefaultValue = defaultValue;
            Required = required;
        }

        private static string TryGetDefaultValueString(ArgumentSetAttribute setAttribute, Argument arg)
        {
            var defaultValue = arg.EffectiveDefaultValue;
            return defaultValue == null
                ? null
                : arg.Format(setAttribute, defaultValue).SingleOrDefault();
        }
    }
}
