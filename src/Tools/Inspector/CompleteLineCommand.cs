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

        [NamedArgument(ArgumentFlags.Optional, LongName = "Skip",
            Description = "Tokens to skip",
            DefaultValue = 1)]
        public int TokensToSkip { get; set; }

        [NamedArgument(ArgumentFlags.Required, LongName = "Cursor",
            Description = "0-based index of cursor")]
        public int CursorIndex { get; set; }

        [NamedArgument(ArgumentFlags.Required, LongName = "CommandLine",
            Description = "Command line")]
        public string CommandLine { get; set; }

        public override CommandResult Execute()
        {
            var verboseMessage = $"{Guid.NewGuid()}: Completing with skip={TokensToSkip} cursor={CursorIndex} of command line: [{CommandLine}]";
            if (_programArgs.Verbose)
            {
                Console.WriteLine(verboseMessage);
            }

            foreach (var completion in CommandLineParser.GetCompletions(
                _programArgs.LoadedType,
                CommandLine,
                CursorIndex,
                tokensToSkip: this.TokensToSkip,
                options: null))
            {
                Console.WriteLine(completion);
            }

            return CommandResult.Success;
        }
    }
}
