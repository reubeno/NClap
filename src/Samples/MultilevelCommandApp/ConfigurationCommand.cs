using System.Threading;
using System.Threading.Tasks;
using NClap.Metadata;

namespace MultilevelCommandApp
{
    enum ConfigurationPolicy
    {
        Permanent,
        Ephemeral
    }

    internal class ConfigurationCommand : ICommand
    {
        internal enum Ty
        {
            [Command(typeof(GetConfigurationCommand))] Get,
            [Command(typeof(SetConfigurationCommand))] Set
        }

        [NamedArgument(ArgumentFlags.Optional)]
        public ConfigurationPolicy Policy { get; set; }

        [PositionalArgument(ArgumentFlags.Required, LongName = "ConfigAction")]
        public CommandGroup<Ty> Command { get; set; }

        public Task<CommandResult> ExecuteAsync(CancellationToken cancel) => Command.ExecuteAsync(cancel);
    }
}