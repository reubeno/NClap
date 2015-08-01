using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute for annotating help verbs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HelpVerbAttribute : VerbAttribute
    {
        /// <summary>
        /// Gets the type that "implements" this verb.
        /// </summary>
        /// <param name="verbType">The type of the verb associated with this
        /// attribute.</param>
        /// <returns>The type.</returns>
        public override Type GetImplementingType(Type verbType) =>
            typeof(HelpVerb<>).MakeGenericType(verbType);
    }
}
