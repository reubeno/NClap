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
            var helpOptions = new ArgumentSetHelpOptions
            {
                Arguments = new ArgumentHelpOptions
                {
                    BlankLinesBetweenArguments = 1,
                    ShortName = ArgumentShortNameHelpMode.IncludeWithLongName,
                    DefaultValue = ArgumentDefaultValueHelpMode.PrependToDescription,
                    Layout = new TwoColumnArgumentHelpLayout()
                }
            };

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
