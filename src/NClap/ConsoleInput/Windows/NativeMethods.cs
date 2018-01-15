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
