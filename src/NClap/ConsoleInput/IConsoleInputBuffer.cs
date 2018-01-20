using System.IO;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Abstract interface for interacting with a console input buffer.
    /// </summary>
    public interface IConsoleInputBuffer
    {
        /// <summary>
        /// The current string contents of the entire buffer.
        /// </summary>
        string Contents { get; }

        /// <summary>
        /// The current cursor index.
        /// </summary>
        int CursorIndex { get; }

        /// <summary>
        /// The current length of the buffer's contents.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// True if the cursor is pointing at the end of the buffer; false
        /// otherwise.
        /// </summary>
        bool CursorIsAtEnd { get; }

        /// <summary>
        /// Reads a character from the specified index of the buffer.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The character.</returns>
        char this[int index] { get; }

        /// <summary>
        /// Move the cursor.
        /// </summary>
        /// <param name="origin">Which origin to use for calculating the new
        /// cursor position.</param>
        /// <param name="offsetFromOrigin">The offset to apply,
        /// relative to the specified origin.</param>
        /// <returns>True on success; false if the movement could not be made.
        /// </returns>
        bool MoveCursor(SeekOrigin origin, int offsetFromOrigin);

        /// <summary>
        /// Move the cursor.
        /// </summary>
        /// <param name="origin">Which origin to use for calculating the new
        /// cursor position.</param>
        /// <param name="offsetFromOrigin">The offset to apply,
        /// relative to the specified origin.</param>
        /// <param name="offsetFromPreviousPosition">On success, receives the
        /// resulting offset moved from the previous position.</param>
        /// <returns>True on success; false if the movement could not be made.
        /// </returns>
        bool MoveCursor(SeekOrigin origin, int offsetFromOrigin, out int offsetFromPreviousPosition);

        /// <summary>
        /// Read the specified number of characters from the buffer, starting
        /// at the cursor.
        /// </summary>
        /// <param name="count">The number of characters to read.</param>
        /// <returns>The read characters.</returns>
        char[] Read(int count);

        /// <summary>
        /// Read the specified number of characters from the buffer, starting
        /// at the specified offset into the buffer.
        /// </summary>
        /// <param name="sourceIndex">The offset into the buffer at which
        /// to start reading.</param>
        /// <param name="count">The number of characters to read.</param>
        /// <returns>The read characters.</returns>
        char[] ReadAt(int sourceIndex, int count);

        /// <summary>
        /// Read the specified number of characters from the buffer, starting
        /// at the specified offset into the buffer and writing to the specified
        /// offset in the destination buffer.
        /// </summary>
        /// <param name="sourceIndex">The offset into the buffer at which
        /// to start reading.</param>
        /// <param name="buffer">The buffer to write the characters to.</param>
        /// <param name="destinationIndex">The index in the destination buffer
        /// at which to start writing.</param>
        /// <param name="count">The number of characters to read.</param>
        void ReadAt(int sourceIndex, char[] buffer, int destinationIndex, int count);

        /// <summary>
        /// Clears the entire buffer.
        /// </summary>
        void Clear();

        /// <summary>
        /// Remove all characters under or after the cursor.
        /// </summary>
        void Truncate();

        /// <summary>
        /// Inserts a character at the cursor without moving the cursor.
        /// </summary>
        /// <param name="value">Character to insert.</param>
        void Insert(char value);

        /// <summary>
        /// Inserts a string at the cursor without moving the cursor.
        /// </summary>
        /// <param name="value">String to insert.</param>
        void Insert(string value);

        /// <summary>
        /// Replaces the character under the cursor with the specified
        /// character.
        /// </summary>
        /// <param name="value">Replacement character.</param>
        void Replace(char value);

        /// <summary>
        /// Replaces characters starting at the one under the cursor with the
        /// characters in the specified string.  An exception is thrown if
        /// the string value provided contains more characters than fit in the
        /// remainder of the string.
        /// </summary>
        /// <param name="value">Replacement value.</param>
        void Replace(string value);

        /// <summary>
        /// Removes the character under the cursor.
        /// </summary>
        /// <returns>True on success; false if the cursor was at the end of the
        /// buffer.</returns>
        bool Remove();

        /// <summary>
        /// Removes the specified number of characters from the buffer, starting
        /// with the character under the cursor.  The cursor's location is not
        /// affected by this operation.
        /// </summary>
        /// <param name="count">The number of characters to remove.</param>
        /// <returns>True on success; false if the characters could not be
        /// removed.</returns>
        bool Remove(int count);

        /// <summary>
        /// Removes the character before the cursor, and moves the cursor
        /// appropriately.
        /// </summary>
        /// <returns>True on success; false if the cursor was at the beginning
        /// of the buffer.</returns>
        bool RemoveCharBeforeCursor();
    }
}