using NClap.Utilities;

namespace NClap.Repl
{
    /// <summary>
    /// Options for executing loops.
    /// </summary>
    public class LoopOptions : IDeepCloneable<LoopOptions>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public LoopOptions()
        {
        }

        /// <summary>
        /// Deeply cloning constructor.
        /// </summary>
        /// <param name="other">Template for clone.</param>
        private LoopOptions(LoopOptions other)
        {
            ParserOptions = other.ParserOptions?.DeepClone();
        }

        /// <summary>
        /// Parser options.
        /// </summary>
        public CommandLineParserOptions ParserOptions { get; set; } = new CommandLineParserOptions();

        /// <summary>
        /// Creates a separate clone of this object.
        /// </summary>
        /// <returns>Clone.</returns>
        public LoopOptions DeepClone() => new LoopOptions(this);
    }
}
