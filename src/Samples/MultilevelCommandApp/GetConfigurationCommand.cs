using System;
using NClap.Metadata;

namespace MultilevelCommandApp
{
    internal class GetConfigurationCommand : CommandBase
    {
        private readonly ConfigurationCommand configCommand;

        public GetConfigurationCommand(ConfigurationCommand configCommand)
        {
            this.configCommand = configCommand;
        }

        public override CommandResult Execute()
        {
            Console.WriteLine($"Getting {configCommand.Policy} configuration");
            return CommandResult.Success;
        }
    }
}