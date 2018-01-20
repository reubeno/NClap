namespace NClap.Help
{
    /// <summary>
    /// Describes a two-column argument help layout: name(s) in the first
    /// column, then description (if applicable) in the second.
    /// </summary>
    public class TwoColumnArgumentHelpLayout : ArgumentHelpLayout
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public TwoColumnArgumentHelpLayout()
        {
        }

        /// <summary>
        /// Deeply cloning constructor.
        /// </summary>
        /// <param name="other">Template for clone.</param>
        private TwoColumnArgumentHelpLayout(TwoColumnArgumentHelpLayout other)
        {
            for (var i = 0; i < ColumnWidths.Length; ++i)
            {
                ColumnWidths[i] = other.ColumnWidths[i];
            }

            FirstLineColumnSeparator = this.FirstLineColumnSeparator;
            DefaultColumnSeparator = this.DefaultColumnSeparator;
        }

        /// <summary>
        /// Optional maximum widths of columns; null indicates no preference.
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public int?[] ColumnWidths { get; } = new int?[2] { null, null };
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Optionally specifies separator string to be used between columns,
        /// but only on the first line; for all subsequent lines, or in case
        /// this property is left null, <see cref="DefaultColumnSeparator"/> is otherwise
        /// used.
        /// </summary>
        public string FirstLineColumnSeparator { get; set; } = " - ";

        /// <summary>
        /// Separator string between columns.
        /// </summary>
        public string DefaultColumnSeparator { get; set; } = "   ";

        /// <summary>
        /// Create a separate clone of this object.
        /// </summary>
        /// <returns>Clone.</returns>
        public override ArgumentHelpLayout DeepClone() =>
            new TwoColumnArgumentHelpLayout(this);
    }
}
