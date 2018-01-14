using System;
using NClap.Utilities;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Abstract interface for interacting with an output console.
    /// </summary>
    public interface IConsoleOutput
    {
        /// <summary>
        /// The size of the cursor, expressed as an integral percentage.
        /// </summary>
        int CursorSize { get; set; }

        /// <summary>
        /// True if the cursor is visible; false otherwise.
        /// </summary>
        bool CursorVisible { get; set; }

        /// <summary>
        /// The x-coordinate of the input cursor.
        /// </summary>
        int CursorLeft { get; set; }

        /// <summary>
        /// The y-coordinate of the input cursor.
        /// </summary>
        int CursorTop { get; set; }

        /// <summary>
        /// The width, in characters, of the window associated with the
        /// console.
        /// </summary>
        int WindowWidth { get; set; }

        /// <summary>
        /// The height, in characters, of the window associated with the
        /// console.
        /// </summary>
        int WindowHeight { get; set; }

        /// <summary>
        /// The width, in characters, of the logical buffer associated with the
        /// console.
        /// </summary>
        int BufferWidth { get; set; }

        /// <summary>
        /// The height, in characters, of the logical buffer associated with the
        /// console.
        /// </summary>
        int BufferHeight { get; set; }

        /// <summary>
        /// The console's foreground color.
        /// </summary>
        ConsoleColor ForegroundColor { get; set; }

        /// <summary>
        /// The console's background color.
        /// </summary>
        ConsoleColor BackgroundColor { get; set; }

        /// <summary>
        /// Moves the cursor to the specified position.
        /// </summary>
        /// <param name="left">The new x-coordinate.</param>
        /// <param name="top">The new y-coordinate.</param>
        /// <returns>True if the move could be made; false if the requested
        /// move was invalid.</returns>
        bool SetCursorPosition(int left, int top);

        /// <summary>
        /// Scrolls the bottom-most lines of the console's buffer upward within
        /// the buffer by the specified number of lines, effectively freeing up
        /// the specified number of lines.  The cursor is adjusted appropriately
        /// upward by the same number of lines.
        /// </summary>
        /// <param name="lineCount">The number of lines by which to scroll the
        /// contents.</param>
        void ScrollContents(int lineCount);

        /// <summary>
        /// Clears the console without moving the cursor.
        /// </summary>
        void Clear();

        /// <summary>
        /// Writes colored text to the console.
        /// </summary>
        /// <param name="text">The text to write.</param>
        void Write(ColoredString text);

        /// <summary>
        /// Writes text to the console.
        /// </summary>
        /// <param name="text">The text to write.</param>
        void Write(string text);

        /// <summary>
        /// Writes a line of text to the console.
        /// </summary>
        /// <param name="text">The line of text to write.</param>
        void WriteLine(string text);
    }
}
