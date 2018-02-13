using System;
using System.Collections.Generic;
using NClap.ConsoleInput;
using NClap.Parser;
using NClap.Utilities;

namespace NClap.Metadata
{
    /// <summary>
    /// Command for display help about the commands available.
    /// </summary>
    /// <typeparam name="TCommandType">The command type.</typeparam>
    internal class HelpCommand<TCommandType> : SynchronousCommand
        where TCommandType : struct
    {
        /// <summary>
        /// Arguments to get help for.
        /// </summary>
        [PositionalArgument(ArgumentFlags.RestOfLine, Position = 0)]
        public string[] Arguments { get; set; }

        /// <summary>
        /// Displays help about the available commands.
        /// </summary>
        /// <returns>Command result.</returns>
        public override CommandResult Execute()
        {
            var outputHandler = HelpCommand.OutputHandler ?? BasicConsole.Default.Write;

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
                CommandGroupType,
                HelpCommand.DefaultHelpOptions);

            outputHandler(info);
        }

        private static void DisplayCommandHelp(Action<ColoredMultistring> outputHandler, IEnumerable<string> tokens)
        {
            if (outputHandler == null) return;

            var group = CreateCommandGroup();
            var parser = new ArgumentSetParser(
                AttributeBasedArgumentDefinitionFactory.CreateArgumentSet(group.GetType()),
                CommandLineParserOptions.Quiet());

            parser.ParseTokens(tokens, group);

            var info = CommandLineParser.GetUsageInfo(
                parser.ArgumentSet,
                HelpCommand.DefaultHelpOptions,
                group);

            outputHandler(info);
        }

        private static object CreateCommandGroup() => new CommandGroup<TCommandType>();

        private static Type CommandGroupType => typeof(CommandGroup<TCommandType>);
    }
}
