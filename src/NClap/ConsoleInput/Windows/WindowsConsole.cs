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
            var handle = NativeMethods.GetStdHandle(NativeMethods.StandardHandleType.Output);

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

            if (!NativeMethods.ScrollConsoleScreenBuffer(handle, ref scrollRect, ref clipRect, destOrigin, ref fill))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            CursorTop -= lineCount;
        }

        /// <summary>
        /// Translates the provided console color to native character
        /// attributes.
        /// </summary>
        /// <param name="color">The color to translate.</param>
        /// <returns>The translated native attributes.</returns>
        private static NativeMethods.CharAttributes TranslateBackgroundColor(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return NativeMethods.CharAttributes.None;
                case ConsoleColor.DarkBlue:
                    return NativeMethods.CharAttributes.BackgroundBlue;
                case ConsoleColor.DarkGreen:
                    return NativeMethods.CharAttributes.BackgroundGreen;
                case ConsoleColor.DarkCyan:
                    return NativeMethods.CharAttributes.BackgroundCyan;
                case ConsoleColor.DarkRed:
                    return NativeMethods.CharAttributes.BackgroundRed;
                case ConsoleColor.DarkMagenta:
                    return NativeMethods.CharAttributes.BackgroundMagenta;
                case ConsoleColor.DarkYellow:
                    return NativeMethods.CharAttributes.BackgroundYellow;
                case ConsoleColor.Gray:
                    return NativeMethods.CharAttributes.BackgroundWhite;
                case ConsoleColor.DarkGray:
                    return NativeMethods.CharAttributes.BackgroundWhite;
                case ConsoleColor.Blue:
                    return NativeMethods.CharAttributes.BackgroundIntenseBlue;
                case ConsoleColor.Green:
                    return NativeMethods.CharAttributes.BackgroundIntenseGreen;
                case ConsoleColor.Cyan:
                    return NativeMethods.CharAttributes.BackgroundIntenseCyan;
                case ConsoleColor.Red:
                    return NativeMethods.CharAttributes.BackgroundIntenseRed;
                case ConsoleColor.Magenta:
                    return NativeMethods.CharAttributes.BackgroundIntenseMagenta;
                case ConsoleColor.Yellow:
                    return NativeMethods.CharAttributes.BackgroundIntenseYellow;
                case ConsoleColor.White:
                    return NativeMethods.CharAttributes.BackgroundIntenseWhite;
                default:
                    return NativeMethods.CharAttributes.None;
            }
        }
    }
}
