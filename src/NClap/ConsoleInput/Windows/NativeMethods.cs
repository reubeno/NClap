using System;
using System.Runtime.InteropServices;

namespace NClap.ConsoleInput.Windows
{
#pragma warning disable PC003 // TODO: Native API not available in UWP

    /// <summary>
    /// Wrapper for native methods only available on traditional Windows platforms.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// Win32 error.
        /// </summary>
        public enum Error
        {
            /// <summary>
            /// An operation was attempted against an invalid handle.
            /// </summary>
            InvalidHandle = 6
        }

        /// <summary>
        /// Enumeration to encapsulate the possible arguments to <see cref="GetStdHandle"/>.
        /// </summary>
        public enum StandardHandleType
        {
            /// <summary>
            /// Standard input (a.k.a. stdin).
            /// </summary>
            Input = -10,

            /// <summary>
            /// Standard output (a.k.a. stdout).
            /// </summary>
            Output = -11,

            /// <summary>
            /// Standard error (a.k.a. stderr).
            /// </summary>
            Error = -12
        }

        /// <summary>
        /// Type to wrap native SMALL_RECT structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SmallRect
        {
            /// <summary>
            /// x-coordinate of rectangle's left edge.
            /// </summary>
            public short Left { get; set; }

            /// <summary>
            /// y-coordinate of rectangle's top edge.
            /// </summary>
            public short Top { get; set; }

            /// <summary>
            /// x-coordinate of rectangle's right edge.
            /// </summary>
            public short Right { get; set; }

            /// <summary>
            /// y-coordinate of rectangle's bottom edge.
            /// </summary>
            public short Bottom { get; set; }
        }

        /// <summary>
        /// Type to describe possible character attributes specifiable in
        /// the CHAR_INFO structure.
        /// </summary>
        [Flags]
        public enum CharAttributes : uint
        {
            /// <summary>
            /// No attributes.
            /// </summary>
            None = 0,

            /// <summary>
            /// Foreground color is blue.
            /// </summary>
            ForegroundDarkBlue = 0x1,

            /// <summary>
            /// Foreground color is green.
            /// </summary>
            ForegroundDarkGreen = 0x2,

            /// <summary>
            /// Foreground color is red.
            /// </summary>
            ForegroundDarkRed = 0x4,

            /// <summary>
            /// Foreground color is modified to be a more intense version of itself.
            /// </summary>
            ForegroundIntensity = 0x8,

            /// <summary>
            /// Foreground color is black.
            /// </summary>
            ForegroundBlack = None,

            /// <summary>
            /// Foreground color is dark gray.
            /// </summary>
            ForegroundDarkGray = ForegroundBlack | ForegroundIntensity,

            /// <summary>
            /// Foreground color is  blue.
            /// </summary>
            ForegroundBlue = ForegroundDarkBlue | ForegroundIntensity,

            /// <summary>
            /// Foreground color is  green.
            /// </summary>
            ForegroundGreen = ForegroundDarkGreen | ForegroundIntensity,

            /// <summary>
            /// Foreground color is  red.
            /// </summary>
            ForegroundRed = ForegroundDarkRed | ForegroundIntensity,

            /// <summary>
            /// Foreground color is cyan.
            /// </summary>
            ForegroundDarkCyan = ForegroundDarkBlue | ForegroundDarkGreen,

            /// <summary>
            /// Foreground color is  cyan.
            /// </summary>
            ForegroundCyan = ForegroundDarkCyan | ForegroundIntensity,

            /// <summary>
            /// Foreground color is magenta.
            /// </summary>
            ForegroundDarkMagenta = ForegroundDarkBlue | ForegroundDarkRed,

            /// <summary>
            /// Foreground color is  magenta.
            /// </summary>
            ForegroundMagenta = ForegroundDarkMagenta | ForegroundIntensity,

            /// <summary>
            /// Foreground color is yellow.
            /// </summary>
            ForegroundDarkYellow = ForegroundDarkRed | ForegroundDarkGreen,

            /// <summary>
            /// Foreground color is  yellow.
            /// </summary>
            ForegroundYellow = ForegroundDarkYellow | ForegroundIntensity,

            /// <summary>
            /// Foreground color is gray.
            /// </summary>
            ForegroundGray = ForegroundDarkRed | ForegroundDarkGreen | ForegroundDarkBlue,

            /// <summary>
            /// Foreground color is white.
            /// </summary>
            ForegroundWhite = ForegroundGray | ForegroundIntensity,

