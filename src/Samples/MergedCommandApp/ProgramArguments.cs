using NClap.Metadata;

namespace MergedCommandApp
{
    internal class ProgramArguments : IArgumentSetWithHelp
    {
        [PositionalArgument(ArgumentFlags.Required)]
        public CommandGroup<CommandType> Command { get; set; }

        [NamedArgument]
        public bool Help { get; set; }
    }
}
