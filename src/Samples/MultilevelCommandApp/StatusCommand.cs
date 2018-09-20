using Microsoft.Extensions.Logging;
using NClap.Metadata;

namespace MultilevelCommandApp
{
    internal class StatusCommand : CommandBase
    {
        private readonly ILogger logger;

        public StatusCommand(ILogger logger)
        {
            this.logger = logger;
        }

        public override CommandResult Execute()
        {
            logger.LogWarning($"Status.");
            return CommandResult.Success;
        }
    }
}