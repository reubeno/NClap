namespace NClap.Metadata
{
    /// <summary>
    /// Interface to be implemented on argument set types that expose help options.
    /// </summary>
    public interface IArgumentSetWithHelp
    {
        /// <summary>
        /// True if the user wants to receive usage help information; false
        /// otherwise.
        /// </summary>
        bool Help { get; set; }
    }
}