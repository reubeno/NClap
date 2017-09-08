using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute class used on VerbGroup types to denote possible sub-verbs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SubVerbAttribute : Attribute
    {
    }
}
