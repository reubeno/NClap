using System;
using NClap.ConsoleInput;
using NClap.Metadata;
using NClap.Repl;
using NClap.Utilities;

namespace NClap.TestApp
{
    enum ReplVerbType
    {
        [HelpVerb(HelpText = "Displays verb help")]
        Help,

        [Verb(typeof(CliHelp))]
        CliHelp,

        [Verb(typeof(ReadLineVerb))]
        ReadLine,

        [Verb(typeof(ExitVerb), HelpText = "Exits the loop")]
        Exit
    }

    class ReplVerb : SynchronousVerb
    {
        public override VerbResult Execute()
        {
            Console.WriteLine("Entering loop.");

            var options = new LoopOptions
            {
                EndOfLineCommentCharacter = '#'
            };

            var keyBindingSet = ConsoleKeyBindingSet.CreateDefaultSet();
            keyBindingSet.Bind('c', ConsoleModifiers.Control, ConsoleInputOperation.Abort);

            var parameters = new LoopInputOutputParameters
            {
                Prompt = new ColoredString("Loop> ", ConsoleColor.Cyan),
                KeyBindingSet = keyBindingSet
            };

            Loop<ReplVerbType>.Execute(parameters, options);

            Console.WriteLine("Exited loop.");

            return VerbResult.Success;
        }
    }
}
