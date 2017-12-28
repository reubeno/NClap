using NClap.Utilities;

namespace NClap.Help
{
    /// <summary>
    /// Extension methods for interacting with <see cref="ArgumentSetHelpOptions"/>.
    /// </summary>
    public static partial class ArgumentSetHelpOptionsExtensions
    {
        /// <summary>
        /// Construct a fluent builder from options.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <returns>Fluent builder.</returns>
        public static FluentBuilder<ArgumentSetHelpOptions> With(this ArgumentSetHelpOptions options) =>
            new FluentBuilder<ArgumentSetHelpOptions>(options);

        /// <summary>
        /// Updates the maximum width.
        /// </summary>
        /// <param name="builder">Options builders.</param>
        /// <param name="maxWidth">Maximum width.</param>
        /// <returns>The updated options.</returns>
        public static FluentBuilder<ArgumentSetHelpOptions> MaxWidth(this FluentBuilder<ArgumentSetHelpOptions> builder, int maxWidth)
        {
            builder.AddTransformer(options => options.MaxWidth = maxWidth);
            return builder;
        }
    }
}
