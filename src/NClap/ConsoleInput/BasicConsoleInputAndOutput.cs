using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using NClap.Utilities;

namespace NClap.ConsoleInput
{
#pragma warning disable PC001 // TODO: API not supported on all platforms
#pragma warning disable PC003 // TODO: Native API not available in UWP

    /// <summary>
    /// Stock implementation of the IConsoleInput and IConsoleOutput interfaces.
    /// </summary>
    internal sealed class BasicConsoleInputAndOutput : IConsoleInput, IConsoleOutput
    {
        private const int _defaultCursorSize = 100;
        private bool _cursorLastKnownToBeVisible = true;

        /// <summary>
        /// Dummy constructor, present to prevent outside callers from
        /// constructing an instance of this class.
        /// </summary>
        private BasicConsoleInputAndOutput()
        {
        }

        /// <summary>
        /// Public factory method.
        /// </summary>
        /// <returns>A basic console instance.</returns>
        public static BasicConsoleInputAndOutput Default { get; } = new BasicConsoleInputAndOutput();

        /// <summary>
        /// The size of the cursor, expressed as an integral percentage.
        /// Note that this property is not faithfully or completely implemented
        /// on all platforms.
        /// </summary>
        public int CursorSize
        {
            get
            {
                try
                {
                    return Console.CursorSize;
                }
                catch (Exception ex) when (IsExceptionAcceptable(ex))
                {
                    return _defaultCursorSize;
                }
            }

            set
            {
                try
                {
                    Console.CursorSize = value;
                }

                // Setting the cursor size is not supported on all platforms,
                // so we swallow the request.  This isn't awesome, but covers
                // up for holes in the platform.
                catch (Exception ex) when (IsExceptionAcceptable(ex))
                {
                }
            }
        }

        /// <summary>
        /// True if the cursor is visible; false otherwise. Note that this
        /// property is not faithfully or completely implemented on all
        /// platforms.
        /// </summary>
        public bool CursorVisible
        {
            get
            {
                try
                {
                    return Console.CursorVisible;
                }
                catch (Exception ex) when (IsExceptionAcceptable(ex))
                {
                    // The underlying platform might not tell us if the cursor
                    // is visible, even though it supports setting the visibility.
                    // In such case, we make a possibly-incorrect guess and
                    // report what we last knew.
                    return _cursorLastKnownToBeVisible;
                }
            }

            set
            {
                Console.CursorVisible = value;
                _cursorLastKnownToBeVisible = value;
            }
        }

        /// <summary>
        /// True if Control-C is treated as a normal input character; false if
        /// it's specially handled.
        /// </summary>
        public bool TreatControlCAsInput
        {
            get => Console.TreatControlCAsInput;
            set => Console.TreatControlCAsInput = value;
        }

        /// <summary>
        /// The x-coordinate of the input cursor.
        /// </summary>
        public int CursorLeft
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        /// <summary>
        /// The y-coordinate of the input cursor.
        /// </summary>
        public int CursorTop
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }

        /// <summary>
        /// The width, in characters, of the window associated with the
        /// console.
        /// </summary>
        public int WindowWidth
        {
            get => Console.WindowWidth;
            set => Console.WindowWidth = value;
        }

        /// <summary>
        /// The width, in height, of the window associated with the
        /// console.
        /// </summary>
        public int WindowHeight
        {
            get => Console.WindowHeight;
            set => Console.WindowHeight = value;
        }

        /// <summary>
        /// The width, in characters, of the logical buffer associated with the
        /// console.
        /// </summary>
        public int BufferWidth
        {
            get => Console.BufferWidth;
            set => Console.BufferWidth = value;
        }

        /// <summary>
        /// The height, in characters, of the logical buffer associated with the
        /// console.
        /// </summary>
        public int BufferHeight
        {
            get => Console.BufferHeight;
            set => Console.BufferHeight = value;
        }

        /// <summary>
        /// The console's foreground color.
        /// </summary>
        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        /// <summary>
        /// The console's background color.
        /// </summary>
        public ConsoleColor BackgroundColor
        {
            get => Console.BackgroundColor;
            set => Console.BackgroundColor = value;
        }

        /// <summary>
        /// Reads a key press from the console.
        /// </summary>
        /// <param name="suppressEcho">True to suppress auto-echoing the key's
        /// character; false to echo it as normal.</param>
        /// <returns>Info about the press.</returns>
        public ConsoleKeyInfo ReadKey(bool suppressEcho) => Console.ReadKey(suppressEcho);

