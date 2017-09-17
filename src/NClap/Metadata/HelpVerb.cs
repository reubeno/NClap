using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        public static UsageInfoOptions DefaultUsageInfoOptions { get; set; } = UsageInfoOptions.Default;

        /// <summary>
        /// The output handler function for this class.
        /// </summary>
        public static Action<ColoredMultistring> OutputHandler { get; set; }
    }

    /// <summary>
    /// Verb for display help about the verbs available.
    /// </summary>
    /// <typeparam name="TVerbType">The verb type.</typeparam>
    internal class HelpVerb<TVerbType> : SynchronousVerb
        where TVerbType : struct
    {
        /// <summary>
        /// Optionally specifies the verb to retrieve detailed help information
        /// for.
        /// </summary>
        [PositionalArgument(ArgumentFlags.AtMostOnce, Description = "Verb to get detailed help information for.")]
        public TVerbType? Verb { get; set; }

        /// <summary>
        /// Options for displaying help.
        /// </summary>
        [NamedArgument(ArgumentFlags.AtMostOnce, Description = "Options for displaying help.")]
        public HelpOptions Options { get; set; }

        /// <summary>
        /// Displays help about the available verbs.
        /// </summary>
        public override VerbResult Execute()
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

            return VerbResult.Success;
        }

        [SuppressMessage("Design", "CC0031:Check for null before calling a delegate")]
        private static void DisplayGeneralHelp(Action<ColoredMultistring> outputHandler)
        {
            Debug.Assert(outputHandler != null);

            var verbNames = typeof(TVerbType).GetTypeInfo().GetEnumValues()
                .Cast<TVerbType>()
                .OrderBy(type => type.ToString(), StringComparer.CurrentCultureIgnoreCase);

            var verbNameMaxLen = verbNames.Max(name => name.ToString().Length);

            var verbSummary = string.Concat(verbNames.Select(verbType =>
            {
                var desc = GetDescription(verbType);

                var formatString = "  {0,-" + verbNameMaxLen.ToString(CultureInfo.InvariantCulture) + "}{1}\n";
                return string.Format(
                    CultureInfo.CurrentCulture,
                    formatString,
                    verbType,
                    desc != null ? " - " + desc : string.Empty);
            }));

            outputHandler(string.Format(CultureInfo.CurrentCulture, Strings.ValidVerbsHeader, verbSummary));
        }

        [SuppressMessage("Design", "CC0031:Check for null before calling a delegate")]
        private static void DisplayVerbHelp(Action<ColoredMultistring> outputHandler, TVerbType verb)
        {
            Debug.Assert(outputHandler != null);

            var attrib = GetVerbAttribute(verb);
            var implementingType = attrib.GetImplementingType(typeof(TVerbType));
            if (implementingType == null)
            {
                outputHandler(Strings.NoHelpAvailable + Environment.NewLine);
                return;
            }

            var usageInfo = CommandLineParser.GetUsageInfo(implementingType, null, null, verb.ToString(), HelpVerb.DefaultUsageInfoOptions);

            outputHandler(usageInfo);
        }

        private static string GetDescription<T>(T value) where T : struct
        {
            var attrib = GetVerbAttribute(value);
            var desc = attrib?.Description;
            return !string.IsNullOrEmpty(desc) ? desc : null;
        }

        private static VerbAttribute GetVerbAttribute<T>(T value) =>
            typeof(T).GetTypeInfo()
                     .GetField(value.ToString())
                     .GetCustomAttributes(typeof(VerbAttribute), false)
                     .Cast<VerbAttribute>()
                     .SingleOrDefault();
    }
}
