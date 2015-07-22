using NClap.Metadata;

namespace NClap.Repl.TestApp
{
    enum ProgramMode
    {
        Invalid,
        SomeMode,
        DifferentMode
    }

    [ArgumentSet(AdditionalHelp = "Some tool.", Examples = new [] {"Foo", "Bar"})]
    class ProgramArguments : HelpArgumentsBase
    {
        [PositionalArgument(ArgumentFlags.AtMostOnce)]
        public ProgramMode PosMode { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)]
        public ProgramMode NamedMode { get; set; }
    }
}
