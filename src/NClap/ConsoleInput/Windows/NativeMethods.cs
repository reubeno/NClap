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
            ForegroundBlue = 0x1,

            /// <summary>
            /// Foreground color is green.
            /// </summary>
            ForegroundGreen = 0x2,

            /// <summary>
            /// Foreground color is red.
            /// </summary>
            ForegroundRed = 0x4,

            /// <summary>
            /// Foreground color is modified to be a more intense version of itself.
            /// </summary>
            ForegroundIntensity = 0x8,

            /// <summary>
            /// Foreground color is intense blue.
            /// </summary>
            ForegroundIntenseBlue = ForegroundBlue | ForegroundIntensity,

            /// <summary>
            /// Foreground color is intense green.
            /// </summary>
            ForegroundIntenseGreen = ForegroundGreen | ForegroundIntensity,

            /// <summary>
            /// Foreground color is intense red.
            /// </summary>
            ForegroundIntenseRed = ForegroundRed | ForegroundIntensity,

            /// <summary>
            /// Foreground color is cyan.
            /// </summary>
            ForegroundCyan = ForegroundBlue | ForegroundGreen,

            /// <summary>
            /// Foreground color is intense cyan.
            /// </summary>
            ForegroundIntenseCyan = ForegroundCyan | ForegroundIntensity,

            /// <summary>
            /// Foreground color is magenta.
            /// </summary>
            ForegroundMagenta = ForegroundBlue | ForegroundRed,

            /// <summary>
            /// Foreground color is intense magenta.
            /// </summary>
            ForegroundIntenseMagenta = ForegroundMagenta | ForegroundIntensity,

            /// <summary>
            /// Foreground color is yellow.
            /// </summary>
            ForegroundYellow = ForegroundRed | ForegroundGreen,

            /// <summary>
            /// Foreground color is intense yellow.
            /// </summary>
            ForegroundIntenseYellow = ForegroundYellow | ForegroundIntensity,

            /// <summary>
            /// Foreground color is white.
            /// </summary>
            ForegroundWhite = ForegroundRed | ForegroundGreen | ForegroundBlue,

            /// <summary>
            /// Foreground color is intense white.
            /// </summary>
            ForegroundIntenseWhite = ForegroundWhite | ForegroundIntensity,

            /// <summary>
            /// Background color is blue.
            /// </summary>
            BackgroundBlue = 0x10,

            /// <summary>
            /// Background color is green.
            /// </summary>
            BackgroundGreen = 0x20,

            /// <summary>
            /// Background color is red.
            /// </summary>
            BackgroundRed = 0x40,

            /// <summary>
            /// Background color is modified to be a more intense version of itself.
            /// </summary>
            BackgroundIntensity = 0x80,

            /// <summary>
            /// Background color is intense blue.
            /// </summary>
            BackgroundIntenseBlue = BackgroundBlue | BackgroundIntensity,

            /// <summary>
            /// Background color is intense green.
            /// </summary>
            BackgroundIntenseGreen = BackgroundGreen | BackgroundIntensity,

            /// <summary>
            /// Background color is intense red.
            /// </summary>
            BackgroundIntenseRed = BackgroundRed | BackgroundIntensity,

            /// <summary>
            /// Background color is cyan.
            /// </summary>
            BackgroundCyan = BackgroundBlue | BackgroundGreen,

            /// <summary>
            /// Background color is intense cyan.
            /// </summary>
            BackgroundIntenseCyan = BackgroundCyan | BackgroundIntensity,

            /// <summary>
            /// Background color is magenta.
            /// </summary>
            BackgroundMagenta = BackgroundBlue | BackgroundRed,

            /// <summary>
            /// Background color is intense magenta.
            /// </summary>
            BackgroundIntenseMagenta = BackgroundMagenta | BackgroundIntensity,

            /// <summary>
            /// Background color is yellow.
            /// </summary>
            BackgroundYellow = BackgroundRed | BackgroundGreen,

            /// <summary>
            /// Background color is intense yellow.
            /// </summary>
            BackgroundIntenseYellow = BackgroundYellow | BackgroundIntensity,

            /// <summary>
            /// Background color is white.
            /// </summary>
            BackgroundWhite = BackgroundRed | BackgroundGreen | BackgroundBlue,

            /// <summary>
            /// Background color is intense white.
            /// </summary>
            BackgroundIntenseWhite = BackgroundWhite | BackgroundIntensity,

            /// <summary>
            /// (unknown)
            /// </summary>
            LeadingByte = 0x100,

            /// <summary>
            /// (unknown)
            /// </summary>
            TrailingByte = 0x200,

            /// <summary>
            /// (unknown)
            /// </summary>
            TopHorizontal = 0x400,

            /// <summary>
            /// (unknown)
            /// </summary>
            LeftVertical = 0x800,

            /// <summary>
            /// (unknown)
            /// </summary>
            RightVertical = 0x1000,

            /// <summary>
            /// Enable reverse video (inverse color).
            /// </summary>
            ReverseVideo = 0x4000,

            /// <summary>
            /// (unknown)
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
