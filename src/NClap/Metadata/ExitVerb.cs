namespace NClap.Metadata
{
    /// <summary>
    /// Simple verb implementation that only exits.
    /// </summary>
    public class ExitVerb : SynchronousVerb
    {
        /// <summary>
        /// Does nothing, but indicates to the caller that termination is desired.
        /// </summary>
        /// <returns>VerbResult.Terminate</returns>
        public override VerbResult Execute() => VerbResult.Terminate;
    }
}
