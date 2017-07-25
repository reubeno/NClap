using NClap.Metadata;
using NClap.Types;

namespace NClap.TestApp
{
    enum ProgramMode
    {
        [ArgumentValue(Flags = ArgumentValueFlags.Disallowed)]
        Invalid,

        SomeMode,

        [ArgumentValue(LongName = "Different", ShortName = "D")]
        DifferentMode,

        [ArgumentValue(Flags = ArgumentValueFlags.Hidden)]
        HiddenMode
    }

    [ArgumentSet(
        AdditionalHelp = "Some tool.",
        Examples = new [] {"Foo", "Bar"},
        PublicMembersAreNamedArguments = true)]
    class ProgramArguments : HelpArgumentsBase
    {
        public string Unannotated { get; set; }

        [PositionalArgument(ArgumentFlags.AtMostOnce, Position = 0)]
        public int Foo { get; set; }

        [PositionalArgument(ArgumentFlags.AtMostOnce, Position = 1)]
        public ProgramMode PosMode { get; set; }

        [PositionalArgument(ArgumentFlags.AtMostOnce, Position = 2)]
        public FileSystemPath OtherPath { get; set; }

        [NamedArgument]
        public ProgramMode NamedMode { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = "foo")]
        [MustNotBeEmpty]
        public FileSystemPath SomePath { get; set; }

        [NamedArgument(DefaultValue = 11)]
        public uint SomeUnsignedInt { get; set; }
    }
}
