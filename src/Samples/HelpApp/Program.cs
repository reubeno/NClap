using System;
using NClap;
using NClap.Help;

namespace HelpApp
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Parsing...");

            // Set up help options the way we want them.
            var helpOptions = new ArgumentSetHelpOptions()
                .With()
                .BlankLinesBetweenArguments(1)
                .ShortNames(ArgumentShortNameHelpMode.IncludeWithLongName)
                .DefaultValues(ArgumentDefaultValueHelpMode.PrependToDescription)
                .TwoColumnLayout();

            // Wrap help options in general parsing options.
            var options = new CommandLineParserOptions { HelpOptions = helpOptions };

            // Try to parse.
            if (!CommandLineParser.TryParse(args, options, out ProgramArguments programArgs))
            {
                return 1;
            }

            Console.WriteLine("Successfully parsed.");

            return 0;
        }
    }
}
