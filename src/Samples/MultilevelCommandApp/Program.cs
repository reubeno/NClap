using System;
using NClap;
using NClap.Metadata;
using NClap.Repl;

namespace MultilevelCommandApp
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Parsing...");

            if (!CommandLineParser.TryParse(args, out ProgramArguments programArgs))
            {
                return 1;
            }

            Console.WriteLine("Successfully parsed; reserialized:");
            Console.WriteLine(string.Join(" ", CommandLineParser.Format(programArgs)));

            Console.WriteLine("Executing...");

            CommandResult result;
            if (programArgs.Command != null)
            {
                result = programArgs.Command.Execute();
            }
            else
            {
                var loop = new Loop(typeof(ProgramArguments.ToplevelCommandType));
                loop.Execute();

                result = CommandResult.Success;
            }

            Console.WriteLine($"Result: {result}");

            return 0;
        }
    }
}
