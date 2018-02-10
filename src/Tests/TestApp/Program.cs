using System;
using NClap.Help;

namespace NClap.TestApp
{
    class Program
    {
        private static int Main(string[] args)
        {
            var options = new CommandLineParserOptions
            {
                HelpOptions = new ArgumentSetHelpOptions()
                    .With()
                    .TwoColumnLayout()
            };

            if (!CommandLineParser.TryParse(args, options, out ProgramArguments programArgs))
            {
                return -1;
            }

            var result = programArgs.Command.Execute();
            Console.WriteLine($@"Result: {result}");

            return 0;
        }
    }
}
