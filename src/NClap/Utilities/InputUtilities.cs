using System;
using System.Runtime.InteropServices;

namespace NClap.Utilities
{
    /// <summary>
    /// Modifier flags for a console key.
    /// </summary>
    [Flags]
    internal enum ConsoleModifierKeys
    {
        /// <summary>
        /// No modifiers present.
        /// </summary>
        None = 0,

        /// <summary>
        /// The shift key was pressed.
        /// </summary>
        Shift = 0x10,

        /// <summary>
        /// The control key was pressed.
        /// </summary>
        Control = 0x11,

        /// <summary>
        /// The alt key was pressed.
        /// </summary>
        Alt = 0x12
    }

    /// <summary>
    /// Utilities for manipulating input (e.g. key input).
    /// </summary>
    internal static class InputUtilities
    {
        /// <summary>
        /// Tries to convert the indicated key (with modifiers) to the generated
        /// character, in accordance with the currently active keyboard layout.
        /// </summary>
        /// <param name="key">The key to translate.</param>
        /// <param name="modifiers">Key modifiers.</param>
        /// <returns>The character, if one exists; otherwise null.</returns>
        public static char? TryGetSingleChar(ConsoleKey key, ConsoleModifiers modifiers)
        {
            var chars = GetChars(key, modifiers);
            return chars.Length == 1 ? new char?(chars[0]) : null;
        }

        /// <summary>
        /// Converts the indicated key (with modifiers) to the generated
        /// characters, in accordance with the currently active keyboard
        /// layout.
        /// </summary>
        /// <param name="key">The key to translate.</param>
        /// <param name="modifiers">Key modifiers.</param>
        /// <returns>The characters.</returns>
        public static char[] GetChars(ConsoleKey key, ConsoleModifiers modifiers)
        {
            var virtKey = (uint)key;
            var output = new char[32];

            var result = NativeMethods.ToUnicode(virtKey, 0, NativeMethods.GetKeyState(modifiers), output, output.Length, 0 /* flags */);
            if (result < 0) result = 0;

            var relevantOutput = new char[result];
            Array.Copy(output, relevantOutput, result);

            return relevantOutput;
        }

        private static class NativeMethods
        {
            public static byte[] GetKeyState(ConsoleModifiers modifiers)
            {
                const byte keyDownFlag = 0x80;

                var keyState = new byte[256];

                if (modifiers.HasFlag(ConsoleModifiers.Alt)) keyState[(int)ConsoleModifierKeys.Alt] |= keyDownFlag;
                if (modifiers.HasFlag(ConsoleModifiers.Control)) keyState[(int)ConsoleModifierKeys.Control] |= keyDownFlag;
                if (modifiers.HasFlag(ConsoleModifiers.Shift)) keyState[(int)ConsoleModifierKeys.Shift] |= keyDownFlag;

                return keyState;
            }

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
}
