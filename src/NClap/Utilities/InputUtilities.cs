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
            //
            // TODO: This whole method needs to be cleaned up.  We have
            // existing code that is Windows-specific, which p/invokes
            // into user32.dll to convert a ConsoleKey to an array of chars.
            // Firstly, this definitely doesn't work on non-Windows platforms;
            // secondly, it's not clear we need such a generic facility here
            // anyhow.  Someone should look back into this to figure out what
            // we *really* need, and find a way to provide that in a
            // platform-agnostic way.
            //

#if NET461
            return GetCharsOnWindows(key, modifiers);
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetCharsOnWindows(key, modifiers);
            }
            else
            {
                return GetCharsOnAnyPlatform(key, modifiers);
            }
#endif
        }

        private static char[] GetCharsOnAnyPlatform(ConsoleKey key, ConsoleModifiers modifiers)
        {
            if (key >= ConsoleKey.A && key <= ConsoleKey.Z)
            {
                var alphaIndex = (int)key - (int)ConsoleKey.A;

                char c;
                if (modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    c = (char)(alphaIndex + 1);
                }
                else
                {
                    c = (char)('a' + alphaIndex);
                    if (modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        c = char.ToUpper(c);
                    }
                }

                return new[] { c };
            }

            if (key >= ConsoleKey.D0 && key <= ConsoleKey.D9)
            {
                char c;
                if (modifiers.HasFlag(ConsoleModifiers.Shift))
                {
                    // TODO: This is plain wrong. It is dependent on keyboard layout.
                    switch (key)
                    {
                        case ConsoleKey.D1: c = '!'; break;
                        case ConsoleKey.D2: c = '@'; break;
                        case ConsoleKey.D3: c = '#'; break;
                        case ConsoleKey.D4: c = '$'; break;
                        case ConsoleKey.D5: c = '%'; break;
                        case ConsoleKey.D6: c = '^'; break;
                        case ConsoleKey.D7: c = '&'; break;
                        case ConsoleKey.D8: c = '*'; break;
                        case ConsoleKey.D9: c = '('; break;
                        case ConsoleKey.D0: c = ')'; break;
                        default:
                            return Array.Empty<char>();
                    }
                }
                else
                {
                    var digitIndex = (int)key - (int)ConsoleKey.D0;
                    c = (char)('0' + digitIndex);
                }

                return new[] { c };
            }

            switch (key)
            {
                case ConsoleKey.Spacebar:
                    return new char[] { ' ' };

                case ConsoleKey.Tab:
                    return new char[] { '\t' };

                case ConsoleKey.OemComma:
                    return modifiers.HasFlag(ConsoleModifiers.Shift) ? new[] { '<' } : new[] { ',' };
                case ConsoleKey.OemPeriod:
                    return modifiers.HasFlag(ConsoleModifiers.Shift) ? new[] { '>' } : new[] { '.' };
                case ConsoleKey.OemMinus:
                    return modifiers.HasFlag(ConsoleModifiers.Shift) ? new[] { '_' } : new[] { '-' };
                case ConsoleKey.OemPlus:
                    return modifiers.HasFlag(ConsoleModifiers.Shift) ? new[] { '+' } : new[] { '=' };
                case ConsoleKey.Oem1:
                    return modifiers.HasFlag(ConsoleModifiers.Shift) ? new[] { ':' } : new[] { ';' };
                case ConsoleKey.Oem2:
                    return modifiers.HasFlag(ConsoleModifiers.Shift) ? new[] { '?' } : new[] { '/' };
                case ConsoleKey.Oem3:
                    return modifiers.HasFlag(ConsoleModifiers.Shift) ? new[] { '~' } : new[] { '`' };
                case ConsoleKey.Oem4:
                    return modifiers.HasFlag(ConsoleModifiers.Shift) ? new[] { '{' } : new[] { '[' };
                case ConsoleKey.Oem5:
                    return modifiers.HasFlag(ConsoleModifiers.Shift) ? new[] { '|' } : new[] { '\\' };
                case ConsoleKey.Oem6:
                    return modifiers.HasFlag(ConsoleModifiers.Shift) ? new[] { '}' } : new[] { ']' };
                case ConsoleKey.Oem7:
                    return modifiers.HasFlag(ConsoleModifiers.Shift) ? new[] { '"' } : new[] { '\'' };

                default:
                    return Array.Empty<char>();
            }
        }

        private static char[] GetCharsOnWindows(ConsoleKey key, ConsoleModifiers modifiers)
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
