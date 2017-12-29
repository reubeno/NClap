namespace NClap.Metadata
{
    /// <summary>
    /// Simple implementation of <see cref="IArgumentSetWithHelp"/>.
    /// </summary>
    public class HelpArgumentsBase : IArgumentSetWithHelp
    {
        /// <summary>
        /// True if the user wants to receive usage help information; false
        /// otherwise.
        /// </summary>
        [NamedArgument(ArgumentFlags.AtMostOnce, Description = "Display help information")]
        public bool Help { get; set; }
    }
}
