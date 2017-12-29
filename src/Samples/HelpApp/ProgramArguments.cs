using NClap.Metadata;

namespace HelpApp
{
    [ArgumentType(DisplayName = "OperationalMode")]
    internal enum Mode
    {
        [ArgumentValue(
            LongName = "functional",
            ShortName = "func",
            Description = "Functional mode of operation")]
        Functional,

        [ArgumentValue(
            LongName = "behavioral",
            ShortName = "behave",
            Description = "Mode focusing on how the application behaves")]
        Behavioral,

        [ArgumentValue(
            LongName = "nontemporal",
            ShortName = "nt",
            Description = "Mode that is not bound to space or time")]
        NonTemporal
    }

    [ArgumentType(DisplayName = "Action")]
    internal enum ProgramActionType
    {
        [Command(
            typeof(Actions.Mediocre),
            LongName = "mediocre",
            Description = "Do something mediocre, at best. It's quite possible something far worse may result as well. That's okay.")]
        DoSomethingMediocre,

        [Command(
            typeof(Actions.Amazing),
            LongName = "amazing",
            Description = "Do something truly stupendous. Amaze us. Amaze even the most jaded of observers that will behold this.")]
        DoSomethingAmazing
    }

    [ArgumentSet(
        Logo = "Sample Help Application v0.1\n" + "Expect nothing.",
        Description = "This is a sample application that exercises the help content generation facilities of NClap. Nothing contained within has any specific meaning or semantic sense, but is structurally representative.",
        PreferNamedArgumentValueAsSucceedingToken = true,
        Style = ArgumentSetStyle.GetOpt)]
    internal class ProgramArguments : IHelpArguments
    {
        [PositionalArgument(
            ArgumentFlags.Required,
            Position = 0,
            LongName = "mode",
            Description = "Specifies the mode in which the application should run. This is very important and you should specify this.")]
        public Mode Mode { get; set; }

        [PositionalArgument(
            ArgumentFlags.Optional,
            Position = 1,
            LongName = "action",
            Description = "An optional action that you may request. This program may choose to comply. It may also choose not to comply.")]
        public CommandGroup<ProgramActionType> Action { get; set; }

        [NamedArgument(
            LongName = "level",
            ShortName = "l",
            DefaultValue = 11,
            Description = "General level of awesomeness desired of this application. The default value is perhaps unsurprising.")]
        public int LevelOfAwesomeness { get; set; }

        [NamedArgument(
            LongName = "verbose",
            ShortName = "v",
            Description = "Enables verbose debug output; primarily useful for debugging and investigations.")]
        public bool Verbose { get; set; }

        [NamedArgument(
            LongName = "help",
            ShortName = "h",
            Description = "Displays this help content; primarily useful for ensuring that you have the right options and for discovering new and exciting options that you can use.")]
        public bool Help { get; set; }
    }
}
