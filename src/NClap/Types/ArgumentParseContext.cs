using System;
using System.Diagnostics.CodeAnalysis;
using NClap.Metadata;

namespace NClap.Types
{
    /// <summary>
    /// Context for parsing values.
    /// </summary>
    public class ArgumentParseContext
    {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private IFileSystemReader _fileSystemReader = Parser.FileSystemReader.Create();

        /// <summary>
        /// The default context for parsing.
        /// </summary>
        public static ArgumentParseContext Default => new ArgumentParseContext();

        /// <summary>
        /// The file-system reader to use during parsing.
        /// </summary>
        public IFileSystemReader FileSystemReader
        {
            get => _fileSystemReader;
            set => _fileSystemReader = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Caller-selected context objext provided in CommandLineParserOptions
        /// object.
        /// </summary>
        public object ParserContext { get; set; }

        /// <summary>
        /// Options for parsing numeric arguments.
        /// </summary>
        public NumberOptions NumberOptions { get; set; }

        /// <summary>
        /// True to allow "empty" arguments (e.g. empty strings); false to
        /// consider them invalid.
        /// </summary>
        public bool AllowEmpty { get; set; }

        /// <summary>
        /// True for parsing to be case sensitive; false to be case insensitive.
        /// </summary>
        public bool CaseSensitive { get; set; }
    }
}