        /// <summary>
        /// Moves the cursor to the specified position.
        /// </summary>
        /// <param name="left">The new x-coordinate.</param>
        /// <param name="top">The new y-coordinate.</param>
        /// <returns>True if the move could be made; false if the requested
        /// move was invalid.</returns>
        public bool SetCursorPosition(int left, int top)
        {
            try
            {
                Console.SetCursorPosition(left, top);
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
        /// <exception cref="Win32Exception">Thrown when an internal error
        /// occurs.</exception>
        public void ScrollContents(int lineCount)
        {
            var handle = NativeMethods.GetStdHandle(NativeMethods.StandardHandleType.Output);

            var fill = new NativeMethods.CharInfo
            {
                UnicodeChar = ' ',
                Attributes = TranslateBackgroundColor(Console.BackgroundColor)
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
        /// Clears the console without moving the cursor.
        /// </summary>
        public void Clear() => Console.Clear();

        /// <summary>
        /// Writes colored text to the console.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void Write(ColoredMultistring text)
        {
            foreach (var value in text.Content)
            {
                Write(value);
            }
        }

        /// <summary>
        /// Writes colored text to the console.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void Write(ColoredString text)
        {
            if (!text.ForegroundColor.HasValue && !text.BackgroundColor.HasValue)
            {
                Write(text.Content);
                return;
            }

            var originalForegroundColor = Console.ForegroundColor;
            var originalBackgroundColor = Console.BackgroundColor;

            try
            {
                if (text.ForegroundColor.HasValue)
                {
                    Console.ForegroundColor = text.ForegroundColor.Value;
                }

                if (text.BackgroundColor.HasValue)
                {
                    Console.BackgroundColor = text.BackgroundColor.Value;
                }

                Write(text.Content);
            }
            finally
            {
                Console.ForegroundColor = originalForegroundColor;
                Console.BackgroundColor = originalBackgroundColor;
            }
        }

        /// <summary>
        /// Writes text to the console.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void Write(string text) => Console.Write(text);

        /// <summary>
        /// Writes a line of text to the console.
        /// </summary>
        /// <param name="text">The line of text to write.</param>
        public void WriteLine(string text) => Console.WriteLine(text);

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

        private static bool IsExceptionAcceptable(Exception ex) =>
            ex is PlatformNotSupportedException || ex is IOException;

        /// <summary>
        /// Wrapper for native methods.
        /// </summary>
        private static class NativeMethods
        {
            public enum StandardHandleType
            {
                Input = -10,
                Output = -11,
                Error = -12
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SmallRect
            {
                public short Left { get; set; }

                public short Top { get; set; }

                public short Right { get; set; }

                public short Bottom { get; set; }
            }

            [Flags]
            public enum CharAttributes : uint
            {
                None = 0,

                ForegroundBlue = 0x1,
                ForegroundGreen = 0x2,
                ForegroundRed = 0x4,
                ForegroundIntensity = 0x8,

                ForegroundIntenseBlue = ForegroundBlue | ForegroundIntensity,
                ForegroundIntenseGreen = ForegroundGreen | ForegroundIntensity,
                ForegroundIntenseRed = ForegroundRed | ForegroundIntensity,

                ForegroundCyan = ForegroundBlue | ForegroundGreen,
                ForegroundIntenseCyan = ForegroundCyan | ForegroundIntensity,

                ForegroundMagenta = ForegroundBlue | ForegroundRed,
                ForegroundIntenseMagenta = ForegroundMagenta | ForegroundIntensity,

                ForegroundYellow = ForegroundRed | ForegroundGreen,
                ForegroundIntenseYellow = ForegroundYellow | ForegroundIntensity,

                ForegroundWhite = ForegroundRed | ForegroundGreen | ForegroundBlue,
                ForegroundIntenseWhite = ForegroundWhite | ForegroundIntensity,

                BackgroundBlue = 0x10,
                BackgroundGreen = 0x20,
                BackgroundRed = 0x40,
                BackgroundIntensity = 0x80,

                BackgroundIntenseBlue = BackgroundBlue | BackgroundIntensity,
                BackgroundIntenseGreen = BackgroundGreen | BackgroundIntensity,
                BackgroundIntenseRed = BackgroundRed | BackgroundIntensity,

                BackgroundCyan = BackgroundBlue | BackgroundGreen,
                BackgroundIntenseCyan = BackgroundCyan | BackgroundIntensity,

                BackgroundMagenta = BackgroundBlue | BackgroundRed,
                BackgroundIntenseMagenta = BackgroundMagenta | BackgroundIntensity,

                BackgroundYellow = BackgroundRed | BackgroundGreen,
                BackgroundIntenseYellow = BackgroundYellow | BackgroundIntensity,

                BackgroundWhite = BackgroundRed | BackgroundGreen | BackgroundBlue,
                BackgroundIntenseWhite = BackgroundWhite | BackgroundIntensity,

                LeadingByte = 0x100,
                TrailingByte = 0x200,
                TopHorizontal = 0x400,
                LeftVertical = 0x800,
                RightVertical = 0x1000,
                ReverseVideo = 0x4000,
                Underscore = 0x8000
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct CharInfo
            {
                public char UnicodeChar { get; set; }

                public CharAttributes Attributes { get; set; }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct Coord
            {
                public short X { get; set; }

                public short Y { get; set; }
            }

            [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
            public static extern IntPtr GetStdHandle(StandardHandleType type);

            [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ScrollConsoleScreenBuffer(
                IntPtr consoleOutput,
                [In] ref SmallRect scrollRectangle,
                [In] ref SmallRect clipRectangle,
                Coord destinationOrigin,
                [In] ref CharInfo fill);
        }
    }
}
