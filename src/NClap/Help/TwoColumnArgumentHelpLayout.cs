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
        private TwoColumnArgumentHelpLayout(TwoColumnArgumentHelpLayout other) : base(other)
        {
            for (var i = 0; i < ColumnWidths.Length; ++i)
            {
                ColumnWidths[i] = other.ColumnWidths[i];
            }

            ColumnSeparator = this.ColumnSeparator;
        }

        /// <summary>
        /// Optional maximum widths of columns; null indicates no preference.
        /// </summary>
        public int?[] ColumnWidths = new int?[2] { null, null };

        /// <summary>
        /// Separator string between columns.
        /// </summary>
        public string ColumnSeparator { get; set; } = " - ";

        /// <summary>
        /// Create a separate clone of this object.
        /// </summary>
        /// <returns>Clone.</returns>
        public override ArgumentHelpLayout DeepClone() =>
            new TwoColumnArgumentHelpLayout(this);
    }
}
