using System;
using System.Collections.Generic;
using System.Linq;
using NClap.Metadata;

namespace NClap.Types
{
    /// <summary>
    /// Context for parsing values.
    /// </summary>
    public class ArgumentParseContext
    {
        private IFileSystemReader _fileSystemReader = Parser.FileSystemReader.Create();
        private IReadOnlyList<string> _elementSeparators = Array.Empty<string>();

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
            set
            {
                _fileSystemReader = value ?? Parser.FileSystemReader.Create();
            }
        }

        /// <summary>
        /// Caller-selected context objext provided in <see cref="CommandLineParserOptions"/>
        /// object.
        /// </summary>
        public object ParserContext { get; set; }

        /// <summary>
        /// Options for parsing numeric arguments.
        /// </summary>
        public NumberOptions NumberOptions { get; set; } = NumberOptions.None;

        /// <summary>
        /// True to allow "empty" arguments (e.g. empty strings); false to
        /// consider them invalid.
        /// </summary>
        public bool AllowEmpty { get; set; } = false;

        /// <summary>
        /// True for parsing to be case sensitive; false to be case insensitive.
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        /// <summary>
        /// Strings that may separate multiple elements stored in the same token.
        /// </summary>
        public IReadOnlyList<string> ElementSeparators
        {
            get => _elementSeparators;
            set
            {
                if (value == null)
                {
                    _elementSeparators = Array.Empty<string>();
                }
                else
                {
                    _elementSeparators = value.ToList();
                }
            }
        }

        /// <summary>
        /// Optionally provides a reference to the object containing the one to
        /// be parsed.
        /// </summary>
        public object ContainingObject { get; set; }
    }
}
