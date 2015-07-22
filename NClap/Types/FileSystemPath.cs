using System;
using System.Collections.Generic;
using System.IO;
using System.Security;

namespace NClap.Types
{
    using System.Globalization;

    /// <summary>
    /// Encapsulates a file-system path (i.e. to file or directory).
    /// </summary>
    public class FileSystemPath
    {
        /// <summary>
        /// Path constructor.
        /// </summary>
        /// <param name="path">The path string.</param>
        /// <param name="expandEnvironmentVariables">True to expand environment
        /// variables; false to leave environment variables unexpanded.</param>
        /// <param name="rootPathForRelativePaths">Root path to resolve the
        /// path with respect to, if it's relative.</param>
        public FileSystemPath(string path, bool expandEnvironmentVariables, string rootPathForRelativePaths)
        {
            var pathToUse = expandEnvironmentVariables ? Environment.ExpandEnvironmentVariables(path) : path;

            Path = (rootPathForRelativePaths != null) ? GetFullPath(pathToUse, rootPathForRelativePaths) : pathToUse;
            OriginalPath = path;
        }

        /// <summary>
        /// Path constructor.  Relative paths are left unresolved.
        /// </summary>
        /// <param name="path">The path string.</param>
        /// <param name="expandEnvironmentVariables">True to expand environment
        /// variables; false to leave environment variables unexpanded.</param>
        public FileSystemPath(string path, bool expandEnvironmentVariables) : this(path, expandEnvironmentVariables, null)
        {
        }

        /// <summary>
        /// Path constructor.  Environment variables in the path are expanded.
        /// Relative paths are left unresolved.
        /// </summary>
        /// <param name="path">The path string.</param>
        public FileSystemPath(string path) : this(path, expandEnvironmentVariables: true)
        {
        }

        /// <summary>
        /// The path as a string.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The original, unexpanded path as a string.
        /// </summary>
        public string OriginalPath { get; }

        /// <summary>
        /// Implicit operator to construct a path from a string.
        /// </summary>
        /// <param name="path">The path string.</param>
        /// <returns>The path object.</returns>
        public static implicit operator FileSystemPath(string path) =>
            (path == null) ? null : new FileSystemPath(path);

        /// <summary>
        /// Implicit operator to construct a string from a path.
        /// </summary>
        /// <param name="path">The path object.</param>
        /// <returns>The path string.</returns>
        public static implicit operator string(FileSystemPath path) => path?.Path;

        /// <summary>
        /// Get possible completions of the provided path prefix string.
        /// </summary>
        /// <param name="context">Context for completion.</param>
        /// <param name="pathPrefix">Path prefix to complete.</param>
        /// <returns>Possible completions.</returns>
        public static IEnumerable<string> GetCompletions(ArgumentCompletionContext context, string pathPrefix)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // If the string to be completed is empty, then treat it as if it
            // were a reference to the current working directory (with the
            // intention of completing to names of files in it).
            if (string.IsNullOrEmpty(pathPrefix))
            {
                pathPrefix = @".\";
            }

            try
            {
                // If the string to be completed has no valid directory part,
                // then use its root as the directory name.  If there's no
                // root either, then assume the current working directory
                // is what we should use.
                var directoryPath = System.IO.Path.GetDirectoryName(pathPrefix);
                if (string.IsNullOrEmpty(directoryPath))
                {
                    var rootPath = System.IO.Path.GetPathRoot(pathPrefix);
                    directoryPath = !String.IsNullOrEmpty(rootPath) ? rootPath : ".";
                }

                // Construct a glob-style file pattern for matching.
                var filePattern = System.IO.Path.GetFileName(pathPrefix) + "*";

                // Enumerate all matching files in the selected directory.
                return context.ParseContext.FileSystemReader.EnumerateFileSystemEntries(directoryPath, filePattern);
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is ArgumentException ||
                ex is NotSupportedException ||
                ex is SecurityException ||
                ex is UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Converts this path object into a string.
        /// </summary>
        /// <returns>The path string.</returns>
        public override string ToString() => this;

        private static string GetFullPath(string path, string workingDirectory)
        {
            if (!System.IO.Path.IsPathRooted(path))
            {
                path = System.IO.Path.Combine(workingDirectory, path);
            }
            else if (System.IO.Path.GetPathRoot(path) == System.IO.Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture))
            {
                var workingDirectoryRootPath = System.IO.Path.GetPathRoot(workingDirectory);
                if (workingDirectoryRootPath != null)
                {
                    path = System.IO.Path.Combine(
                        workingDirectoryRootPath,
                        path.TrimStart(System.IO.Path.DirectorySeparatorChar));
                }
            }

            return System.IO.Path.GetFullPath(path).TrimEnd(System.IO.Path.DirectorySeparatorChar);
        }
    }
}
