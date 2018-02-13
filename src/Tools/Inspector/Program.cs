using NClap.Metadata;

namespace NClap.Inspector
{
    class Program
    {
        public static int Main(string[] args)
        {
            if (!CommandLineParser.TryParse(args, out ProgramArguments parsedArgs))
            {
                return 1;
            }

            if (parsedArgs.Execute() != CommandResult.Success)
            {
                return 1;
            }

            return 0;
        }
    }
}
