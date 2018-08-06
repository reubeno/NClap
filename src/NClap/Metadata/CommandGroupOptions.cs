using NClap.Utilities;

namespace NClap.Metadata
{
    /// <summary>
    /// Options for command groups.
    /// </summary>
    public class CommandGroupOptions : IDeepCloneable<CommandGroupOptions>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        internal CommandGroupOptions()
        {
        }

        /// <summary>
        /// Deeply cloning constructor.
        /// </summary>
        /// <param name="other">Template for clone.</param>
        private CommandGroupOptions(CommandGroupOptions other)
        {
            ServiceConfigurer = other.ServiceConfigurer;
        }

        /// <summary>
        /// Service configurer.
        /// </summary>
        internal ServiceConfigurer ServiceConfigurer { get; set; }

        /// <summary>
        /// Duplicates the options.
        /// </summary>
        /// <returns>The duplicate.</returns>
        public CommandGroupOptions DeepClone() => new CommandGroupOptions(this);
    }
}