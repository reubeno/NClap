using System;
using NClap.Help;
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
            HelpOutputHandler = other.HelpOutputHandler;
        }

        /// <summary>
        /// Parser options; initialized with defaults.
        /// </summary>
        public CommandLineParserOptions ParserOptions { get; set; } = new CommandLineParserOptions
        {
            HelpOptions = new ArgumentSetHelpOptions
            {
                Logo = new ArgumentMetadataHelpOptions { Include = false },
                Name = string.Empty
            }
        };

        /// <summary>
        /// The output handler for help/usage information.
        /// </summary>
        public Action<ColoredMultistring> HelpOutputHandler { get; set; }

        /// <summary>
        /// Creates a separate clone of this object.
        /// </summary>
        /// <returns>Clone.</returns>
        public LoopOptions DeepClone() => new LoopOptions(this);
    }
}
