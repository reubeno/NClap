using System;

namespace NClap.TestApp
{
    class Program
    {
        private static int Main(string[] args)
        {
            if (!CommandLineParser.TryParse(args, out ProgramArguments programArgs))
            {
                return -1;
            }

            var result = programArgs.Command.Execute();
            Console.WriteLine($"Result: {result}");

            return 0;
        }
    }
}
