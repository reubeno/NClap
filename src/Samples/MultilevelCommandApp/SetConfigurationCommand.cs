using System;
using NClap.Metadata;

namespace MultilevelCommandApp
{
    internal class SetConfigurationCommand : CommandBase
    {
        private readonly ConfigurationCommand configCommand;

        public SetConfigurationCommand(ConfigurationCommand configCommand)
        {
            this.configCommand = configCommand;
        }

        public override CommandResult Execute()
        {
            Console.WriteLine($"Setting {configCommand.Policy} configuration");
            return CommandResult.Success;
        }
    }
}