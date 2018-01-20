using System;
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
        /// Updates the "use color or not" preference.
        /// </summary>
        /// <param name="builder">Options builders.</param>
        /// <param name="useColor">true to use color; false otherwise.</param>
        /// <returns>The updated options.</returns>
        public static FluentBuilder<ArgumentSetHelpOptions> Color(this FluentBuilder<ArgumentSetHelpOptions> builder, bool useColor)
        {
            builder.AddTransformer(options => options.UseColor = useColor);
            return builder;
        }

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

        /// <summary>
        /// Updates help options to add blank lines between adjacent arguments.
        /// </summary>
        /// <param name="builder">Options builders.</param>
        /// <param name="blankLineCount">Count of blank lines to add.</param>
        /// <returns>The updated options.</returns>
        public static FluentBuilder<ArgumentSetHelpOptions> BlankLinesBetweenArguments(this FluentBuilder<ArgumentSetHelpOptions> builder, int blankLineCount = 1)
        {
            builder.AddTransformer(options => options.Arguments.BlankLinesBetweenArguments = blankLineCount);
            return builder;
        }

        /// <summary>
        /// Updates default mode for display of arguments' default values.
        /// </summary>
        /// <param name="builder">Options builders.</param>
        /// <param name="mode">Desired mode.</param>
        /// <returns>The updated options.</returns>
        public static FluentBuilder<ArgumentSetHelpOptions> DefaultValues(this FluentBuilder<ArgumentSetHelpOptions> builder, ArgumentDefaultValueHelpMode mode)
        {
            builder.AddTransformer(options => options.Arguments.DefaultValue = mode);
            return builder;
        }

        /// <summary>
        /// Updates default mode for display of arguments' short names.
        /// </summary>
        /// <param name="builder">Options builders.</param>
        /// <param name="mode">Desired mode.</param>
        /// <returns>The updated options.</returns>
        public static FluentBuilder<ArgumentSetHelpOptions> ShortNames(this FluentBuilder<ArgumentSetHelpOptions> builder, ArgumentShortNameHelpMode mode)
        {
            builder.AddTransformer(options => options.Arguments.ShortName = mode);
            return builder;
        }

        /// <summary>
        /// Updates help options to use a one-column layout.
        /// </summary>
        /// <param name="builder">Options builders.</param>
        /// <returns>The updated options.</returns>
        public static FluentBuilder<ArgumentSetHelpOptions> OneColumnLayout(this FluentBuilder<ArgumentSetHelpOptions> builder)
        {
            builder.AddTransformer(options => options.Arguments.Layout = new OneColumnArgumentHelpLayout());
            return builder;
        }

        /// <summary>
        /// Updates help options to use a two-column layout.
        /// </summary>
        /// <param name="builder">Options builders.</param>
        /// <returns>The updated options.</returns>
        public static FluentBuilder<ArgumentSetHelpOptions> TwoColumnLayout(this FluentBuilder<ArgumentSetHelpOptions> builder)
        {
            builder.AddTransformer(options => options.Arguments.Layout = new TwoColumnArgumentHelpLayout());
            return builder;
        }

        /// <summary>
        /// Updates help options to use the given column widths.
        /// </summary>
        /// <param name="builder">Options builders.</param>
        /// <param name="widths">Column widths.</param>
        /// <returns>The updated options.</returns>
        public static FluentBuilder<ArgumentSetHelpOptions> ColumnWidths(this FluentBuilder<ArgumentSetHelpOptions> builder, params int[] widths)
        {
            builder.AddTransformer(options =>
            {
                if (options.Arguments.Layout is TwoColumnArgumentHelpLayout layout)
                {
                    if (widths.Length > 2)
                    {
                        throw new NotSupportedException("May only specify up to 2 column widths with this layout");
                    }

                    if (widths.Length >= 1) layout.ColumnWidths[0] = widths[0];
                    if (widths.Length >= 2) layout.ColumnWidths[1] = widths[1];
                }
                else
                {
                    throw new NotSupportedException("Cannot specify column widths on this layout");
                }
            });

            return builder;
        }

        /// <summary>
        /// Updates help options to use the given column widths.
        /// </summary>
        /// <param name="builder">Options builders.</param>
        /// <param name="defaultSeparator">Default separator to use.</param>
        /// <param name="firstLineSeparator">Separator to use only on first line,
        /// or null to have default used on first line as well.</param>
        /// <returns>The updated options.</returns>
        public static FluentBuilder<ArgumentSetHelpOptions> ColumnSeparator(this FluentBuilder<ArgumentSetHelpOptions> builder, string defaultSeparator, string firstLineSeparator = null)
        {
            builder.AddTransformer(options =>
            {
                if (options.Arguments.Layout is TwoColumnArgumentHelpLayout layout)
                {
                    layout.DefaultColumnSeparator = defaultSeparator;
                    layout.FirstLineColumnSeparator = firstLineSeparator;
                }
                else
                {
                    throw new NotSupportedException("Cannot specify column widths on this layout");
                }
            });

            return builder;
        }
    }
}
