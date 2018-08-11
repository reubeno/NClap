using System;
using NClap.Metadata;

namespace NClap.Inspector
{
    class CompleteLineCommand : SynchronousCommand
    {
        private readonly ProgramArguments _programArgs;

        public CompleteLineCommand(ProgramArguments programArgs)
        {
            _programArgs = programArgs;
        }

        [NamedArgument(ArgumentFlags.Optional, LongName = "SkipFirst",
            Description = "Skip first token?",
            DefaultValue = true)]
        public bool SkipFirstToken { get; set; }

        [NamedArgument(ArgumentFlags.Required, LongName = "Cursor",
            Description = "0-based index of cursor")]
        public int CursorIndex { get; set; }

        [NamedArgument(ArgumentFlags.Required, LongName = "CommandLine",
            Description = "Command line")]
        public string CommandLine { get; set; }

        public override CommandResult Execute()
        {
            if (_programArgs.Verbose)
            {
                Console.WriteLine($"Completing with cursor={CursorIndex} of command line: [{CommandLine}]");
            }

            foreach (var completion in CommandLineParser.GetCompletions(
                _programArgs.LoadedType,
                CommandLine,
                CursorIndex,
                tokensToSkip: 1,
                options: null))
            {
                Console.WriteLine(completion);
            }

            return CommandResult.Success;
        }
    }
}
