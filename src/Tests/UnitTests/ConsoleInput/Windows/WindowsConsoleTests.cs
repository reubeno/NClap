using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.ConsoleInput.Windows;

namespace NClap.Tests.ConsoleInput.Windows
{
    [TestClass]
    public class WindowsConsoleTests
    {
        [TestMethod]
        public void TestThatWindowsConsoleIsScrollable()
        {
            if (!IsTestApplicable) return;

            var console = new WindowsConsole();
            console.IsScrollable.Should().BeTrue();
        }

        [TestMethod]
        public void TestThatScrollingANegativeNumberOfLinesThrows()
        {
            if (!IsTestApplicable) return;

            var console = new WindowsConsole();
            console.Invoking(c => c.ScrollContents(Any.NegativeInt()))
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestThatScrollingZeroLinesDoesNotThrow()
        {
            if (!IsTestApplicable) return;

            var console = new WindowsConsole();
            console.Invoking(c => c.ScrollContents(0)).Should().NotThrow();
        }

        [TestMethod]
        public void TestThatScrollingPositiveLinesEitherSucceedsOrThrowsNotSupportedException()
        {
            if (!IsTestApplicable) return;

            var console = new WindowsConsole();

            try
            {
                console.ScrollContents(1);
            }
            catch (NotSupportedException)
            {
            }
            catch (Exception)
            {
                throw;
            }
        }

        [TestMethod]
        public void TestThatColorTranslationIsCorrectForValidColors()
        {
            if (!IsTestApplicable) return;

            foreach (var colorName in Enum.GetNames(typeof(ConsoleColor)))
            {
                var consoleColor = (ConsoleColor)
                    (typeof(ConsoleColor).GetTypeInfo().GetField(colorName).GetValue(null));

                var bgColorName = "Background" + colorName;

                var bgEnumValue = typeof(NativeMethods.CharAttributes).GetTypeInfo().GetField(bgColorName);
                bgEnumValue.Should().NotBeNull($"{bgColorName} should be a member of {nameof(NativeMethods.CharAttributes)}");

                var bgColor = (NativeMethods.CharAttributes)bgEnumValue.GetValue(null);

                var translated = WindowsConsole.TranslateBackgroundColor(consoleColor);
                translated.Should().Be(bgColor);
            }
        }

        [TestMethod]
        public void TestThatUnknownColorsTranslateToNone()
        {
            if (!IsTestApplicable) return;

            WindowsConsole.TranslateBackgroundColor((ConsoleColor)int.MaxValue)
                .Should().Be(NativeMethods.CharAttributes.None);
        }

        private static bool IsTestApplicable => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}