            /// <summary>
            /// Background color is blue.
            /// </summary>
            BackgroundDarkBlue = 0x10,

            /// <summary>
            /// Background color is green.
            /// </summary>
            BackgroundDarkGreen = 0x20,

            /// <summary>
            /// Background color is red.
            /// </summary>
            BackgroundDarkRed = 0x40,

            /// <summary>
            /// Background color is modified to be a more intense version of itself.
            /// </summary>
            BackgroundIntensity = 0x80,

            /// <summary>
            /// Background color is black.
            /// </summary>
            BackgroundBlack = None,

            /// <summary>
            /// Background color is dark gray.
            /// </summary>
            BackgroundDarkGray = BackgroundBlack | BackgroundIntensity,

            /// <summary>
            /// Background color is  blue.
            /// </summary>
            BackgroundBlue = BackgroundDarkBlue | BackgroundIntensity,

            /// <summary>
            /// Background color is  green.
            /// </summary>
            BackgroundGreen = BackgroundDarkGreen | BackgroundIntensity,

            /// <summary>
            /// Background color is  red.
            /// </summary>
            BackgroundRed = BackgroundDarkRed | BackgroundIntensity,

            /// <summary>
            /// Background color is cyan.
            /// </summary>
            BackgroundDarkCyan = BackgroundDarkBlue | BackgroundDarkGreen,

            /// <summary>
            /// Background color is  cyan.
            /// </summary>
            BackgroundCyan = BackgroundDarkCyan | BackgroundIntensity,

            /// <summary>
            /// Background color is magenta.
            /// </summary>
            BackgroundDarkMagenta = BackgroundDarkBlue | BackgroundDarkRed,

            /// <summary>
            /// Background color is  magenta.
            /// </summary>
            BackgroundMagenta = BackgroundDarkMagenta | BackgroundIntensity,

            /// <summary>
            /// Background color is yellow.
            /// </summary>
            BackgroundDarkYellow = BackgroundDarkRed | BackgroundDarkGreen,

            /// <summary>
            /// Background color is  yellow.
            /// </summary>
            BackgroundYellow = BackgroundDarkYellow | BackgroundIntensity,

            /// <summary>
            /// Background color is gray.
            /// </summary>
            BackgroundGray = BackgroundDarkRed | BackgroundDarkGreen | BackgroundDarkBlue,

            /// <summary>
            /// Background color is white.
            /// </summary>
            BackgroundWhite = BackgroundGray | BackgroundIntensity,

            /// <summary>
            /// (unknown).
            /// </summary>
            LeadingByte = 0x100,

            /// <summary>
            /// (unknown).
            /// </summary>
            TrailingByte = 0x200,

            /// <summary>
            /// (unknown).
            /// </summary>
            TopHorizontal = 0x400,

            /// <summary>
            /// (unknown).
            /// </summary>
            LeftVertical = 0x800,

            /// <summary>
            /// (unknown).
            /// </summary>
            RightVertical = 0x1000,

            /// <summary>
            /// Enable reverse video (inverse color).
            /// </summary>
            ReverseVideo = 0x4000,

            /// <summary>
            /// (unknown).
            /// </summary>
            Underscore = 0x8000
        }

        /// <summary>
        /// Type to wrap native CHAR_INFO structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CharInfo
        {
            /// <summary>
            /// Unicode character.
            /// </summary>
            public char UnicodeChar { get; set; }

            /// <summary>
            /// Display attributes for character (e.g. color).
            /// </summary>
            public CharAttributes Attributes { get; set; }
        }

        /// <summary>
        /// Type to wrap native COORD structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            /// <summary>
            /// X coordinate.
            /// </summary>
            public short X { get; set; }

            /// <summary>
            /// Y coordinate.
            /// </summary>
            public short Y { get; set; }
        }

        /// <summary>
        /// Native routine for retrieving a standard console handle.
        /// </summary>
        /// <param name="type">Type of handle to retrieve.</param>
        /// <returns>The handle, if present; IntPtr.Zero otherwise.</returns>
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern IntPtr GetStdHandle(StandardHandleType type);

        /// <summary>
        /// Native routine for scrolling the console's screen buffer.
        /// </summary>
        /// <param name="consoleOutput">Handle to console output.</param>
        /// <param name="scrollRectangle">Rectangle for scrolling.</param>
        /// <param name="clipRectangle">Rectangle for clipping.</param>
        /// <param name="destinationOrigin">Destination origin coordinates.</param>
        /// <param name="fill">Character to fill freed space.</param>
        /// <returns>true on success; false otherwise.</returns>
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
