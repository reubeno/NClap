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
        }

        /// <summary>
        /// Create a separate clone of this object.
        /// </summary>
        /// <returns>Clone.</returns>
        public override ArgumentHelpLayout DeepClone() =>
            new TwoColumnArgumentHelpLayout();
    }
}
