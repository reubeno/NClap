using System;
using System.Collections.Generic;
using System.Linq;
using NClap.Metadata;

namespace NClap.Inspector
{
    class CompleteTokensCommand : SynchronousCommand
    {
        private readonly ProgramArguments _programArgs;

        public CompleteTokensCommand(ProgramArguments programArgs)
        {
            _programArgs = programArgs;
        }

        [NamedArgument(ArgumentFlags.Required, LongName = "TokenIndex",
            Description = "0-based index of token to complete")]
        public int IndexOfTokenToComplete { get; set; }

        [NamedArgument(ArgumentFlags.RestOfLine, LongName = "Tokens",
            Description = "Command-line tokens")]
        public List<string> Tokens { get; set; } = new List<string>();

        public override CommandResult Execute()
        {
            if (_programArgs.Verbose)
            {
                Console.WriteLine($"Completing token {IndexOfTokenToComplete} of args: [{string.Join(" ", Tokens.Select(a => "\"" + a + "\""))}]");
            }

            // Swallow bogus requests.
            if (IndexOfTokenToComplete > Tokens.Count)
            {
                return CommandResult.Success;
            }

            foreach (var completion in CommandLineParser.GetCompletions(_programArgs.LoadedType, Tokens, IndexOfTokenToComplete))
            {
                Console.WriteLine(completion);
            }

            return CommandResult.Success;
        }
    }
}
