using Microsoft.Extensions.Logging;
using NClap.Metadata;

namespace MultilevelCommandApp
{
    internal class SetConfigurationCommand : CommandBase
    {
        private readonly ILogger logger;
        private readonly ConfigurationCommand configCommand;

        public SetConfigurationCommand(ILogger logger, ConfigurationCommand configCommand)
        {
            this.logger = logger;
            this.configCommand = configCommand;
        }

        public override CommandResult Execute()
        {
            logger.LogInformation($"Setting {configCommand.Policy} configuration");
            return CommandResult.Success;
        }
    }
}