using System;
using System.Diagnostics.CodeAnalysis;

namespace NClap.Metadata
{
    /// <summary>
    /// Used to control parsing of command line arguments.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Zero means something that needs to be described")]
    [Flags]
    public enum ArgumentFlags
    {
        /// <summary>
        /// The argument is not required, but an error will be reported if it is
        /// specified more than once.
        /// </summary>
        AtMostOnce = 0x00,

        /// <summary>
        /// Indicates that this field is required. An error will be displayed
        /// if it is not present when parsing arguments.
        /// </summary>
        Required = 0x01,

        /// <summary>
        /// Indicates that the argument may be specified more than once. Only
        /// valid if the argument is a collection.
        /// </summary>
        Multiple = 0x02,

        /// <summary>
        /// Only valid in conjunction with Multiple. Duplicate values will
        /// result in an error.
        /// </summary>
        Unique = 0x04,

        /// <summary>
        /// The argument is not required, but if it is encountered, then the
        /// rest of the command line will be consumed and used as its value
        /// (whether or not the arguments start with a prefix indicating a
        /// named argument or answer file).  Only valid for string collections.
        /// It is different from a default argument which just consumes parts
        /// of the command line that don't start with those characters and
        /// aren't used as values for other arguments.  If a "RestOfLine"
        /// argument is encountered on the command line, parsing stops and
        /// the rest of the line is passed as-is.
        /// </summary>
        RestOfLine = 0x08,

        /// <summary>
        /// The argument is permitted to occur multiple times, but duplicate
        /// values will cause an error to be reported.
        /// </summary>
        MultipleUnique = Multiple | Unique,

        /// <summary>
        /// The argument is required and may be specified more than once.
        /// </summary>
        AtLeastOnce = Multiple | Required
    }
}