namespace NClap.Metadata
{
    /// <summary>
    /// Interface for interacting with any command group.
    /// </summary>
    public interface ICommandGroup
    {
        /// <summary>
        /// True if the group has a selection, false if no selection was yet
        /// made.
        /// </summary>
        bool HasSelection { get; }

        /// <summary>
        /// The enum value corresponding with the selected command, or null if no
        /// selection has yet been made.
        /// </summary>
        object Selection { get; }

        /// <summary>
        /// The command presently selected from this group, or null if no
        /// selection has yet been made.
        /// </summary>
        ICommand InstantiatedCommand { get; }
    }
}
