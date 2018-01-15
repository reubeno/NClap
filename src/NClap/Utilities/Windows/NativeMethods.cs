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
