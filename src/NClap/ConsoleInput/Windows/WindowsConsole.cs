using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace NClap.ConsoleInput.Windows
{
    /// <summary>
    /// Implementation of console I/O using the Windows console.
    /// </summary>
    internal sealed class WindowsConsole : BasicConsole
    {
        /// <summary>
        /// Indicates if the console's buffer is scrollable.
        /// </summary>
        public override bool IsScrollable => true;

        /// <summary>
        /// Scrolls the bottom-most lines of the console's buffer upward within
        /// the buffer by the specified number of lines, effectively freeing up
        /// the specified number of lines.  The cursor is adjusted appropriately
        /// upward by the same number of lines.
        /// </summary>
        /// <param name="lineCount">The number of lines by which to scroll the
        /// contents.</param>
        /// <exception cref="Win32Exception">Thrown when an internal error
        /// occurs.</exception>
        public override void ScrollContents(int lineCount)
        {
            if (lineCount < 0) throw new ArgumentOutOfRangeException(nameof(lineCount));
            if (lineCount == 0) return;

            var fill = new NativeMethods.CharInfo
            {
                UnicodeChar = ' ',
                Attributes = TranslateBackgroundColor(BackgroundColor)
            };

            var destOrigin = new NativeMethods.Coord { X = 0, Y = 0 };

            var scrollRect = new NativeMethods.SmallRect
            {
                Left = 0,
                Top = (short)lineCount,
                Right = (short)(BufferWidth - 1),
                Bottom = (short)(BufferHeight - 1)
            };

            var clipRect = new NativeMethods.SmallRect
            {
                Left = 0,
                Top = 0,
                Right = (short)(BufferWidth - 1),
                Bottom = (short)(BufferHeight - 1)
            };

            var handle = NativeMethods.GetStdHandle(NativeMethods.StandardHandleType.Output);
            if (!NativeMethods.ScrollConsoleScreenBuffer(handle, ref scrollRect, ref clipRect, destOrigin, ref fill))
            {
                var lastError = Marshal.GetLastWin32Error();
                if (lastError == (int)NativeMethods.Error.InvalidHandle)
                {
                    throw new NotSupportedException();
                }

                throw new Win32Exception(lastError);
            }

            CursorTop -= lineCount;
        }

        /// <summary>
        /// Translates the provided console color to native character
        /// attributes.
        /// </summary>
        /// <param name="color">The color to translate.</param>
        /// <returns>The translated native attributes.</returns>
        internal static NativeMethods.CharAttributes TranslateBackgroundColor(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return NativeMethods.CharAttributes.BackgroundBlack;
                case ConsoleColor.DarkBlue:
                    return NativeMethods.CharAttributes.BackgroundDarkBlue;
                case ConsoleColor.DarkGreen:
                    return NativeMethods.CharAttributes.BackgroundDarkGreen;
                case ConsoleColor.DarkCyan:
                    return NativeMethods.CharAttributes.BackgroundDarkCyan;
                case ConsoleColor.DarkRed:
                    return NativeMethods.CharAttributes.BackgroundDarkRed;
                case ConsoleColor.DarkMagenta:
                    return NativeMethods.CharAttributes.BackgroundDarkMagenta;
                case ConsoleColor.DarkYellow:
                    return NativeMethods.CharAttributes.BackgroundDarkYellow;
                case ConsoleColor.Gray:
                    return NativeMethods.CharAttributes.BackgroundGray;
                case ConsoleColor.DarkGray:
                    return NativeMethods.CharAttributes.BackgroundDarkGray;
                case ConsoleColor.Blue:
                    return NativeMethods.CharAttributes.BackgroundBlue;
                case ConsoleColor.Green:
                    return NativeMethods.CharAttributes.BackgroundGreen;
                case ConsoleColor.Cyan:
                    return NativeMethods.CharAttributes.BackgroundCyan;
                case ConsoleColor.Red:
                    return NativeMethods.CharAttributes.BackgroundRed;
                case ConsoleColor.Magenta:
                    return NativeMethods.CharAttributes.BackgroundMagenta;
                case ConsoleColor.Yellow:
                    return NativeMethods.CharAttributes.BackgroundYellow;
                case ConsoleColor.White:
                    return NativeMethods.CharAttributes.BackgroundWhite;
                default:
                    return NativeMethods.CharAttributes.None;
            }
        }
    }
}
