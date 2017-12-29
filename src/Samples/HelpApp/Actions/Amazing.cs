using NClap.Metadata;

namespace HelpApp.Actions
{
    internal class Amazing : SynchronousCommand
    {
        public override CommandResult Execute() => CommandResult.Success;
    }
}