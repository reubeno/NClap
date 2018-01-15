using System;

namespace NClap.Utilities.Windows
{
    /// <summary>
    /// Windows-specific input utilities.
    /// </summary>
    internal static class InputUtilities
    {
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

            var result = NativeMethods.ToUnicode(virtKey, 0, GetKeyState(modifiers), output, output.Length, 0 /* flags */);
            if (result < 0) result = 0;

            var relevantOutput = new char[result];
            Array.Copy(output, relevantOutput, result);

            return relevantOutput;
        }

        private static byte[] GetKeyState(ConsoleModifiers modifiers)
        {
            const byte keyDownFlag = 0x80;

            var keyState = new byte[256];

            if (modifiers.HasFlag(ConsoleModifiers.Alt)) keyState[(int)ConsoleModifierKeys.Alt] |= keyDownFlag;
            if (modifiers.HasFlag(ConsoleModifiers.Control)) keyState[(int)ConsoleModifierKeys.Control] |= keyDownFlag;
            if (modifiers.HasFlag(ConsoleModifiers.Shift)) keyState[(int)ConsoleModifierKeys.Shift] |= keyDownFlag;

            return keyState;
        }
    }
}
