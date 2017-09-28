using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NClap.ConsoleInput;
using NClap.Utilities;

namespace NClap.Metadata
{
    /// <summary>
    /// Static class useful for configuring the behavior of help commands.
    /// </summary>
    public static class HelpCommand
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
    /// Command for display help about the commands available.
    /// </summary>
    /// <typeparam name="TCommandType">The command type.</typeparam>
    internal class HelpCommand<TCommandType> : SynchronousCommand
        where TCommandType : struct
    {
        /// <summary>
        /// Optionally specifies the command to retrieve detailed help information
        /// for.
        /// </summary>
        [PositionalArgument(ArgumentFlags.AtMostOnce, Description = "Command to get detailed help information for.")]
        public TCommandType? Command { get; set; }

        /// <summary>
        /// Options for displaying help.
        /// </summary>
        [NamedArgument(ArgumentFlags.AtMostOnce, Description = "Options for displaying help.")]
        public HelpOptions Options { get; set; }

        /// <summary>
        /// Displays help about the available commands.
        /// </summary>
        public override CommandResult Execute()
        {
            var outputHandler = HelpCommand.OutputHandler ?? BasicConsoleInputAndOutput.Default.Write;

            if (Command.HasValue)
            {
                DisplayCommandHelp(outputHandler, Command.Value);
            }
            else
            {
                DisplayGeneralHelp(outputHandler);
            }

            return CommandResult.Success;
        }

        [SuppressMessage("Design", "CC0031:Check for null before calling a delegate")]
        private static void DisplayGeneralHelp(Action<ColoredMultistring> outputHandler)
        {
            Debug.Assert(outputHandler != null);

            // TODO CASE: need to dynamically choose case sensitivity or not.
            var commandNames = typeof(TCommandType).GetTypeInfo().GetEnumValues()
                .Cast<TCommandType>()
                .OrderBy(type => type.ToString(), StringComparer.CurrentCultureIgnoreCase);

            var commandNameMaxLen = commandNames.Max(name => name.ToString().Length);

            var commandSummary = string.Concat(commandNames.Select(commandType =>
            {
                var desc = GetDescription(commandType);

                var formatString = "  {0,-" + commandNameMaxLen.ToString(CultureInfo.InvariantCulture) + "}{1}\n";
                return string.Format(
                    CultureInfo.CurrentCulture,
                    formatString,
                    commandType,
                    desc != null ? " - " + desc : string.Empty);
            }));

            outputHandler(ColoredMultistring.FromString(string.Format(
                CultureInfo.CurrentCulture,
                Strings.ValidCommandsHeader,
                commandSummary)));
        }

        [SuppressMessage("Design", "CC0031:Check for null before calling a delegate")]
        private static void DisplayCommandHelp(Action<ColoredMultistring> outputHandler, TCommandType command)
        {
            Debug.Assert(outputHandler != null);

            var attrib = GetCommandAttribute(command);
            var implementingType = attrib.GetImplementingType(typeof(TCommandType));
            if (implementingType == null)
            {
                outputHandler(ColoredMultistring.FromString(Strings.NoHelpAvailable + Environment.NewLine));
                return;
            }

            var usageInfo = CommandLineParser.GetUsageInfo(implementingType, null, null, command.ToString(), HelpCommand.DefaultUsageInfoOptions);

            outputHandler(usageInfo);
        }

        private static string GetDescription<T>(T value) where T : struct
        {
            var attrib = GetCommandAttribute(value);
            var desc = attrib?.Description;
            return !string.IsNullOrEmpty(desc) ? desc : null;
        }

        private static CommandAttribute GetCommandAttribute<T>(T value) =>
            typeof(T).GetTypeInfo()
                     .GetField(value.ToString())
                     .GetSingleAttribute<CommandAttribute>(inherit: false);
    }
}
