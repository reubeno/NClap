using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute class used to denote verbs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class VerbAttribute : Attribute
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
        /// The help text associated with this verb.
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// True to indicate that this verb will "exit" the contained REPL;
        /// false otherwise.
        /// </summary>
        public bool Exits { get; set; }

        /// <summary>
        /// Gets the type that "implements" this verb.
        /// </summary>
        /// <param name="verbType">The type of the verb associated with this
        /// attribute.</param>
        /// <returns>The type.</returns>
        public virtual Type GetImplementingType(Type verbType) => _implementingType;
    }
}
