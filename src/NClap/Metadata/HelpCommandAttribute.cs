using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute for annotating help commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HelpCommandAttribute : CommandAttribute
    {
        /// <summary>
        /// Gets the type that "implements" this command.
        /// </summary>
        /// <param name="commandType">The type of the command associated with this
        /// attribute.</param>
        /// <returns>The type.</returns>
        public override Type GetImplementingType(Type commandType) =>
            typeof(HelpCommand<>).MakeGenericType(commandType);
    }
}
