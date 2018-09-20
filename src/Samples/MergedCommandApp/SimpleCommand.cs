using System;
using NClap.Metadata;

namespace MergedCommandApp
{
    internal class SimpleCommand : SynchronousCommand
    {
        public override CommandResult Execute()
        {
            Console.WriteLine("Simple.");
            return CommandResult.Success;
        }
    }
}
