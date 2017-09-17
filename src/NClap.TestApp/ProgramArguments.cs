using NClap.Metadata;

namespace NClap.TestApp
{
    [ArgumentSet(
        Logo = Logo,
        Style = ArgumentSetStyle.PowerShell,
        AdditionalHelp = "Some tool that is useful only for testing.")]
    class ProgramArguments : HelpArgumentsBase
    {
        public const string Logo = @"My Test Tool
Version 1.0";

        [PositionalArgument(ArgumentFlags.Required, Position = 0)]
        public VerbGroup<ReplVerbType> Verb { get; set; }
    }
}
