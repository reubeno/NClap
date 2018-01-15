using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class InputUtilitiesTests
    {
        [TestMethod]
        public void TestThatAlphaKeysTranslateCorrectly()
        {
            InputUtilities.TryGetSingleChar(ConsoleKey.E, (ConsoleModifiers)0).Should().Be('e');
            InputUtilities.TryGetSingleChar(ConsoleKey.E, ConsoleModifiers.Shift).Should().Be('E');
            InputUtilities.TryGetSingleChar(ConsoleKey.E, ConsoleModifiers.Control).Should().Be('\x05');
        }

        [TestMethod]
        public void TestThatDigitKeysTranslateCorrectly()
        {
            InputUtilities.TryGetSingleChar(ConsoleKey.D3, (ConsoleModifiers)0).Should().Be('3');

            InputUtilities.TryGetSingleChar(ConsoleKey.D0, ConsoleModifiers.Shift).Should().Be(')');
            InputUtilities.TryGetSingleChar(ConsoleKey.D1, ConsoleModifiers.Shift).Should().Be('!');
            InputUtilities.TryGetSingleChar(ConsoleKey.D2, ConsoleModifiers.Shift).Should().Be('@');
            InputUtilities.TryGetSingleChar(ConsoleKey.D3, ConsoleModifiers.Shift).Should().Be('#');
            InputUtilities.TryGetSingleChar(ConsoleKey.D4, ConsoleModifiers.Shift).Should().Be('$');
            InputUtilities.TryGetSingleChar(ConsoleKey.D5, ConsoleModifiers.Shift).Should().Be('%');
            InputUtilities.TryGetSingleChar(ConsoleKey.D6, ConsoleModifiers.Shift).Should().Be('^');
            InputUtilities.TryGetSingleChar(ConsoleKey.D7, ConsoleModifiers.Shift).Should().Be('&');
            InputUtilities.TryGetSingleChar(ConsoleKey.D8, ConsoleModifiers.Shift).Should().Be('*');
            InputUtilities.TryGetSingleChar(ConsoleKey.D9, ConsoleModifiers.Shift).Should().Be('(');
        }

        [TestMethod]
        public void TestThatWhitespaceKeysTranslateCorrectly()
        {
            InputUtilities.TryGetSingleChar(ConsoleKey.Spacebar, (ConsoleModifiers)0).Should().Be(' ');
            InputUtilities.TryGetSingleChar(ConsoleKey.Tab, (ConsoleModifiers)0).Should().Be('\t');
        }

        [TestMethod]
        public void TestThatNamedOemKeysTranslateCorrectly()
        {
            InputUtilities.TryGetSingleChar(ConsoleKey.OemComma, (ConsoleModifiers)0).Should().Be(',');
            InputUtilities.TryGetSingleChar(ConsoleKey.OemComma, ConsoleModifiers.Shift).Should().Be('<');

            InputUtilities.TryGetSingleChar(ConsoleKey.OemMinus, (ConsoleModifiers)0).Should().Be('-');
            InputUtilities.TryGetSingleChar(ConsoleKey.OemMinus, ConsoleModifiers.Shift).Should().Be('_');

            InputUtilities.TryGetSingleChar(ConsoleKey.OemPeriod, (ConsoleModifiers)0).Should().Be('.');
            InputUtilities.TryGetSingleChar(ConsoleKey.OemPeriod, ConsoleModifiers.Shift).Should().Be('>');

            InputUtilities.TryGetSingleChar(ConsoleKey.OemPlus, (ConsoleModifiers)0).Should().Be('=');
            InputUtilities.TryGetSingleChar(ConsoleKey.OemPlus, ConsoleModifiers.Shift).Should().Be('+');
        }

        [TestMethod]
        public void TestThatNumberedOemKeysTranslateCorrectly()
        {
            InputUtilities.TryGetSingleChar(ConsoleKey.Oem1, (ConsoleModifiers)0).Should().Be(';');
            InputUtilities.TryGetSingleChar(ConsoleKey.Oem1, ConsoleModifiers.Shift).Should().Be(':');

            InputUtilities.TryGetSingleChar(ConsoleKey.Oem2, (ConsoleModifiers)0).Should().Be('/');
            InputUtilities.TryGetSingleChar(ConsoleKey.Oem2, ConsoleModifiers.Shift).Should().Be('?');

            InputUtilities.TryGetSingleChar(ConsoleKey.Oem3, (ConsoleModifiers)0).Should().Be('`');
            InputUtilities.TryGetSingleChar(ConsoleKey.Oem3, ConsoleModifiers.Shift).Should().Be('~');

            InputUtilities.TryGetSingleChar(ConsoleKey.Oem4, (ConsoleModifiers)0).Should().Be('[');
            InputUtilities.TryGetSingleChar(ConsoleKey.Oem4, ConsoleModifiers.Shift).Should().Be('{');

            InputUtilities.TryGetSingleChar(ConsoleKey.Oem5, (ConsoleModifiers)0).Should().Be('\\');
            InputUtilities.TryGetSingleChar(ConsoleKey.Oem5, ConsoleModifiers.Shift).Should().Be('|');

            InputUtilities.TryGetSingleChar(ConsoleKey.Oem6, (ConsoleModifiers)0).Should().Be(']');
            InputUtilities.TryGetSingleChar(ConsoleKey.Oem6, ConsoleModifiers.Shift).Should().Be('}');

            InputUtilities.TryGetSingleChar(ConsoleKey.Oem7, (ConsoleModifiers)0).Should().Be('\'');
            InputUtilities.TryGetSingleChar(ConsoleKey.Oem7, ConsoleModifiers.Shift).Should().Be('\"');
        }

        [TestMethod]
        public void TestThatTwoCodePathsAgree()
        {
            var allConsoleKeys = typeof(ConsoleKey).GetTypeInfo().GetEnumValues().Cast<ConsoleKey>();

            bool result = true;
            foreach (var key in allConsoleKeys)
            {
                if (key == ConsoleKey.Packet || key == ConsoleKey.Oem102) continue;

                result = DoTwoCodePathsAgree(key, (ConsoleModifiers)0) && result;
                result = DoTwoCodePathsAgree(key, ConsoleModifiers.Control) && result;
                result = DoTwoCodePathsAgree(key, ConsoleModifiers.Shift) && result;
                result = DoTwoCodePathsAgree(key, ConsoleModifiers.Alt) && result;
                result = DoTwoCodePathsAgree(key, ConsoleModifiers.Control | ConsoleModifiers.Alt) && result;
                result = DoTwoCodePathsAgree(key, ConsoleModifiers.Control | ConsoleModifiers.Shift) && result;
                result = DoTwoCodePathsAgree(key, ConsoleModifiers.Shift | ConsoleModifiers.Alt) && result;
                result = DoTwoCodePathsAgree(key, ConsoleModifiers.Control | ConsoleModifiers.Alt | ConsoleModifiers.Shift) && result;
            }

            result.Should().BeTrue();
        }

        private bool DoTwoCodePathsAgree(ConsoleKey key, ConsoleModifiers modifiers)
        {
            var portableChars = InputUtilities.GetCharsPortable(key, modifiers);
            var winChars = NClap.Utilities.Windows.InputUtilities.GetChars(key, modifiers);

            if (portableChars.Length != winChars.Length)
            {
                Console.Error.WriteLine(
                    $"Different char counts for '{key}' (mods={modifiers}): " +
                    $"portable=[{string.Join(", ", portableChars.Select(c => ((byte)c).ToString()))}] " +
                    $"win=[{string.Join(", ", winChars.Select(c => ((byte)c).ToString()))}]");
                return false;
            }

            for (var i = 0; i < portableChars.Length; ++i)
            {
                if (portableChars[i] != winChars[i])
                {
                    Console.Error.WriteLine(
                        $"Different chars for '{key}' (mods={modifiers}): " +
                        $"portable=[{string.Join(", ", portableChars.Select(c => ((byte)c).ToString()))}] " +
                        $"win=[{string.Join(", ", winChars.Select(c => ((byte)c).ToString()))}]");
                    return false;
                }
            }

            return true;
        }
    }
}
