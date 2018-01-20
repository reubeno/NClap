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
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Cannot change class name")]
    public sealed class ConsoleKeyBindingSet : IReadOnlyConsoleKeyBindingSet
    {
        private static readonly IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> DefaultIgnoredModifierKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>
        {
#if NET461
            [ConsoleKey.LeftWindows] = ConsoleInputOperation.NoOp,
            [ConsoleKey.RightWindows] = ConsoleInputOperation.NoOp,
#endif
        };

        private static readonly IReadOnlyDictionary<char, ConsoleInputOperation> DefaultControlCharBindings = new Dictionary<char, ConsoleInputOperation>
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

        private static readonly IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> DefaultControlKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>
        {
            [ConsoleKey.Backspace] = ConsoleInputOperation.BackwardKillWord,
            [ConsoleKey.Delete] = ConsoleInputOperation.KillWord,
            [ConsoleKey.LeftArrow] = ConsoleInputOperation.BackwardWord,
            [ConsoleKey.RightArrow] = ConsoleInputOperation.ForwardWord
        };

        private static readonly IReadOnlyDictionary<char, ConsoleInputOperation> DefaultAltCharBindings = new Dictionary<char, ConsoleInputOperation>
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

        private static readonly IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> DefaultAltKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>
        {
            [ConsoleKey.Delete] = ConsoleInputOperation.BackwardKillWord,
            [ConsoleKey.Tab] = ConsoleInputOperation.TabInsert
        };

        private static readonly IReadOnlyDictionary<char, ConsoleInputOperation> DefaultControlAltCharBindings = new Dictionary<char, ConsoleInputOperation>
        {
            ['y'] = ConsoleInputOperation.YankNthArg,
            [']'] = ConsoleInputOperation.CharacterSearchBackward
        };

        private static readonly IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> DefaultControlAltKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>();

        private static readonly IReadOnlyDictionary<char, ConsoleInputOperation> DefaultPlainCharBindings = new Dictionary<char, ConsoleInputOperation>
        {
            ['\0'] = ConsoleInputOperation.EndOfFile
        };

        private static readonly IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> DefaultPlainKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>
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
            [ConsoleKey.UpArrow] = ConsoleInputOperation.PreviousHistory,
        };

        private static readonly IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> DefaultShiftKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>
        {
            [ConsoleKey.Tab] = ConsoleInputOperation.CompleteTokenPrevious
        };

        private readonly Dictionary<ConsoleKey, ConsoleInputOperation> _ignoredModifierKeyBindings;
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
            _ignoredModifierKeyBindings = new Dictionary<ConsoleKey, ConsoleInputOperation>();
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
            IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> ignoredModifierKeyBindings,
            IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> controlAltKeyBindings,
            IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> altKeyBindings,
            IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> controlKeyBindings,
            IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> shiftKeyBindings,
            IReadOnlyDictionary<ConsoleKey, ConsoleInputOperation> plainKeyBindings,
            IReadOnlyDictionary<char, ConsoleInputOperation> controlAltCharBindings,
            IReadOnlyDictionary<char, ConsoleInputOperation> altCharBindings,
            IReadOnlyDictionary<char, ConsoleInputOperation> controlCharBindings,
            IReadOnlyDictionary<char, ConsoleInputOperation> plainCharBindings)
        {
            _ignoredModifierKeyBindings = ignoredModifierKeyBindings.ToDictionary(pair => pair.Key, pair => pair.Value);
            _controlAltKeyBindings = controlAltKeyBindings.ToDictionary(pair => pair.Key, pair => pair.Value);
            _altKeyBindings = altKeyBindings.ToDictionary(pair => pair.Key, pair => pair.Value);
            _controlKeyBindings = controlKeyBindings.ToDictionary(pair => pair.Key, pair => pair.Value);
            _shiftKeyBindings = shiftKeyBindings.ToDictionary(pair => pair.Key, pair => pair.Value);
            _plainKeyBindings = plainKeyBindings.ToDictionary(pair => pair.Key, pair => pair.Value);
            _controlAltCharBindings = controlAltCharBindings.ToDictionary(pair => pair.Key, pair => pair.Value);
            _altCharBindings = altCharBindings.ToDictionary(pair => pair.Key, pair => pair.Value);
            _controlCharBindings = controlCharBindings.ToDictionary(pair => pair.Key, pair => pair.Value);
            _plainCharBindings = plainCharBindings.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <summary>
        /// Default bindings.
        /// </summary>
        public static IReadOnlyConsoleKeyBindingSet Default { get; } = CreateDefaultSet();

        /// <summary>
        /// Creates a new <see cref="ConsoleKeyBindingSet"/> populated with
        /// defaults.
        /// </summary>
        /// <returns>The new set.</returns>
        public static ConsoleKeyBindingSet CreateDefaultSet() =>
            new ConsoleKeyBindingSet(
                DefaultIgnoredModifierKeyBindings,
                DefaultControlAltKeyBindings,
                DefaultAltKeyBindings,
                DefaultControlKeyBindings,
                DefaultShiftKeyBindings,
                DefaultPlainKeyBindings,
                DefaultControlAltCharBindings,
                DefaultAltCharBindings,
                DefaultControlCharBindings,
                DefaultPlainCharBindings);

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
            KeyTables.Sum(tablePair => tablePair.Item2.Count) +
            CharTables.Sum(tablePair => tablePair.Item2.Count);

        /// <summary>
        /// Retrieves the operation the key is mapped to.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The operation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Justification = "[Legacy]")]
        public ConsoleInputOperation this[ConsoleKeyInfo key] => GetValue(key);

        /// <summary>
        /// Enumerates all keys bound within this binding set.
        /// </summary>
        public IEnumerable<ConsoleKeyInfo> Keys => GetPairs().Select(pair => pair.Key);

        /// <summary>
        /// Enumerates all operations bound within this binding set.
        /// </summary>
        public IEnumerable<ConsoleInputOperation> Values =>
            KeyTables.SelectMany(tablePair => tablePair.Item2.Values).Concat(
                CharTables.SelectMany(tablePair => tablePair.Item2.Values));

        /// <summary>
        /// Checks if the specified key is bound.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if the key is bound; false otherwise.</returns>
        public bool ContainsKey(ConsoleKeyInfo key) => TryGetValue(key, out ConsoleInputOperation op);

        /// <summary>
        /// Find the operation mapped to the specified key press.
        /// </summary>
        /// <param name="key">The key info.</param>
        /// <returns>The mapped operation; throws an exception if the key press is
        /// not mapped.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key press
        /// is not mapped.</exception>
        public ConsoleInputOperation GetValue(ConsoleKeyInfo key)
        {
            if (!TryGetValue(key, out ConsoleInputOperation op))
            {
                throw new KeyNotFoundException();
            }

            return op;
        }

        /// <summary>
        /// Try to find the operation mapped to the specified key press.
        /// </summary>
        /// <param name="key">The key info.</param>
        /// <param name="value">On success, receives the mapped operation.</param>
        /// <returns>True if the key press is mapped; false otherwise.</returns>
        public bool TryGetValue(ConsoleKeyInfo key, out ConsoleInputOperation value)
        {
            if (_ignoredModifierKeyBindings.TryGetValue(key.Key, out value))
            {
                return true;
            }

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
        /// to; otherwise, unbinds the key.</param>
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

        /// <summary>
        /// Binds the specified key (with *any* combination of modifiers) to
        /// the indicated operation.  Note that this is different from binding
        /// the use of a key with *no* modifiers.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="op">If non-null, the operation to bind the key
        /// to; otherwise, unbinds the key.</param>
        public void BindWithIgnoredModifiers(ConsoleKey key, ConsoleInputOperation? op)
        {
            if (op.HasValue)
            {
                _ignoredModifierKeyBindings[key] = op.Value;
            }
            else if (_ignoredModifierKeyBindings.ContainsKey(key))
            {
                _ignoredModifierKeyBindings.Remove(key);
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

        private IEnumerable<KeyValuePair<ConsoleKeyInfo, ConsoleInputOperation>> GetPairs() =>
            KeyTables.SelectMany(
                table => table.Item2.Select(binding => CreatePair(binding.Key, table.Item1, binding.Value))).Concat(
            CharTables.SelectMany(
                table => table.Item2.Select(binding => CreatePair(binding.Key, table.Item1, binding.Value))));

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

        [SuppressMessage("Microsoft.Performance", "CA1801:ReviewUnusedParameters", MessageId = "modifiers", Justification = "[Legacy]")]
        private static ConsoleKey GetKey(char value, ConsoleModifiers modifiers)
        {
            var c = char.ToUpperInvariant(value);

            if ((c >= 'A') && (c <= 'Z'))
            {
                return ConsoleKey.A + (c - 'A');
            }

            return (ConsoleKey)0;
        }
    }
}
