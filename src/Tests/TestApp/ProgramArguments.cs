using NClap.Metadata;

namespace NClap.TestApp
{
    enum LogLevel
    {
        Default,
        Heightened,
        Awesome,
        Subdued
    }

    [ArgumentSet(
        Logo = Logo,
        Style = ArgumentSetStyle.PowerShell,
        Description = "Some tool that is useful only for testing.")]
    class ProgramArguments : HelpArgumentsBase
    {
        public const string Logo = @"My Test Tool
Version 1.0";

        [PositionalArgument(ArgumentFlags.Required, Position = 0)]
        public CommandGroup<MainCommandType> Command { get; set; }

        [NamedArgument(ArgumentFlags.Optional)]
        public LogLevel LogLevel { get; set; }
    }
}
