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
        private CommandLineParserOptions _options;
        private ArgumentSetAttribute _argSetAttrib;

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="options">Parser options.</param>
        /// <param name="argSetAttrib">Argument set attribute.</param>
        public HelpCommand(CommandLineParserOptions options, ArgumentSetAttribute argSetAttrib)
        {
            _options = options?.DeepClone() ?? new CommandLineParserOptions();
            _argSetAttrib = argSetAttrib;
        }

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

        private void DisplayGeneralHelp(Action<ColoredMultistring> outputHandler)
        {
            if (outputHandler == null) return;

            var info = CommandLineParser.GetUsageInfo(CreateArgSet(), HelpCommand.DefaultHelpOptions, null);

            outputHandler(info);
        }

        private void DisplayCommandHelp(Action<ColoredMultistring> outputHandler, IEnumerable<string> tokens)
        {
            if (outputHandler == null) return;

            var groupOptions = new CommandGroupOptions
            {
                ServiceConfigurer = _options.ServiceConfigurer
            };

            var group = new CommandGroup<TCommandType>(groupOptions);
            var parser = new ArgumentSetParser(CreateArgSet(), _options.With().Quiet());

            parser.ParseTokens(tokens, group);

            var info = CommandLineParser.GetUsageInfo(
                parser.ArgumentSet,
                HelpCommand.DefaultHelpOptions,
                group);

            outputHandler(info);
        }

        private ArgumentSetDefinition CreateArgSet() =>
            AttributeBasedArgumentDefinitionFactory.CreateArgumentSet(typeof(CommandGroup<TCommandType>), _argSetAttrib);
    }
}
