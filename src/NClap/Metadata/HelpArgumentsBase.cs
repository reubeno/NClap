namespace NClap.Metadata
{
    /// <summary>
    /// Base class to be used with the command-line parser to ensure that /?
    /// will display help usage information.
    /// </summary>
    public class HelpArgumentsBase : IHelpArguments
    {
        /// <summary>
        /// True if the user wants to receive usage help information; false
        /// otherwise.
        /// </summary>
        [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "?", Description = "Display help information")]
        public bool Help { get; set; }
    }
}
