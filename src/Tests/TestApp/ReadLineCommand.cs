using System;
using NClap.ConsoleInput;
using NClap.Metadata;

namespace NClap.TestApp
{
    class ReadLineCommand : SynchronousCommand
    {
        public override CommandResult Execute()
        {
            Console.WriteLine(@"Reading input line...");

            var line = ConsoleUtilities.ReadLine();

            Console.WriteLine($@"Read: [{line}]");

            return CommandResult.Success;
        }
    }
}
