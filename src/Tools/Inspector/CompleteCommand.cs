using System;
using System.Collections.Generic;
using System.Linq;
using NClap.Metadata;

namespace NClap.Inspector
{
    class CompleteCommand : SynchronousCommand
    {
        private readonly ProgramArguments _programArgs;

        public CompleteCommand(ProgramArguments programArgs)
        {
            _programArgs = programArgs;
        }

        [NamedArgument(ArgumentFlags.Required, LongName = "ArgIndex",
            Description = "0-based index of argument to complete")]
        public int IndexOfArgToComplete { get; set; }

        [NamedArgument(ArgumentFlags.RestOfLine, LongName = "Args",
            Description = "Command-line tokens")]
        public List<string> Arguments { get; set; } = new List<string>();

        public override CommandResult Execute()
        {
            if (_programArgs.Verbose)
            {
                Console.WriteLine($"Completing token {IndexOfArgToComplete} of args: [{string.Join(" ", Arguments.Select(a => "\"" + a + "\""))}]");
            }

            // Swallow bogus requests.
            if (IndexOfArgToComplete > Arguments.Count)
            {
                return CommandResult.Success;
            }

            foreach (var completion in CommandLineParser.GetCompletions(_programArgs.LoadedType, Arguments, IndexOfArgToComplete))
            {
                Console.WriteLine(completion);
            }

            return CommandResult.Success;
        }
    }
}
