using System.Collections.Generic;

namespace NClap.Parser
{
    /// <summary>
    /// Abstract interface for reading/querying file system contents.
    /// </summary>
    public interface IFileSystemReader
    {
        /// <summary>
        /// Checks if the path exists as a file.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns>True if the path exists and references a non-directory
        /// file; false otherwise.</returns>
        bool FileExists(string path);

        /// <summary>
        /// Checks if the path exists as a directory.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns>True if the path exists and references a directory; false
        /// otherwise.</returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Enumerates the names of the files and directories that exist in the
        /// indicated directory, and which match the provided file pattern.
        /// </summary>
        /// <param name="directoryPath">Path to the containing directory.
        /// </param>
        /// <param name="filePattern">The file pattern to match.</param>
        /// <returns>An enumeration of the names of the files.</returns>
        IEnumerable<string> EnumerateFileSystemEntries(string directoryPath, string filePattern);

        /// <summary>
        /// Enumerate the textual lines in the specified file.  Throws an
        /// IOException if I/O errors occur while accessing the file.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns>The line enumeration.</returns>
        IEnumerable<string> GetLines(string filePath);
    }
}
