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

            var keyBindingSet = ConsoleKeyBindingSet.CreateDefaultSet();
            keyBindingSet.Bind('c', ConsoleModifiers.Control, ConsoleInputOperation.Abort);

            ++_count;

            var parameters = new LoopInputOutputParameters
            {
                Prompt = new ColoredString($"Loop{new string('>', _count)} ", ConsoleColor.Cyan),
                KeyBindingSet = keyBindingSet,
                EndOfLineCommentCharacter = '#'
            };

            new Loop(typeof(MainCommandType), parameters).Execute();

            --_count;

            Console.WriteLine("Exited loop.");

            return CommandResult.Success;
        }
    }
}
