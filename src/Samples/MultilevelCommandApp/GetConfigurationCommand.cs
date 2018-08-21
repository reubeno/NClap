using Microsoft.Extensions.Logging;
using NClap.Metadata;

namespace MultilevelCommandApp
{
    internal class GetConfigurationCommand : CommandBase
    {
        private readonly ILogger logger;
        private readonly ConfigurationCommand configCommand;

        public GetConfigurationCommand(ILogger logger, ConfigurationCommand configCommand)
        {
            this.logger = logger;
            this.configCommand = configCommand;
        }

        public override CommandResult Execute()
        {
            logger.LogInformation($"Getting {configCommand.Policy} configuration");
            return CommandResult.Success;
        }
    }
}