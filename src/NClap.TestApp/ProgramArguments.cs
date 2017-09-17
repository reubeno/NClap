using NClap.Metadata;

namespace NClap.TestApp
{
    enum ProgramVerbType
    {
        [ArgumentValue(Flags = ArgumentValueFlags.Disallowed)]
        Invalid,

        [Verb(typeof(ReplVerb))]
        Repl
    }

    [ArgumentSet(
        Logo = Logo,
        AdditionalHelp = "Some tool that is useful only for testing.")]
    class ProgramArguments : HelpArgumentsBase
    {
        public const string Logo = @"My Test Tool
Version 1.0";

        [PositionalArgument(ArgumentFlags.Required, Position = 0)]
        public VerbGroup<ProgramVerbType> Verb { get; set; }
    }
}
