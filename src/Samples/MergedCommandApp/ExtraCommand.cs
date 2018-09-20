using System;
using NClap.Metadata;

namespace MergedCommandApp
{
    internal class ExtraCommand : SynchronousCommand
    {
        public override CommandResult Execute()
        {
            Console.WriteLine("Extra!");
            return CommandResult.Success;
        }
    }
}
