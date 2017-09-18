﻿using System;
using NClap.ConsoleInput;
using NClap.Metadata;
using NClap.Repl;
using NClap.Utilities;

namespace NClap.TestApp
{
    [ArgumentType(DisplayName = "REPL command")]
    enum ReplVerbType
    {
        [HelpVerb(Description = "Displays verb help")]
        Help,

        [Verb(typeof(CliHelp))]
        CliHelp,

        [Verb(typeof(ReadLineVerb))]
        ReadLine,

        [Verb(typeof(ExitVerb), Description = "Exits the loop")]
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

            new Loop<ReplVerbType>(parameters, options).Execute();

            Console.WriteLine("Exited loop.");

            return VerbResult.Success;
        }
    }
}