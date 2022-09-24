using System;

namespace NClap.Metadata
{
    // CA1813: Avoid unsealed attributes
#pragma warning disable CA1813

    // CA1019: Define accessors for attribute arguments
#pragma warning disable CA1019

    /// <summary>
    /// Attribute class used to denote commands.
    /// </summary>
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

#pragma warning restore CA1019
#pragma warning restore CA1813
}
