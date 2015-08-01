using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using NClap.Utilities;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Represents a console key binding set.
    /// </summary>
    public class ConsoleKeyBindingSet : IReadOnlyConsoleKeyBindingSet
    {
        private static readonly IDictionary<char, ConsoleInputOperation> s_defaultControlCharBindings = new Dictionary<char, ConsoleInputOperation>
        {
            ['a'] = ConsoleInputOperation.BeginningOfLine,
            ['b'] = ConsoleInputOperation.BackwardChar,
            ['c'] = ConsoleInputOperation.EndOfFile,
            ['d'] = ConsoleInputOperation.EndOfFile,
            ['e'] = ConsoleInputOperation.EndOfLine,
            ['f'] = ConsoleInputOperation.ForwardChar,
            ['g'] = ConsoleInputOperation.Abort,
            ['k'] = ConsoleInputOperation.KillLine,
            ['l'] = ConsoleInputOperation.ClearScreen,
            ['p'] = ConsoleInputOperation.PreviousHistory,
            ['n'] = ConsoleInputOperation.NextHistory,
            ['q'] = ConsoleInputOperation.QuotedInsert,
            ['r'] = ConsoleInputOperation.ReverseSearchHistory,
            ['s'] = ConsoleInputOperation.ForwardSearchHistory,
            ['t'] = ConsoleInputOperation.TransposeChars,
            ['u'] = ConsoleInputOperation.UnixLineDiscard,
            ['v'] = ConsoleInputOperation.QuotedInsert,
            ['w'] = ConsoleInputOperation.UnixWordRubout,
            ['y'] = ConsoleInputOperation.Yank,
            ['_'] = ConsoleInputOperation.Undo,
            ['@'] = ConsoleInputOperation.SetMark,
            [']'] = ConsoleInputOperation.CharacterSearch,
            [' '] = ConsoleInputOperation.PossibleCompletions
        };

        private static readonly IDictionary<ConsoleKey, ConsoleInputOperation> s_defaultControlKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>
        {
            [ConsoleKey.Backspace] = ConsoleInputOperation.BackwardKillWord,
            [ConsoleKey.Delete] = ConsoleInputOperation.KillWord,
            [ConsoleKey.LeftArrow] = ConsoleInputOperation.BackwardWord,
            [ConsoleKey.RightArrow] = ConsoleInputOperation.ForwardWord,
        };

        private static readonly IDictionary<char, ConsoleInputOperation> s_defaultAltCharBindings = new Dictionary<char, ConsoleInputOperation>
        {
            ['b'] = ConsoleInputOperation.BackwardWord,
            ['c'] = ConsoleInputOperation.CapitalizeWord,
            ['d'] = ConsoleInputOperation.KillWord,
            ['f'] = ConsoleInputOperation.ForwardWord,
            ['l'] = ConsoleInputOperation.DowncaseWord,
            ['n'] = ConsoleInputOperation.NonIncrementalForwardSearchHistory,
            ['p'] = ConsoleInputOperation.NonIncrementalReverseSearchHistory,
            ['r'] = ConsoleInputOperation.RevertLine,
            ['u'] = ConsoleInputOperation.UpcaseWord,
            ['y'] = ConsoleInputOperation.YankPop,
            ['<'] = ConsoleInputOperation.BeginningOfHistory,
            ['>'] = ConsoleInputOperation.EndOfHistory,
            ['.'] = ConsoleInputOperation.YankLastArg,
            ['?'] = ConsoleInputOperation.PossibleCompletions,
            ['*'] = ConsoleInputOperation.InsertCompletions,
            ['~'] = ConsoleInputOperation.TildeExpand,
            ['#'] = ConsoleInputOperation.InsertComment
        };

        private static readonly IDictionary<ConsoleKey, ConsoleInputOperation> s_defaultAltKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>
        {
            [ConsoleKey.Delete] = ConsoleInputOperation.BackwardKillWord,
            [ConsoleKey.Tab] = ConsoleInputOperation.TabInsert
        };

        private static readonly IDictionary<char, ConsoleInputOperation> s_defaultControlAltCharBindings = new Dictionary<char, ConsoleInputOperation>
        {
            ['y'] = ConsoleInputOperation.YankNthArg,
            [']'] = ConsoleInputOperation.CharacterSearchBackward
        };

        private static readonly IDictionary<ConsoleKey, ConsoleInputOperation> s_defaultControlAltKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>();

        private static readonly IDictionary<char, ConsoleInputOperation> s_defaultPlainCharBindings = new Dictionary<char, ConsoleInputOperation>
        {
            ['\0'] = ConsoleInputOperation.EndOfFile
        };

        private static readonly IDictionary<ConsoleKey, ConsoleInputOperation> s_defaultPlainKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>
        {
            [ConsoleKey.Backspace] = ConsoleInputOperation.DeletePreviousChar,
            [ConsoleKey.Delete] = ConsoleInputOperation.DeleteChar,
            [ConsoleKey.DownArrow] = ConsoleInputOperation.NextHistory,
            [ConsoleKey.End] = ConsoleInputOperation.EndOfLine,
            [ConsoleKey.Enter] = ConsoleInputOperation.AcceptLine,
            [ConsoleKey.Escape] = ConsoleInputOperation.RevertLine,
            [ConsoleKey.Home] = ConsoleInputOperation.BeginningOfLine,
            [ConsoleKey.Insert] = ConsoleInputOperation.ToggleInsertMode,
            [ConsoleKey.LeftArrow] = ConsoleInputOperation.BackwardChar,
            [ConsoleKey.RightArrow] = ConsoleInputOperation.ForwardChar,
            [ConsoleKey.Tab] = ConsoleInputOperation.CompleteTokenNext,
            [ConsoleKey.UpArrow] = ConsoleInputOperation.PreviousHistory
        };

        private static readonly IDictionary<ConsoleKey, ConsoleInputOperation> s_defaultShiftKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>
        {
            [ConsoleKey.Tab] = ConsoleInputOperation.CompleteTokenPrevious
        };

        private readonly Dictionary<ConsoleKey, ConsoleInputOperation> _controlAltKeyBindings;
        private readonly Dictionary<ConsoleKey, ConsoleInputOperation> _altKeyBindings;
        private readonly Dictionary<ConsoleKey, ConsoleInputOperation> _controlKeyBindings;
        private readonly Dictionary<ConsoleKey, ConsoleInputOperation> _shiftKeyBindings;
        private readonly Dictionary<ConsoleKey, ConsoleInputOperation> _plainKeyBindings;

        private readonly Dictionary<char, ConsoleInputOperation> _controlAltCharBindings;
        private readonly Dictionary<char, ConsoleInputOperation> _altCharBindings;
        private readonly Dictionary<char, ConsoleInputOperation> _controlCharBindings;
        private readonly Dictionary<char, ConsoleInputOperation> _plainCharBindings;

        /// <summary>
        /// Constructs an empty key binding set.
        /// </summary>
        public ConsoleKeyBindingSet()
        {
            _controlAltKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>();
            _altKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>();
            _controlKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>();
            _shiftKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>();
            _plainKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>();
            _controlAltCharBindings = new Dictionary<char, ConsoleInputOperation>();
            _altCharBindings = new Dictionary<char, ConsoleInputOperation>();
            _controlCharBindings = new Dictionary<char, ConsoleInputOperation>();
            _plainCharBindings = new Dictionary<char, ConsoleInputOperation>();
        }

        private ConsoleKeyBindingSet(
            IDictionary<ConsoleKey, ConsoleInputOperation> controlAltKeyBindings,
            IDictionary<ConsoleKey, ConsoleInputOperation> altKeyBindings,
            IDictionary<ConsoleKey, ConsoleInputOperation> controlKeyBindings,
            IDictionary<ConsoleKey, ConsoleInputOperation> shiftKeyBindings,
            IDictionary<ConsoleKey, ConsoleInputOperation> plainKeyBindings,
            IDictionary<char, ConsoleInputOperation> controlAltCharBindings,
            IDictionary<char, ConsoleInputOperation> altCharBindings,
            IDictionary<char, ConsoleInputOperation> controlCharBindings,
            IDictionary<char, ConsoleInputOperation> plainCharBindings
            )
        {
            _controlAltKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>(controlAltKeyBindings);
            _altKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>(altKeyBindings);
            _controlKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>(controlKeyBindings);
            _shiftKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>(shiftKeyBindings);
            _plainKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>(plainKeyBindings);
            _controlAltCharBindings = new Dictionary<char, ConsoleInputOperation>(controlAltCharBindings);
            _altCharBindings = new Dictionary<char, ConsoleInputOperation>(altCharBindings);
            _controlCharBindings = new Dictionary<char, ConsoleInputOperation>(controlCharBindings);
            _plainCharBindings = new Dictionary<char, ConsoleInputOperation>(plainCharBindings);
        }

        /// <summary>
        /// Default bindings.
        /// </summary>
        public static IReadOnlyConsoleKeyBindingSet Default { get; } = new ConsoleKeyBindingSet(
            s_defaultControlAltKeyBindings,
            s_defaultAltKeyBindings,
            s_defaultControlKeyBindings,
            s_defaultShiftKeyBindings,
            s_defaultPlainKeyBindings,
            s_defaultControlAltCharBindings,
            s_defaultAltCharBindings,
            s_defaultControlCharBindings,
            s_defaultPlainCharBindings);

        /// <summary>
        /// Enumerate the contents of the binding set.
        /// </summary>
        /// <returns>The enumeration.</returns>
        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable<KeyValuePair<ConsoleKeyInfo, ConsoleInputOperation>>)this).GetEnumerator();

        /// <summary>
        /// Enumerate the contents of the binding set.
        /// </summary>
        /// <returns>The enumeration.</returns>
        IEnumerator<KeyValuePair<ConsoleKeyInfo, ConsoleInputOperation>> IEnumerable<KeyValuePair<ConsoleKeyInfo, ConsoleInputOperation>>.GetEnumerator() =>
            GetPairs().GetEnumerator();

        /// <summary>
        /// The number of operations bound in it.
        /// </summary>
        public int Count =>
            KeyTables.Sum(tablePair => tablePair.Item2.Count()) +
            CharTables.Sum(tablePair => tablePair.Item2.Count());

        /// <summary>
        /// Retrieves the operation the key is mapped to.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The operation.</returns>
        public ConsoleInputOperation this[ConsoleKeyInfo key]
        {
            get
            {
                ConsoleInputOperation op;
                if (!TryGetValue(key, out op))
                {
                    throw new KeyNotFoundException();
                }

                return op;
            }
        }

        /// <summary>
        /// Enumerates all keys bound within this binding set.
        /// </summary>
        public IEnumerable<ConsoleKeyInfo> Keys => GetPairs().Select(pair => pair.Key);

        /// <summary>
        /// Enumerates all operations bound within this binding set.
        /// </summary>
        public IEnumerable<ConsoleInputOperation> Values
        {
            get
            {
                return KeyTables.SelectMany(tablePair => tablePair.Item2.Values).Concat(
                    CharTables.SelectMany(tablePair => tablePair.Item2.Values));
            }
        }

        /// <summary>
        /// Checks if the specified key is bound.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if the key is bound; false otherwise.</returns>
        public bool ContainsKey(ConsoleKeyInfo key)
        {
            ConsoleInputOperation op;
            return TryGetValue(key, out op);
        }

        /// <summary>
        /// Try to find the operation mapped to the specified key press.
        /// </summary>
        /// <param name="key">The key info.</param>
        /// <param name="value">On success, receives the mapped operation.</param>
        /// <returns>True if the key press is mapped; false otherwise.</returns>
        public bool TryGetValue(ConsoleKeyInfo key, out ConsoleInputOperation value)
        {
            var ctrl = key.Modifiers.HasFlag(ConsoleModifiers.Control);
            var alt = key.Modifiers.HasFlag(ConsoleModifiers.Alt);
            var shift = key.Modifiers.HasFlag(ConsoleModifiers.Shift);

            var modifiers = (ConsoleModifiers)0;
            if (ctrl) modifiers |= ConsoleModifiers.Control;
            if (alt) modifiers |= ConsoleModifiers.Alt;
            if (shift) modifiers |= ConsoleModifiers.Shift;

            IReadOnlyDictionary<char, ConsoleInputOperation> charBindings;
            IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> keyBindings;
            IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> shiftKeyBindings;
            if (ctrl && alt)
            {
                charBindings = _controlAltCharBindings;
                keyBindings = _controlAltKeyBindings;
                shiftKeyBindings = null;
            }
            else if (alt)
            {
                charBindings = _altCharBindings;
                keyBindings = _altKeyBindings;
                shiftKeyBindings = null;
            }
            else if (ctrl)
            {
                charBindings = _controlCharBindings;
                keyBindings = _controlKeyBindings;
                shiftKeyBindings = null;
            }
            else
            {
                charBindings = _plainCharBindings;
                keyBindings = _plainKeyBindings;
                shiftKeyBindings = _shiftKeyBindings;
            }

            return TryGetValue(charBindings, keyBindings, shiftKeyBindings, key.Key, modifiers, out value);
        }

        /// <summary>
        /// Bind the specified character (with the specified modifiers) to the
        /// indicated operation.
        /// </summary>
        /// <param name="value">The character.</param>
        /// <param name="modifiers">The modifiers for the character.</param>
        /// <param name="op">If non-null, the operation to bind the character
        /// to; otherwise, unbinds the character.</param>
        public void Bind(char value, ConsoleModifiers modifiers, ConsoleInputOperation? op)
        {
            Dictionary<char, ConsoleInputOperation> bindings;

            if (modifiers.HasFlag(ConsoleModifiers.Control) && modifiers.HasFlag(ConsoleModifiers.Alt))
            {
                bindings = _controlAltCharBindings;
            }
            else if (modifiers.HasFlag(ConsoleModifiers.Alt))
            {
                bindings = _altCharBindings;
            }
            else if (modifiers.HasFlag(ConsoleModifiers.Control))
            {
                bindings = _controlCharBindings;
            }
            else
            {
                bindings = _plainCharBindings;
            }

            if (op.HasValue)
            {
                bindings[value] = op.Value;
            }
            else if (bindings.ContainsKey(value))
            {
                bindings.Remove(value);
            }
        }

        /// <summary>
        /// Bind the specified key (with the specified modifiers) to the
        /// indicated operation.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="modifiers">The modifiers for the character.</param>
        /// <param name="op">If non-null, the operation to bind the key
        /// to; otherwise, unbinds the character.</param>
        public void Bind(ConsoleKey key, ConsoleModifiers modifiers, ConsoleInputOperation? op)
        {
            Dictionary<ConsoleKey, ConsoleInputOperation> bindings;

            if (modifiers.HasFlag(ConsoleModifiers.Control) && modifiers.HasFlag(ConsoleModifiers.Alt))
            {
                bindings = _controlAltKeyBindings;
            }
            else if (modifiers.HasFlag(ConsoleModifiers.Alt))
            {
                bindings = _altKeyBindings;
            }
            else if (modifiers.HasFlag(ConsoleModifiers.Control))
            {
                bindings = _controlKeyBindings;
            }
            else if (modifiers.HasFlag(ConsoleModifiers.Shift))
            {
                bindings = _shiftKeyBindings;
            }
            else
            {
                bindings = _plainKeyBindings;
            }

            if (op.HasValue)
            {
                bindings[key] = op.Value;
            }
            else if (bindings.ContainsKey(key))
            {
                bindings.Remove(key);
            }
        }

        private static bool TryGetValue(
            IReadOnlyDictionary<char, ConsoleInputOperation> charBindings,
            IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> keyBindings,
            IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> shiftKeyBindings,
            ConsoleKey key,
            ConsoleModifiers modifiers,
            out ConsoleInputOperation op)
        {
            var shiftModifiers = modifiers & ConsoleModifiers.Shift;
            var charWithoutNonShiftModifiers = InputUtilities.TryGetSingleChar(key, shiftModifiers);

            // It's important that we try the character bindings first.
            if (charWithoutNonShiftModifiers.HasValue &&
                charBindings.TryGetValue(char.ToLowerInvariant(charWithoutNonShiftModifiers.Value), out op))
            {
                return true;
            }

            if (modifiers.HasFlag(ConsoleModifiers.Shift) &&
                (shiftKeyBindings != null) &&
                shiftKeyBindings.TryGetValue(key, out op))
            {
                return true;
            }

            return keyBindings.TryGetValue(key, out op);
        }

        private IEnumerable<Tuple<ConsoleModifiers, Dictionary<ConsoleKey, ConsoleInputOperation>>> KeyTables => new[]
        {
            Tuple.Create(ConsoleModifiers.Control | ConsoleModifiers.Alt, _controlAltKeyBindings),
            Tuple.Create(ConsoleModifiers.Alt, _altKeyBindings),
            Tuple.Create(ConsoleModifiers.Control, _controlKeyBindings),
            Tuple.Create(ConsoleModifiers.Shift, _shiftKeyBindings),
            Tuple.Create((ConsoleModifiers)0, _plainKeyBindings)
        };

        private IEnumerable<Tuple<ConsoleModifiers, Dictionary<char, ConsoleInputOperation>>> CharTables => new[]
        {
            Tuple.Create(ConsoleModifiers.Control | ConsoleModifiers.Alt, _controlAltCharBindings),
            Tuple.Create(ConsoleModifiers.Alt, _altCharBindings),
            Tuple.Create(ConsoleModifiers.Control, _controlCharBindings),
            Tuple.Create((ConsoleModifiers)0, _plainCharBindings)
        };

        private IEnumerable<KeyValuePair<ConsoleKeyInfo, ConsoleInputOperation>> GetPairs()
        {
            return
                KeyTables.SelectMany(
                    table => table.Item2.Select(binding => CreatePair(binding.Key, table.Item1, binding.Value))).Concat(
                CharTables.SelectMany(
                    table => table.Item2.Select(binding => CreatePair(binding.Key, table.Item1, binding.Value))));
        }

        private static KeyValuePair<ConsoleKeyInfo, ConsoleInputOperation> CreatePair(
            ConsoleKey key,
            ConsoleModifiers modifiers,
            ConsoleInputOperation op)
        {
            var c = GetChar(key, modifiers);

            var keyInfo = new ConsoleKeyInfo(
                c,
                key,
                modifiers.HasFlag(ConsoleModifiers.Shift),
                modifiers.HasFlag(ConsoleModifiers.Alt),
                modifiers.HasFlag(ConsoleModifiers.Control));

            return new KeyValuePair<ConsoleKeyInfo, ConsoleInputOperation>(keyInfo, op);
        }

        private static KeyValuePair<ConsoleKeyInfo, ConsoleInputOperation> CreatePair(
            char value,
            ConsoleModifiers modifiers,
            ConsoleInputOperation op)
        {
            var key = GetKey(value, modifiers);

            var keyInfo = new ConsoleKeyInfo(
                value,
                key,
                modifiers.HasFlag(ConsoleModifiers.Shift),
                modifiers.HasFlag(ConsoleModifiers.Alt),
                modifiers.HasFlag(ConsoleModifiers.Control));

            return new KeyValuePair<ConsoleKeyInfo, ConsoleInputOperation>(keyInfo, op);
        }

        private static char GetChar(ConsoleKey key, ConsoleModifiers modifiers)
        {
            return InputUtilities.TryGetSingleChar(key, modifiers).GetValueOrDefault('\0');
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static ConsoleKey GetKey(char value, ConsoleModifiers modifiers)
        {
            throw new NotImplementedException();
        }
    }
}
