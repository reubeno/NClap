namespace NClap.Metadata
{
    /// <summary>
    /// Simple command implementation that only exits.
    /// </summary>
    public class ExitCommand : SynchronousCommand
    {
        /// <summary>
        /// Does nothing, but indicates to the caller that termination is desired.
        /// </summary>
        /// <returns>CommandResult.Terminate.</returns>
        public override CommandResult Execute() => CommandResult.Terminate;
    }
}
