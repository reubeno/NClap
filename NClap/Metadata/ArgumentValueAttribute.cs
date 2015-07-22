using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute for annotating values that can be used with arguments. It is
    /// most frequently used with values on enum types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ArgumentValueAttribute : Attribute
    {
        /// <summary>
        /// Flags controlling the use of this value.
        /// </summary>
        public ArgumentValueFlags Flags { get; set; }

        /// <summary>
        /// The short name used to identify this value.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// The long name used to identify this value.
        /// </summary>
        public string LongName { get; set; }
    }
}
