using System;
using System.Reflection;

namespace NClap.Completer
{
    class Program
    {
        private readonly ProgramArguments _args;

        public static int Main(string[] args)
        {
            if (!CommandLineParser.TryParse(args, out ProgramArguments parsedArgs))
            {
                return 1;
            }

            return new Program(parsedArgs).Execute();
        }

        private Program(ProgramArguments args)
        {
            _args = args;
        }

        private int Execute()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (sender, args) =>
                Assembly.ReflectionOnlyLoad(args.Name);

            var assembly = Assembly.ReflectionOnlyLoadFrom(_args.AssemblyPath);
            var type = assembly.GetType(_args.TypeName);

            foreach (var completion in CommandLineParser.GetCompletions(type, _args.Arguments, _args.IndexOfArgToComplete))
            {
                Console.WriteLine(completion);
            }

            return 0;
        }
    }
}
