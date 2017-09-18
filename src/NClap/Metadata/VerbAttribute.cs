using System;
using System.Diagnostics.CodeAnalysis;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute class used to denote verbs.
    /// </summary>
    [SuppressMessage("Performance", "CC0023:Unsealed Attribute")]
    [AttributeUsage(AttributeTargets.Field)]
    public class VerbAttribute : ArgumentValueAttribute
    {
        private readonly Type _implementingType;

        /// <summary>
        /// Default, parameterless constructor.
        /// </summary>
        public VerbAttribute()
        {
        }

        /// <summary>
        /// Constructor that allows specifying the type that "implements"
        /// this verb.
        /// </summary>
        /// <param name="implementingType">The implementing type.</param>
        public VerbAttribute(Type implementingType)
        {
            _implementingType = implementingType;
        }

        /// <summary>
        /// Gets the type that "implements" this verb.
        /// </summary>
        /// <param name="verbType">The type of the verb associated with this
        /// attribute.</param>
        /// <returns>The type.</returns>
        public virtual Type GetImplementingType(Type verbType) => _implementingType;
    }
}
