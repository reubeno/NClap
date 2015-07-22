using NClap.Metadata;
using NClap.Parser;
using System;

namespace NClap.Types
{
    /// <summary>
    /// Context for parsing values.
    /// </summary>
    public class ArgumentParseContext
    {
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
            get
            {
                return _fileSystemReader;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _fileSystemReader = value;
            }
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
    }
}
