using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute for annotating types that can be used as arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public sealed class ArgumentTypeAttribute : Attribute
    {
        /// <summary>
        /// Optionally indicates how this type should be displayed in
        /// help/usage information.
        /// </summary>
        public string DisplayName { get; set; }
    }
}
