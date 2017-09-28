using System;

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

            var result = programArgs.Command.Execute();
            Console.WriteLine($"Result: {result}");

            return 0;
        }
    }
}
