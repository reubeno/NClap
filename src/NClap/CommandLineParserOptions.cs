using NClap.Help;
using NClap.Types;
using NClap.Utilities;

namespace NClap
{
    /// <summary>
    /// Set of options for command-line parsing operations.
    /// </summary>
    public class CommandLineParserOptions : IDeepCloneable<CommandLineParserOptions>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public CommandLineParserOptions()
        {
        }

        /// <summary>
        /// Deeply cloning constructor.
        /// </summary>
        /// <param name="other">Template for clone.</param>
        private CommandLineParserOptions(CommandLineParserOptions other)
        {
            DisplayUsageInfoOnError = other.DisplayUsageInfoOnError;
            HelpOptions = other.HelpOptions.DeepClone();
            Reporter = other.Reporter;
            FileSystemReader = other.FileSystemReader;
            Context = other.Context;
        }

        /// <summary>
        /// True to display usage info on error; false otherwise.
        /// </summary>
        public bool DisplayUsageInfoOnError { get; set; } = true;

        /// <summary>
        /// Specifies which options to use when generating (and displaying)
        /// help for the argument set being parsed.
        /// </summary>
        public ArgumentSetHelpOptions HelpOptions { get; set; } = new ArgumentSetHelpOptions();

        /// <summary>
        /// Function to invoke when reporting errors.
        /// </summary>
        public ErrorReporter Reporter { get; set; }

        /// <summary>
        /// File system reader to use.
        /// </summary>
        public IFileSystemReader FileSystemReader { get; set; }

        /// <summary>
        /// Arbitrary context object to be made available in created instances
        /// of the <see cref="ArgumentParseContext" /> type.
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// Duplicates the options.
        /// </summary>
        /// <returns>The duplicate.</returns>
        public CommandLineParserOptions DeepClone() => new CommandLineParserOptions(this);
    }
}
