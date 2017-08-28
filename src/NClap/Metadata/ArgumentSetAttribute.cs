using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace NClap.Metadata
{
    /// <summary>
    /// Attributes that may be used on classes representing argument sets
    /// (i.e. classes including fields and/or properties with
    /// ArgumentAttributes).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ArgumentSetAttribute : Attribute
    {
        private string[] _namedArgumentPrefixes;
        private string[] _shortNameArgumentPrefixes;
        private char[] _argumentValueSeparators;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ArgumentSetAttribute()
        {
            // Detect whether '/' is a file-system separator; if it is, then we
            // choose not to include it in any default prefix lists.
            bool useForwardSlashAsPrefix = !Path.DirectorySeparatorChar.Equals('/');

            // Initialize properties with basic defaults.
            AnswerFileArgumentPrefix = "@";
            ArgumentValueSeparators = new[] { '=', ':' };

            // Initialize context-sensitive defaults.
            if (useForwardSlashAsPrefix)
            {
                NamedArgumentPrefixes = new[] { "/", "-" };
                ShortNameArgumentPrefixes = new[] { "/", "-" };
            }
            else
            {
                NamedArgumentPrefixes = new[] { "--" };
                ShortNameArgumentPrefixes = new[] { "-" };
            }
        }

        /// <summary>
        /// If this is non-null, it is added to the end of the help/usage
        /// message when that is generated.
        /// </summary>
        public string AdditionalHelp { get; set; }

        /// <summary>
        /// Optionally provides examples for usage information.
        /// </summary>
        public string[] Examples { get; set; }

        /// <summary>
        /// If this is non-null, it is the argument prefix that references an
        /// argument answer file.
        /// </summary>
        public string AnswerFileArgumentPrefix { get; set; }

        /// <summary>
        /// True to indicate that all writable, public members (in the type
        /// annotated with this attribute) should be treated as optional named
        /// arguments.
        /// </summary>
        public bool PublicMembersAreNamedArguments { get; set; }

        /// <summary>
        /// If this is non-null, it is the set of prefixes that indicate named
        /// arguments indicated by long name.  The first separator listed
        /// in this array is considered "preferred" and will be used in
        /// generated usage help information.  This array may not be null.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Needs to be array so it functions as an attribute parameter")]
        public string[] NamedArgumentPrefixes
        {
            get
            {
                return _namedArgumentPrefixes;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _namedArgumentPrefixes = value;
            }
        }

        /// <summary>
        /// If this is non-null, it is the set of prefixes that indicate named
        /// arguments' short names.  The first separator listed in this array
        /// is considered "preferred" and will be used in generated usage help
        /// information.  This array may not be null.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Needs to be array so it functions as an attribute parameter")]
        public string[] ShortNameArgumentPrefixes
        {
            get
            {
                return _shortNameArgumentPrefixes;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _shortNameArgumentPrefixes = value;
            }
        }

        /// <summary>
        /// The set of characters that separate a named argument from the
        /// value associated with it.  The first separator listed in this
        /// array is considered "preferred" and will be used in generated
        /// usage help information.  This array may not be null.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Needs to be array so it functions as an attribute parameter")]
        public char[] ArgumentValueSeparators
        {
            get
            {
                return _argumentValueSeparators;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _argumentValueSeparators = value;
            }
        }
    }
}
