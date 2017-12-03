using System;
using System.Collections.Generic;
using NClap.ConsoleInput;
using NClap.Parser;
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
        public static UsageInfoOptions DefaultUsageInfoOptions { get; set; } =
            UsageInfoOptions.Default & ~UsageInfoOptions.IncludeLogo;

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
        [PositionalArgument(ArgumentFlags.RestOfLine, Position = 0)]
        public string[] Arguments { get; set; }

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

            if (Arguments != null && Arguments.Length > 0)
            {
                DisplayCommandHelp(outputHandler, Arguments);
            }
            else
            {
                DisplayGeneralHelp(outputHandler);
            }

            return CommandResult.Success;
        }

        private static void DisplayGeneralHelp(Action<ColoredMultistring> outputHandler)
        {
            if (outputHandler == null) return;

            var info = CommandLineParser.GetUsageInfo(
                typeof(CommandGroup<TCommandType>),
                null, // defaultValues
                null, // columns
                string.Empty, // commandName
                HelpCommand.DefaultUsageInfoOptions);

            outputHandler(info);
        }

        private static void DisplayCommandHelp(Action<ColoredMultistring> outputHandler, IEnumerable<string> tokens)
        {
            if (outputHandler == null) return;

            var group = new CommandGroup<TCommandType>();
            var parser = new ArgumentSetParser(
                ReflectionBasedParser.CreateArgumentSet(group.GetType()),
                new CommandLineParserOptions());

            parser.ParseTokens(tokens, group);

            var info = CommandLineParser.GetUsageInfo(
                parser.ArgumentSet,
                null, // columns
                string.Empty, // commandName
                HelpCommand.DefaultUsageInfoOptions,
                group);

            outputHandler(info);
        }
    }
}
