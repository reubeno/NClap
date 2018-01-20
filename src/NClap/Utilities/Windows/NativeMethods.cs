using System;
using System.Runtime.InteropServices;

namespace NClap.Utilities.Windows
{
#pragma warning disable PC003 // TODO: Native API not available in UWP

    /// <summary>
    /// Wrapper for native methods only available on traditional Windows platforms.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// Extracts a Unicode character from a key press.
        /// </summary>
        /// <param name="wVirtKey">Virtual key.</param>
        /// <param name="wScanCode">Key scan code.</param>
        /// <param name="lpKeyState">Current modifiers key state.</param>
        /// <param name="pwszBuff">On success receives extracted characters.</param>
        /// <param name="cchBuff">Maximum number of characters to retrieve.</param>
        /// <param name="wFlags">Flags.</param>
        /// <returns>On success, returns number of characters extracted; otherwise zero
        /// or a negative number.</returns>
        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, ThrowOnUnmappableChar = true)]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [MarshalAs(UnmanagedType.LPArray)] [Out] char[] pwszBuff,
            int cchBuff,
            uint wFlags);
    }
}
