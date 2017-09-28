using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute for objects that group arguments, and which should be
    /// included when parsing the containing object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ArgumentGroupAttribute : Attribute
    {
    }
}
