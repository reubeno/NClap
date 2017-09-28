using NClap.Metadata;

namespace NClap.TestApp
{
    [ArgumentType(DisplayName = "Sub-command")]
    enum SubCommandType
    {
        [Command(typeof(UnimplementedCommand))]
        Foo,

        [Command(typeof(UnimplementedCommand))]
        Bar,

        [Command(typeof(CommandGroup<MainCommandType>))]
        Main
    }
}
