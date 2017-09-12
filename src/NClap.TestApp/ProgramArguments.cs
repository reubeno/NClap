using NClap.Metadata;
using NClap.Types;

namespace NClap.TestApp
{
    enum ProgramMode
    {
        [ArgumentValue(Flags = ArgumentValueFlags.Disallowed)]
        Invalid,

        SomeMode,

        [ArgumentValue(LongName = "Different", ShortName = "D", HelpText = "A different mode of a different kind")]
        DifferentMode,

        [ArgumentValue(Flags = ArgumentValueFlags.Hidden)]
        HiddenMode
    }

    class NestedArguments
    {
        [NamedArgument]
        public string SomethingNested { get; set; }
    }

    [ArgumentSet(
        AdditionalHelp = "Some tool.",
        Examples = new [] {"Foo", "Bar"},
        PublicMembersAreNamedArguments = true)]
    class ProgramArguments : HelpArgumentsBase
    {
        public string Unannotated { get; set; }

        [PositionalArgument(ArgumentFlags.AtMostOnce, Position = 0,
            HelpText = "Some foo that you might want to do something with")]
        public int Foo { get; set; }

        [PositionalArgument(ArgumentFlags.AtMostOnce, Position = 1, HelpText = "A positional program mode option")]
        public ProgramMode PosMode { get; set; }

        [ArgumentGroup]
        public NestedArguments Nested { get; set; } = new NestedArguments();

        [PositionalArgument(ArgumentFlags.AtMostOnce, Position = 2, HelpText = "That other path")]
        public FileSystemPath OtherPath { get; set; }

        [NamedArgument(HelpText = "My named mode")]
        public ProgramMode NamedMode { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce, DefaultValue = "foo", HelpText = "Some path that really must not be empty")]
        [MustNotBeEmpty]
        public FileSystemPath SomePath { get; set; }

        [NamedArgument(DefaultValue = 11,
            HelpText = "Some unsigned integer that has a default value turned up all the way to 11")]
        public uint SomeUnsignedInt { get; set; }

        [PositionalArgument(Position = 3)]
        public VerbGroup<VerbType> MyVerb { get; set; }
    }
}
