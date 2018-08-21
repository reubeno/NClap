using System;
using Microsoft.Extensions.DependencyInjection;
using NClap.Help;
using NClap.Types;
using NClap.Utilities;

namespace NClap
{
    /// <summary>
    /// Delegate type for a method that configures a service collection.
    /// </summary>
    /// <param name="serviceCollection">Service collection to be configured.</param>
    [CLSCompliant(false)]
    public delegate void ServiceConfigurer(IServiceCollection serviceCollection);

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
            HelpOptions = other.HelpOptions?.DeepClone();
            Reporter = other.Reporter;
            FileSystemReader = other.FileSystemReader;
            Context = other.Context;
            ServiceConfigurer = other.ServiceConfigurer;
        }

        /// <summary>
        /// True to display usage info on parse error; false otherwise.
        /// </summary>
        public bool DisplayUsageInfoOnError { get; set; } = true;

        /// <summary>
        /// Specifies which options to use when generating (and displaying)
        /// help for the argument set being parsed.
        /// </summary>
        public ArgumentSetHelpOptions HelpOptions { get; set; } = new ArgumentSetHelpOptions();

        /// <summary>
        /// Function to invoke when reporting errors. Defaults to a basic
        /// reporter that displays errors to the console.
        /// </summary>
        public ErrorReporter Reporter { get; set; } = CommandLineParser.DefaultReporter;

        /// <summary>
        /// File system reader to use.
        /// </summary>
        public IFileSystemReader FileSystemReader { get; set; } = Parser.FileSystemReader.Create();

        /// <summary>
        /// Arbitrary context object to be made available in created instances
        /// of the <see cref="ArgumentParseContext" /> type.
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// Optionally provides an action invoked to configure the <see cref="IServiceCollection"/>
        /// associated with these options.
        /// </summary>
        [CLSCompliant(false)]
        public ServiceConfigurer ServiceConfigurer { get; set; }

        /// <summary>
        /// Duplicates the options.
        /// </summary>
        /// <returns>The duplicate.</returns>
        public CommandLineParserOptions DeepClone() => new CommandLineParserOptions(this);

        /// <summary>
        /// Constructs a new set of options intended for quiet operation (i.e. no
        /// console output).
        /// </summary>
        /// <returns>The options.</returns>
        public static CommandLineParserOptions Quiet() => new CommandLineParserOptions
        {
            DisplayUsageInfoOnError = false,
            Reporter = s => { }
        };
    }
}
