using System;
using System.Collections.Generic;

using NClap.ConsoleInput;
using NClap.Utilities;

namespace NClap.Tests.ConsoleInput
{
    class SimulatedConsoleOutput : IConsoleOutput
    {
        private readonly int _bufferWidth;
        private readonly int _bufferHeight;
        private readonly char[] _buffer;
        private int _cursorLeft;
        private int _cursorTop;

        public SimulatedConsoleOutput(int width = 80, int height = 24)
        {
            _bufferWidth = width;
            _bufferHeight = height;
            _buffer = new char[width * height];
            Clear();
        }

        /// <summary>
        /// The size of the cursor, expressed as an integral percentage.
        /// </summary>
        public int CursorSize { get; set; }

        /// <summary>
        /// True if the cursor is visible; false otherwise.
        /// </summary>
        public bool CursorVisible { get; set; }

        /// <summary>
        /// The x-coordinate of the input cursor.
        /// </summary>
        public int CursorLeft
        {
            get => _cursorLeft;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (value >= BufferWidth)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _cursorLeft = value;
            }
        }

        /// <summary>
        /// The y-coordinate of the input cursor.
        /// </summary>
        public int CursorTop
        {
            get => _cursorTop;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (value >= BufferHeight)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _cursorTop = value;
            }
        }

        /// <summary>
        /// The width, in characters, of the window associated with the
        /// console.
        /// </summary>
        public int WindowWidth
        {
            get => _bufferWidth;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// The width, in characters, of the window associated with the
        /// console.
        /// </summary>
        public int WindowHeight
        {
            get => _bufferHeight;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// The width, in characters, of the logical buffer associated with the
        /// console.
        /// </summary>
        public int BufferWidth
        {
            get => _bufferWidth;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// The height, in characters, of the logical buffer associated with the
        /// console.
        /// </summary>
        public int BufferHeight
        {
            get => _bufferHeight;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// The console's foreground color.
        /// </summary>
        public ConsoleColor ForegroundColor { get; set; }

        /// <summary>
        /// The console's background color.
        /// </summary>
        public ConsoleColor BackgroundColor { get; set; }

        /// <summary>
        /// Moves the cursor to the specified position.
        /// </summary>
        /// <param name="left">The new x-coordinate.</param>
        /// <param name="top">The new y-coordinate.</param>
        public bool SetCursorPosition(int left, int top)
        {
            try
            {
                CursorLeft = left;
                CursorTop = top;

                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        /// <summary>
        /// Scrolls the bottom-most lines of the console's buffer upward within
        /// the buffer by the specified number of lines, effectively freeing up
        /// the specified number of lines.  The cursor is adjusted appropriately
        /// upward by the same number of lines.
        /// </summary>
        /// <param name="lineCount">The number of lines by which to scroll the
        /// contents.</param>
        public void ScrollContents(int lineCount)
        {
            if (lineCount > BufferHeight)
            {
                throw new ArgumentOutOfRangeException(nameof(lineCount));
            }

            var copiedBuffer = new char[BufferHeight * BufferWidth];

            var linesToCopy = BufferHeight - lineCount;
            if (linesToCopy > 0)
            {
                Array.Copy(_buffer, lineCount * BufferWidth, copiedBuffer, 0, linesToCopy * BufferWidth);
            }

            copiedBuffer.CopyTo(_buffer, 0);

            CursorTop -= lineCount;
        }

        /// <summary>
        /// Clears the console without moving the cursor.
        /// </summary>
        public void Clear()
        {
            for (var index = 0; index < _buffer.Length; ++index)
            {
                _buffer[index] = '\0';
            }
        }

        /// <summary>
        /// Writes a character to the console.
        /// </summary>
        /// <param name="value">The character to write.</param>
        public void Write(char value)
        {
            var currentOffset = (CursorTop * BufferWidth) + CursorLeft;

            switch (value)
            {
                case '\n':
                    currentOffset -= CursorLeft;
                    currentOffset += BufferWidth;
                    break;

                case '\r':
                    currentOffset -= CursorLeft;
                    break;

                default:
                    _buffer[currentOffset] = value;
                    ++currentOffset;
                    break;
            }

            var lineIndex = currentOffset / BufferWidth;
            var columnIndex = currentOffset % BufferWidth;

            if (lineIndex >= BufferHeight)
            {
                ScrollContents(lineIndex - BufferHeight + 1);
                lineIndex = BufferHeight - 1;
            }

            CursorLeft = columnIndex;
            CursorTop = lineIndex;
        }

        /// <summary>
        /// Writes colored text to the console.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void Write(ColoredString text)
        {
            Write(text.Content);
        }

        /// <summary>
        /// Writes text to the console.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void Write(string text)
        {
            foreach (var c in text)
            {
                Write(c);
            }
        }

        /// <summary>
        /// Writes a line of text to the console.
        /// </summary>
        /// <param name="text">The line of text to write.</param>
        public void WriteLine(string text)
        {
            Write(text);
            Write('\n');
        }

        /// <summary>
        /// The simulated output buffer.
        /// </summary>
        public IReadOnlyList<char> OutputBuffer => _buffer;
    }
}
