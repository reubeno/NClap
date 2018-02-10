using System;
using NClap.Metadata;

namespace NClap.TestApp
{
    class LogoCommand : SynchronousCommand
    {
        public override CommandResult Execute()
        {
            Console.WriteLine(@"Logo:");
            Console.WriteLine(@"---------------------------");
            Console.Write(CommandLineParser.GetLogo());
            Console.WriteLine(@"---------------------------");

            return CommandResult.Success;
        }
    }
}
