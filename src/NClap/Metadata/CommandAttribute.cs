using System;
using System.Diagnostics.CodeAnalysis;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute class used to denote commands.
    /// </summary>
    [SuppressMessage("Performance", "CC0023:Unsealed Attribute")]
    [AttributeUsage(AttributeTargets.Field)]
    public class CommandAttribute : ArgumentValueAttribute
    {
        private readonly Type _implementingType;

        /// <summary>
        /// Default, parameterless constructor.
        /// </summary>
        protected CommandAttribute()
        {
        }

        /// <summary>
        /// Constructor that allows specifying the type that "implements"
        /// this command.
        /// </summary>
        /// <param name="implementingType">The implementing type.</param>
        public CommandAttribute(Type implementingType)
        {
            _implementingType = implementingType;
        }

        /// <summary>
        /// Gets the type that "implements" this command.
        /// </summary>
        /// <param name="commandType">The type of the command associated with this
        /// attribute.</param>
        /// <returns>The type.</returns>
        public virtual Type GetImplementingType(Type commandType) => _implementingType;
    }
}
