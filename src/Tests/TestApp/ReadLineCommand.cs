using System;
using NClap.ConsoleInput;
using NClap.Metadata;

namespace NClap.TestApp
{
    class ReadLineCommand : SynchronousCommand
    {
        [NamedArgument(ArgumentFlags.Optional, DefaultValue = true, Description = "Echo input back to screen.")]
        bool Echo { get; set; }

        public override CommandResult Execute()
        {
            Console.WriteLine("Reading input line...");

            var line = ConsoleUtilities.ReadLine();

            if (Echo)
            {
                Console.WriteLine($"Read: [{line}]");
            }

            return CommandResult.Success;
        }
    }
}
