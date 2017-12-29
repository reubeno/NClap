using NClap.Metadata;

namespace HelpApp.Actions
{
    internal enum MediocrityLevel
    {
        Mediocre,
        TrulyMediocre
    }

    internal class Mediocre : SynchronousCommand
    {
        [NamedArgument(
            ArgumentFlags.Required,
            LongName = "med-level",
            Description = "Level of mediocrity desired to be attained. Or barely passed.")]
        MediocrityLevel Level { get; set; }

        public override CommandResult Execute() => CommandResult.Success;
    }
}