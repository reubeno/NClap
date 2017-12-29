﻿using System;
using System.Collections.Generic;
using NClap.ConsoleInput;
using NClap.Help;
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
        /// The default options to use for generate help.
        /// </summary>
        public static ArgumentSetHelpOptions DefaultHelpOptions { get; set; } =
            new ArgumentSetHelpOptions
            {
                Logo = new ArgumentMetadataHelpOptions { Include = false },
                Name = string.Empty
            };

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
                HelpCommand.DefaultHelpOptions);

            outputHandler(info);
        }

        private static void DisplayCommandHelp(Action<ColoredMultistring> outputHandler, IEnumerable<string> tokens)
        {
            if (outputHandler == null) return;

            var group = new CommandGroup<TCommandType>();
            var parser = new ArgumentSetParser(
                ReflectionBasedParser.CreateArgumentSet(group.GetType()),
                CommandLineParserOptions.Quiet());

            parser.ParseTokens(tokens, group);

            var info = CommandLineParser.GetUsageInfo(
                parser.ArgumentSet,
                HelpCommand.DefaultHelpOptions,
                group);

            outputHandler(info);
        }
    }
}
