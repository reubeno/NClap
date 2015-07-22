using System.Collections.Generic;
using System.IO;

namespace NClap.Parser
{
    /// <summary>
    /// Stock implementation of the answer file reader interface.
    /// </summary>
    internal class FileSystemReader : IFileSystemReader
    {
        private FileSystemReader()
        {
        }

        /// <summary>
        /// Public factory method.
        /// </summary>
        /// <returns>A file system reader instance.</returns>
        public static FileSystemReader Create() => s_instance;

        /// <summary>
        /// Singleton instance of this class.
        /// </summary>
        private static readonly FileSystemReader s_instance = new FileSystemReader();

        /// <summary>
        /// Checks if the path exists as a file.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns>True if the path exists and references a non-directory
        /// file; false otherwise.</returns>
        public bool FileExists(string path) => File.Exists(path);

        /// <summary>
        /// Checks if the path exists as a directory.
        /// </summary>
        /// <param name="path">Path to check.</param>
        /// <returns>True if the path exists and references a directory; false
        /// otherwise.</returns>
        public bool DirectoryExists(string path) => Directory.Exists(path);

        /// <summary>
        /// Enumerates the names of the files and directories that exist in the
        /// indicated directory, and which match the provided file pattern.
        /// </summary>
        /// <param name="directoryPath">Path to the containing directory.</param>
        /// <param name="filePattern">The file pattern to match.</param>
        /// <returns>An enumeration of the names of the files.</returns>
        public IEnumerable<string> EnumerateFileSystemEntries(string directoryPath, string filePattern) =>
            Directory.EnumerateFileSystemEntries(directoryPath, filePattern);

        /// <summary>
        /// Enumerate the textual lines in the specified file.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <returns>The line enumeration.</returns>
        public IEnumerable<string> GetLines(string filePath) => File.ReadLines(filePath);
    }
}
