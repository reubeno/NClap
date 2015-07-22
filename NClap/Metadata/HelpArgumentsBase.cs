namespace NClap.Metadata
{
    /// <summary>
    /// Base class to be used with the command-line parser to ensure that /?
    /// will display help usage information.
    /// </summary>
    public class HelpArgumentsBase
    {
        /// <summary>
        /// True if the user wants to receive usage help information; false
        /// otherwise.
        /// </summary>
        [NamedArgument(ArgumentFlags.AtMostOnce, ShortName = "?")]
        public bool Help { get; set; }
    }
}
