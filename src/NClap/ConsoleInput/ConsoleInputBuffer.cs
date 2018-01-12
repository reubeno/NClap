using System;
using System.IO;
using NClap.Utilities;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Encapsulates the state of a console input line buffer.
    /// </summary>
    internal class ConsoleInputBuffer : IConsoleInputBuffer
    {
        private readonly IStringBuilder _buffer = StringWrapper.CreateBuilder();

        /// <summary>
        /// The current string contents of the entire buffer.
        /// </summary>
        public string Contents => _buffer.ToString();

        /// <summary>
        /// The current cursor index.
        /// </summary>
        public int CursorIndex { get; private set; }

        /// <summary>
        /// The current length of the buffer's contents.
        /// </summary>
        public int Length => _buffer.Length;

        /// <summary>
        /// True if the cursor is pointing at the end of the buffer; false
        /// otherwise.
        /// </summary>
        public bool CursorIsAtEnd => CursorIndex >= Length;

        /// <summary>
        /// Reads a character from the specified index of the buffer.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <returns>The character.</returns>
        public char this[int i] => _buffer[i];

        /// <summary>
        /// Retrieves the contents of the buffer.
        /// </summary>
        /// <returns>The contents as a string.</returns>
        public override string ToString() => Contents;

        /// <summary>
        /// Move the cursor.
        /// </summary>
        /// <param name="origin">Which origin to use for calculating the new
        /// cursor position.</param>
        /// <param name="offsetFromOrigin">The offset to apply,
        /// relative to the specified origin.</param>
        /// <returns>True on success; false if the movement could not be made.
        /// </returns>
        public bool MoveCursor(SeekOrigin origin, int offsetFromOrigin) =>
            MoveCursor(origin, offsetFromOrigin, out int offsetFromPreviousPosition);

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
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="origin"/>
        /// is not a valid origin value.</exception>
        public bool MoveCursor(SeekOrigin origin, int offsetFromOrigin, out int offsetFromPreviousPosition)
        {
            int startingIndex;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    startingIndex = 0;
                    break;

                case SeekOrigin.Current:
                    startingIndex = CursorIndex;
                    break;

                case SeekOrigin.End:
                    startingIndex = _buffer.Length;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(origin));
            }

            var newIndex = startingIndex + offsetFromOrigin;
            if ((newIndex < 0) || (newIndex > _buffer.Length))
            {
                offsetFromPreviousPosition = 0;
                return false;
            }

            offsetFromPreviousPosition = newIndex - CursorIndex;
            CursorIndex = newIndex;

            return true;
        }

        /// <summary>
        /// Read the specified number of characters from the buffer, starting
        /// at the cursor.
        /// </summary>
        /// <param name="count">The number of characters to read.</param>
        /// <returns>The read characters.</returns>
        public char[] Read(int count) => ReadAt(CursorIndex, count);

        /// <summary>
        /// Read the specified number of characters from the buffer, starting
        /// at the specified offset into the buffer.
        /// </summary>
        /// <param name="sourceIndex">The offset into the buffer at which
        /// to start reading.</param>
        /// <param name="count">The number of characters to read.</param>
        /// <returns>The read characters.</returns>
        public char[] ReadAt(int sourceIndex, int count)
        {
            var destination = new char[count];
            ReadAt(sourceIndex, destination, 0, count);

            return destination;
        }

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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="buffer"/>
        /// is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when indicated
        /// range would require reading past the end of the the input buffer or
        /// writing past the end of the provided output buffer.</exception>
        public void ReadAt(int sourceIndex, char[] buffer, int destinationIndex, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            if (sourceIndex + count > _buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (destinationIndex + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer));
            }

            _buffer.CopyTo(sourceIndex, buffer, destinationIndex, count);
        }
        
        /// <summary>
        /// Clears the entire buffer.
        /// </summary>
        public void Clear()
        {
            _buffer.Clear();
            CursorIndex = 0;
        }

        /// <summary>
        /// Remove all characters under or after the cursor.
        /// </summary>
        public void Truncate() => _buffer.Truncate(CursorIndex);

        /// <summary>
        /// Inserts a character at the cursor without moving the cursor.
        /// </summary>
        /// <param name="value">Character to insert.</param>
        public void Insert(char value) => _buffer.Insert(CursorIndex, value);

        /// <summary>
        /// Inserts a string at the cursor without moving the cursor.
        /// </summary>
        /// <param name="value">String to insert.</param>
        public void Insert(string value) => _buffer.Insert(CursorIndex, value);

        /// <summary>
        /// Replaces the character under the cursor with the specified
        /// character.
        /// </summary>
        /// <param name="value">Replacement character.</param>
        public void Replace(char value) => _buffer[CursorIndex] = value;

        /// <summary>
        /// Replaces characters starting at the one under the cursor with the
        /// characters in the specified string.  An exception is thrown if
        /// the string value provided contains more characters than fit in the
        /// remainder of the string.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/>
        /// is null.</exception>
        public void Replace(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (CursorIndex + value.Length > _buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            for (var index = 0; index < value.Length; ++index)
            {
                _buffer[CursorIndex + index] = value[index];
            }
        }

        /// <summary>
        /// Removes the character under the cursor.
        /// </summary>
        /// <returns>True on success; false if the cursor was at the end of the
        /// buffer.</returns>
        public bool Remove() => Remove(1);

        /// <summary>
        /// Removes the specified number of characters from the buffer, starting
        /// with the character under the cursor.  The cursor's location is not
        /// affected by this operation.
        /// </summary>
        /// <param name="count">The number of characters to remove.</param>
        /// <returns>True on success; false if the characters could not be
        /// removed.</returns>
        public bool Remove(int count)
        {
            if (CursorIndex + count > _buffer.Length)
            {
                return false;
            }

            _buffer.Remove(CursorIndex, count);
            return true;
        }

        /// <summary>
        /// Removes the character before the cursor, and moves the cursor
        /// appropriately.
        /// </summary>
        /// <returns>True on success; false if the cursor was at the beginning
        /// of the buffer.</returns>
        public bool RemoveCharBeforeCursor()
        {
            if (CursorIndex == 0)
            {
                return false;
            }

            --CursorIndex;
            return Remove();
        }
    }
}
