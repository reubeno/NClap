using NClap.Parser;

namespace NClap.Metadata
{
    /// <summary>
    /// Context for argument validation.
    /// </summary>
    public class ArgumentValidationContext
    {
        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="fileSystemReader">File system reader for context.
        /// </param>
        public ArgumentValidationContext(IFileSystemReader fileSystemReader)
        {
            FileSystemReader = fileSystemReader;
        }

        /// <summary>
        /// The file-system reader to use in this context.
        /// </summary>
        public IFileSystemReader FileSystemReader { get; }
    }
}