using System;
using System.Collections.Generic;
using NClap.Utilities;

namespace NClap
{
    /// <summary>
    /// Extension methods for <see cref="CommandLineParserOptions" />.
    /// </summary>
    public static class CommandLineParserOptionsExtensions
    {
        /// <summary>
        /// Tries to parse the given string arguments into a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the destination object; this type should use
        /// appropriate NClap attributes to annotate and define options.</typeparam>
        /// <param name="options">Options describing how to parse.</param>
        /// <param name="arguments">The string arguments to parse.</param>
        /// <param name="result">On success, returns the constructed result object.</param>
        /// <returns>True on success; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="arguments"/>
        /// is null.</exception>
        public static bool TryParse<T>(this CommandLineParserOptions options, IEnumerable<string> arguments, out T result)
            where T : class, new() =>
            CommandLineParser.TryParse<T>(arguments, options, out result);

        /// <summary>
        /// Construct a fluent builder from options.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <returns>Fluent builder.</returns>
        public static FluentBuilder<CommandLineParserOptions> With(this CommandLineParserOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            return new FluentBuilder<CommandLineParserOptions>(options.DeepClone());
        }

        /// <summary>
        /// Quiets the parser with default options (suppressing errors and reporting).
        /// </summary>
        /// <param name="builder">Options builder.</param>
        /// <returns>The updated builder.</returns>
        public static FluentBuilder<CommandLineParserOptions> Quiet(this FluentBuilder<CommandLineParserOptions> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.AddTransformer(options =>
            {
                options.DisplayUsageInfoOnError = false;
                options.Reporter = s => { };
            });

            return builder;
        }

        /// <summary>
        /// Provides a <see cref="Action"/> to configure services within the options.
        /// </summary>
        /// <param name="builder">Options builder.</param>
        /// <param name="configurer">Service configurer action.</param>
        /// <returns>The updated builder.</returns>
        [CLSCompliant(false)]
        public static FluentBuilder<CommandLineParserOptions> ConfigureServices(this FluentBuilder<CommandLineParserOptions> builder, ServiceConfigurer configurer)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));

            builder.AddTransformer(options => options.ServiceConfigurer = configurer);
            return builder;
        }
    }
}