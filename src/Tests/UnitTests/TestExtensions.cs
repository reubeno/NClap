using System;
using System.Collections.Generic;
using System.Linq;
using NClap.Utilities;

namespace NClap.Tests
{
    static class TestExtensions
    {
        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, TSource second) =>
            first.Concat(new[] { second });

        public static IEnumerable<ConsoleKeyInfo> Concat(this IEnumerable<ConsoleKeyInfo> first, params ConsoleKey[] second) =>
            first.Concat(second.Select(key => key.ToKeyInfo()));

        public static IEnumerable<ConsoleKeyInfo> AsKeys(this string value)
        {
            return value.Select(c => c.ToKeyInfo());
        }

        public static IEnumerable<ConsoleKeyInfo> WithShift(this IEnumerable<ConsoleKeyInfo> keyInfo) =>
            keyInfo.Select(ki => new ConsoleKeyInfo(
                char.ToUpperInvariant(ki.KeyChar),
                ki.Key,
                true,
                ki.Modifiers.HasFlag(ConsoleModifiers.Alt),
                ki.Modifiers.HasFlag(ConsoleModifiers.Control)));

        public static IEnumerable<ConsoleKeyInfo> WithAlt(this IEnumerable<ConsoleKeyInfo> keyInfo) =>
            keyInfo.Select(ki => new ConsoleKeyInfo(
                '\0',
                ki.Key,
                ki.Modifiers.HasFlag(ConsoleModifiers.Shift),
                true,
                ki.Modifiers.HasFlag(ConsoleModifiers.Control)));

        public static ConsoleKeyInfo CtrlInfo(this ConsoleKey key) =>
            new ConsoleKeyInfo('\0', key, false, false, true);

        public static IEnumerable<ConsoleKeyInfo> WithCtrl(this ConsoleKey key)
        {
            yield return key.CtrlInfo();
        }

        public static ConsoleKeyInfo AltInfo(this ConsoleKey key) =>
            new ConsoleKeyInfo('\0', key, false, true, false);

        public static IEnumerable<ConsoleKeyInfo> WithAlt(this ConsoleKey key)
        {
            yield return key.AltInfo();
        }

        public static ConsoleKeyInfo ShiftInfo(this ConsoleKey key)
        {
            var c = InputUtilities.TryGetSingleChar(key, ConsoleModifiers.Shift);
            return new ConsoleKeyInfo(c.GetValueOrDefault(), key, true, false, false);
        }

        public static IEnumerable<ConsoleKeyInfo> WithShift(this ConsoleKey key)
        {
            yield return key.ShiftInfo();
        }

        public static ConsoleKeyInfo ToKeyInfo(this ConsoleKey key)
        {
            return new ConsoleKeyInfo('\0', key, false, false, false);
        }

        public static IEnumerable<ConsoleKeyInfo> AsInfo(this ConsoleKey key)
        {
            yield return key.ToKeyInfo();
        }

        public static ConsoleKeyInfo ToKeyInfo(this char value, bool shift = false, bool alt = false, bool control = false)
        {
            ConsoleKey key;
            if (value == ' ')
            {
                key = ConsoleKey.Spacebar;
            }
            else
            {
                var valueStr = char.ToUpperInvariant(value).ToString();
                if (!Enum.TryParse(valueStr, true, out key))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
            }

            return new ConsoleKeyInfo(value, key, shift, alt, control);
        }
    }
}
