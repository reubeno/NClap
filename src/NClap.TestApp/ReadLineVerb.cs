using System;
using NClap.ConsoleInput;
using NClap.Metadata;

namespace NClap.TestApp
{
    class ReadLineVerb : SynchronousVerb
    {
        public override VerbResult Execute()
        {
            Console.WriteLine("Reading input line...");

            var line = ConsoleUtilities.ReadLine();

            Console.WriteLine($"Read: [{line}]");

            return VerbResult.Success;
        }
    }
}
