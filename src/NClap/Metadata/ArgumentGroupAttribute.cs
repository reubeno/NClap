using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Unused.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Obsolete("This attribute is not supported and will be removed from a future release.", true)]
    public sealed class ArgumentGroupAttribute : Attribute
    {
    }
}
