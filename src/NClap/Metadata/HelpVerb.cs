using System;
using System.Globalization;
using System.Linq;
using NClap.ConsoleInput;
using NClap.Parser;
using NClap.Utilities;

namespace NClap.Metadata
{
    /// <summary>
    /// Static class useful for configuring the behavior of help verbs.
    /// </summary>
    public static class HelpVerb
    {
        /// <summary>
        /// The default options to use for generate usage info.
        /// </summary>
        public static UsageInfoOptions DefaultUsageInfoOptions { get; set; } =
            UsageInfoOptions.Default | UsageInfoOptions.UseColor;

        /// <summary>
        /// The output handler function for this class.
        /// </summary>
        public static Action<ColoredMultistring> OutputHandler { get; set; }
    }

    /// <summary>
    /// Verb for display help about the verbs available.
    /// </summary>
    /// <typeparam name="TVerbType">The verb type.</typeparam>
    internal class HelpVerb<TVerbType> : IVerb
        where TVerbType : struct
    {
        /// <summary>
        /// Optionally specifies the verb to retrieve detailed help information
        /// for.
        /// </summary>
        [PositionalArgument(ArgumentFlags.AtMostOnce, HelpText = "Verb to get detailed help information for.")]
        public TVerbType? Verb { get; set; }

        /// <summary>
        /// Options for displaying help.
        /// </summary>
        [NamedArgument(ArgumentFlags.AtMostOnce, HelpText = "Options for displaying help.")]
        public HelpOptions Options { get; set; }

        /// <summary>
        /// Displays help about the available verbs.
        /// </summary>
        /// <param name="context"></param>
        public void Execute(object context)
        {
            var outputHandler = HelpVerb.OutputHandler ?? BasicConsoleInputAndOutput.Default.Write;

            if (Verb.HasValue)
            {
                DisplayVerbHelp(outputHandler, Verb.Value);
            }
            else
            {
                DisplayGeneralHelp(outputHandler);
            }
        }

        private static void DisplayGeneralHelp(Action<ColoredMultistring> outputHandler)
        {
            var verbNames = typeof(TVerbType).GetEnumValues()
                .Cast<TVerbType>()
                .OrderBy(type => type.ToString(), StringComparer.CurrentCultureIgnoreCase);

            var verbNameMaxLen = verbNames.Max(name => name.ToString().Length);

            var verbSummary = string.Concat(verbNames.Select(verbType =>
            {
                var helpText = GetHelpText(verbType);

                var formatString = "  {0,-" + verbNameMaxLen.ToString(CultureInfo.InvariantCulture) + "}{1}\n";
                return string.Format(
                    CultureInfo.CurrentCulture,
                    formatString,
                    verbType,
                    helpText != null ? " - " + helpText : string.Empty);
            }));

            outputHandler(string.Format(CultureInfo.CurrentCulture, Strings.ValidVerbsHeader, verbSummary));
        }

        private static void DisplayVerbHelp(Action<ColoredMultistring> outputHandler, TVerbType verb)
        {
            var attrib = GetVerbAttribute(verb);
            var implementingType = attrib.GetImplementingType(typeof(TVerbType));
            if (implementingType == null)
            {
                outputHandler(Strings.NoHelpAvailable + Environment.NewLine);
                return;
            }

            var usageInfo = CommandLineParser.GetUsageInfo(implementingType, null, null, verb.ToString(), HelpVerb.DefaultUsageInfoOptions);

            /*
            if (!string.IsNullOrEmpty(attrib.HelpText))
            {
                usageInfo = attrib.HelpText + "\n\n" + usageInfo;
            }
            */

            outputHandler(usageInfo);
        }

        private static string GetHelpText<T>(T value) where T : struct
        {
            var attrib = GetVerbAttribute(value);
            var helpText = attrib?.HelpText;
            return !string.IsNullOrEmpty(helpText) ? helpText : null;
        }

        private static VerbAttribute GetVerbAttribute<T>(T value) =>
            typeof(T).GetField(value.ToString())
                     .GetCustomAttributes(typeof(VerbAttribute), false)
                     .Cast<VerbAttribute>()
                     .SingleOrDefault();
    }
}
