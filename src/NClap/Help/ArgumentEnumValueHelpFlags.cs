using System;

namespace NClap.Help
{
    /// <summary>
    /// Mode for generating help for enum values.
    /// </summary>
    [Flags]
    public enum ArgumentEnumValueHelpFlags
    {
        /// <summary>
        /// Each enum is documented in the default format, at each use site.
        /// This summary will be duplicated if the enum type is used in multiple
        /// arguments.
        /// </summary>
        None = 0,

        /// <summary>
        /// Enum types with multiple references will be promoted to their own
        /// sections.
        /// </summary>
        SingleSummaryOfEnumsWithMultipleUses = 0x1,

        /// <summary>
        /// Command enums will be promoted to their own sections.
        /// </summary>
        SingleSummaryOfAllCommandEnums = 0x2
    }
}
