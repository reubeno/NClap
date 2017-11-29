using NClap.Types;

namespace NClap
{
    /// <summary>
    /// Set of options for command-line parsing operations.
    /// </summary>
    public class CommandLineParserOptions
    {
        /// <summary>
        /// True to display usage info on error; false otherwise.
        /// </summary>
        public bool DisplayUsageInfoOnError { get; set; } = true;

        /// <summary>
        /// Specifies which options to use when display usage.
        /// </summary>
        public UsageInfoOptions UsageInfoOptions { get; set; } = UsageInfoOptions.Default;

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
        public CommandLineParserOptions Clone() => new CommandLineParserOptions
        {
            DisplayUsageInfoOnError = DisplayUsageInfoOnError,
            UsageInfoOptions = UsageInfoOptions,
            Reporter = Reporter,
            FileSystemReader = FileSystemReader,
            Context = Context
        };
    }
}
