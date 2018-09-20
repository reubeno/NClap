using System;
using NClap;

namespace MergedCommandApp
{
    class Program
    {
        static int Main(string[] args)
        {
            if (!CommandLineParser.TryParse(args, out ProgramArguments progArgs))
            {
                return 1;
            }

            var result = progArgs.Command.Execute();
            return result == NClap.Metadata.CommandResult.Success ? 0 : 1;
        }
    }
}
