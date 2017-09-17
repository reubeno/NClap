using System;
using System.Threading;
using NClap.Parser;

namespace NClap.TestApp
{
    class Program
    {
        private static int Main(string[] args)
        {
            var programArgs = new ProgramArguments();

            if (!CommandLineParser.ParseWithUsage(args, programArgs))
            {
                return -1;
            }

            if (programArgs.Verb?.HasSelection ?? false)
            {
                Console.WriteLine($"Executing verb {programArgs.Verb.SelectedVerbType.Value}");
                var result = programArgs.Verb.SelectedVerb.ExecuteAsync(CancellationToken.None).Result;
                Console.WriteLine($"Result: {result}");
            }

            return 0;
        }
    }
}
