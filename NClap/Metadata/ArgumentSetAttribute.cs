using System;
using System.Diagnostics.CodeAnalysis;

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
        private char[] _argumentValueSeparators;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ArgumentSetAttribute()
        {
            // Initialize properties with defaults.
            AnswerFileArgumentPrefix = "@";
            NamedArgumentPrefixes = new[] { "/", "-" };
            ArgumentValueSeparators = new[] { '=', ':' };
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
        /// argument answer file.  Defaults to "@".
        /// </summary>
        public string AnswerFileArgumentPrefix { get; set; }

        /// <summary>
        /// If this is non-null, it is the set of prefixes that indicate named
        /// arguments.  Defaults to { "/", "-" }.  The first separator listed
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
        /// The set of characters that separate a named argument from the
        /// value associated with it.  Defaults to { '=', ':' }.  The first
        /// separator listed in this array is considered "preferred" and will
        /// be used in generated usage help information.  This array may not
        /// be null.
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
