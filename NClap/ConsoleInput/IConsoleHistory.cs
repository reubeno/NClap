using System.IO;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Abstract interface for managing console history.
    /// </summary>
    public interface IConsoleHistory
    {
        /// <summary>
        /// The count of entries in the history.
        /// </summary>
        int EntryCount { get; }

        /// <summary>
        /// If the cursor is valid, the current entry in the history; null
        /// otherwise.
        /// </summary>
        string CurrentEntry { get; }

        /// <summary>
        /// Add a new entry to the end of the history, and reset the history's
        /// cursor to that new entry.
        /// </summary>
        /// <param name="entry"></param>
        void Add(string entry);

        /// <summary>
        /// Move the current history cursor by the specified offset.
        /// </summary>
        /// <param name="origin">Reference for movement.</param>
        /// <param name="offset">Positive or negative offset to apply to the
        /// specified origin.</param>
        /// <returns>True on success; false if the move could not be made.
        /// </returns>
        bool MoveCursor(SeekOrigin origin, int offset);
    }
}