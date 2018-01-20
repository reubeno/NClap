using NClap.Utilities;

namespace NClap.Help
{
    /// <summary>
    /// Abstract base class for an argument help layout.
    /// </summary>
    public abstract class ArgumentHelpLayout : IDeepCloneable<ArgumentHelpLayout>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected ArgumentHelpLayout()
        {
        }

        /// <summary>
        /// Create a separate clone of this object.
        /// </summary>
        /// <returns>Clone.</returns>
        public abstract ArgumentHelpLayout DeepClone();
    }
}
