using NClap.Metadata;
using NClap.Types;

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
        [PositionalArgument(ArgumentFlags.Required, Position = 0)]
        public int Foo { get; set; }

        [PositionalArgument(ArgumentFlags.AtMostOnce, Position = 1)]
        public ProgramMode PosMode { get; set; }

        [PositionalArgument(ArgumentFlags.AtMostOnce, Position = 2)]
        public FileSystemPath OtherPath { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)]
        public ProgramMode NamedMode { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = "foo")]
        [MustNotBeEmpty]
        public FileSystemPath SomePath { get; set; }
    }
}
