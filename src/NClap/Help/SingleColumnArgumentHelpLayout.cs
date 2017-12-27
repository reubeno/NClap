namespace NClap.Help
{
    /// <summary>
    /// Describes a single-column argument help layout: name(s) followed
    /// by description (if applicable) in the second.
    /// </summary>
    public class SingleColumnArgumentHelpLayout : ArgumentHelpLayout
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SingleColumnArgumentHelpLayout()
        {
        }

        /// <summary>
        /// Deeply cloning constructor.
        /// </summary>
        /// <param name="other">Template for clone.</param>
        private SingleColumnArgumentHelpLayout(SingleColumnArgumentHelpLayout other) : base(other)
        {
        }

        /// <summary>
        /// Create a separate clone of this object.
        /// </summary>
        /// <returns>Clone.</returns>
        public override ArgumentHelpLayout DeepClone() =>
            new SingleColumnArgumentHelpLayout();
    }
}
