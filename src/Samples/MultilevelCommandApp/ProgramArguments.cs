using NClap.Metadata;

namespace MultilevelCommandApp
{
    class ProgramArguments
    {
        public enum ToplevelCommandType
        {
            [ArgumentValue(Flags = ArgumentValueFlags.Disallowed)] Invalid,

            [Command(typeof(ConfigurationCommand), LongName = "Config")] Configuration,
            [Command(typeof(StatusCommand))] Status,

            [Command(typeof(ExitCommand))] Exit,
            [HelpCommand] Help
        }
        
        [NamedArgument]
        public bool Verbose { get; set; }

        [PositionalArgument(ArgumentFlags.Optional)]
        public CommandGroup<ToplevelCommandType> Command { get; set; }
    }
}
