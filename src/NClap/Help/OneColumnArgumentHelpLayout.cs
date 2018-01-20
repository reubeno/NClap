namespace NClap.Help
{
    /// <summary>
    /// Describes a single-column argument help layout: name(s) followed
    /// by description (if applicable) in the second.
    /// </summary>
    public class OneColumnArgumentHelpLayout : ArgumentHelpLayout
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public OneColumnArgumentHelpLayout()
        {
        }

        /// <summary>
        /// Create a separate clone of this object.
        /// </summary>
        /// <returns>Clone.</returns>
        public override ArgumentHelpLayout DeepClone() => new OneColumnArgumentHelpLayout();
    }
}
