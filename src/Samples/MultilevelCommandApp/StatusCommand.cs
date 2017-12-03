using System;
using NClap.Metadata;

namespace MultilevelCommandApp
{
    internal class StatusCommand : CommandBase
    {
        public override CommandResult Execute()
        {
            Console.WriteLine($"Status.");
            return CommandResult.Success;
        }
    }
}