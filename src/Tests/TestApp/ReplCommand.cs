using System;
using NClap.ConsoleInput;
using NClap.Metadata;
using NClap.Repl;
using NClap.Utilities;

namespace NClap.TestApp
{
    [ArgumentSet(Style = ArgumentSetStyle.PowerShell)]
    class ReplCommand : SynchronousCommand
    {
        [NamedArgument(ArgumentFlags.Optional)]
        public LogLevel ReplLevel { get; set; }

        private static int _count = 0;

        public override CommandResult Execute()
        {
            Console.WriteLine("Entering loop.");

            var options = new LoopOptions
            {
                EndOfLineCommentCharacter = '#'
            };

            var keyBindingSet = ConsoleKeyBindingSet.CreateDefaultSet();
            keyBindingSet.Bind('c', ConsoleModifiers.Control, ConsoleInputOperation.Abort);

            ++_count;

            var parameters = new LoopInputOutputParameters
            {
                Prompt = new ColoredString($"Loop{new string('>', _count)} ", ConsoleColor.Cyan),
                KeyBindingSet = keyBindingSet
            };

            new Loop<MainCommandType>(parameters, options).Execute();

            --_count;

            Console.WriteLine("Exited loop.");

            return CommandResult.Success;
        }
    }
}
