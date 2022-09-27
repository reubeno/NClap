using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NClap;
using NClap.Metadata;
using NClap.Repl;

namespace MultilevelCommandApp
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Setting up logging...");

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            var logger = loggerFactory.CreateLogger<Program>();

            Console.WriteLine("Parsing...");

            var options = new CommandLineParserOptions().With()
                .ConfigureServices(s => s.AddSingleton<ILogger>(logger));

            if (!CommandLineParser.TryParse(args, options, out ProgramArguments programArgs))
            {
                return 1;
            }

            Console.WriteLine("Successfully parsed; reserialized:");
            Console.WriteLine("    " + string.Join(" ", CommandLineParser.Format(programArgs)));

            Console.WriteLine("Executing...");

            CommandResult result;
            if (programArgs.Command != null)
            {
                result = programArgs.Command.Execute();
            }
            else
            {
                var loopOptions = new LoopOptions { ParserOptions = options };
                var loop = new Loop(typeof(ProgramArguments.ToplevelCommandType), options: loopOptions);
                loop.Execute();

                result = CommandResult.Success;
            }

            loggerFactory.Dispose();

            Console.WriteLine($"Result: {result}");

            return 0;
        }
    }
}
