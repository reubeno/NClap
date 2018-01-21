using NClap.Exceptions;
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
            // Firstly, this definitely doesn't work on non-Windows platforms.
            // Secondly, it's not clear we need such a generic facility here
            // anyhow.  Someone should look back into this to figure out what
            // we *really* need, and find a way to provide that in a
            // platform-agnostic way.
            //

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Windows.InputUtilities.GetChars(key, modifiers);
            }
            else
            {
                return GetCharsPortable(key, modifiers);
            }
        }

        /// <summary>
        /// Converts the indicated key (with modifiers) to the generated
        /// characters, in accordance with the currently active keyboard
        /// layout. Implementation is portable and expected to be supported
        /// on all host platforms.
        /// </summary>
        /// <param name="key">The key to translate.</param>
        /// <param name="modifiers">Key modifiers.</param>
        /// <returns>The characters.</returns>
        internal static char[] GetCharsPortable(ConsoleKey key, ConsoleModifiers modifiers)
        {
            if (key >= ConsoleKey.A && key <= ConsoleKey.Z)
            {
                var alphaIndex = (int)key - (int)ConsoleKey.A;

                char c;
                if (modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    if (modifiers.HasFlag(ConsoleModifiers.Alt))
                    {
                        return Array.Empty<char>();
                    }
                    else
                    {
                        c = (char)(alphaIndex + 1);
                    }
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

            if (modifiers == ConsoleModifiers.Control)
            {
                int c = 0;
                switch (key)
                {
                    case ConsoleKey.Backspace: c = 127; break;
                    case ConsoleKey.Enter: c = 10; break;
                    case ConsoleKey.Escape: c = (int)key; break;
                    case ConsoleKey.Spacebar: c = (int)key; break;
                    case ConsoleKey.Oem4: c = 27; break;
                    case ConsoleKey.Oem5: c = 28; break;
                    case ConsoleKey.Oem6: c = 29; break;
                }

                if (c != 0)
                {
                    return new[] { (char)c };
                }
            }

            if (modifiers.HasFlag(ConsoleModifiers.Control))
            {
                if (modifiers == (ConsoleModifiers.Control | ConsoleModifiers.Shift))
                {
                    switch (key)
                    {
                        case ConsoleKey.D2: return new[] { (char)0 };
                        case ConsoleKey.D6: return new[] { (char)30 };
                        case ConsoleKey.OemMinus: return new[] { (char)31 };
                    }
                }

                return Array.Empty<char>();
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
                        default: throw new InternalInvariantBrokenException();
                    }
                }
                else
                {
                    var digitIndex = (int)key - (int)ConsoleKey.D0;
                    c = (char)('0' + digitIndex);
                }

                return new[] { c };
            }

            if (key >= ConsoleKey.NumPad0 && key <= ConsoleKey.NumPad9)
            {
                if (modifiers.HasFlag(ConsoleModifiers.Shift))
                {
                    return Array.Empty<char>();
                }

                var offset = (int)key - (int)ConsoleKey.NumPad0;
                return new[] { (char)('0' + offset) };
            }

            switch (key)
            {
                case ConsoleKey.Backspace:
                case ConsoleKey.Enter:
                case ConsoleKey.Escape:
                case ConsoleKey.Spacebar:
                case ConsoleKey.Tab:
                    return new char[] { (char)key };

                case ConsoleKey.Multiply:
                    return new char[] { '*' };
                case ConsoleKey.Add:
                    return new char[] { '+' };
                case ConsoleKey.Subtract:
                    return new char[] { '-' };
                case ConsoleKey.Decimal:
                    return new char[] { '.' };
                case ConsoleKey.Divide:
                    return new char[] { '/' };

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
    }
}
