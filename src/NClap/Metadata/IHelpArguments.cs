namespace NClap.Metadata
{
    /// <summary>
    /// Interface to be implemented on arguments types that expose help options.
    /// </summary>
    public interface IHelpArguments
    {
        /// <summary>
        /// True if the user wants to receive usage help information; false
        /// otherwise.
        /// </summary>
        bool Help { get; set; }
    }
}