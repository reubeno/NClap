using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.ConsoleInput;
using NClap.ConsoleInput.Windows;
using NClap.Utilities;

namespace NClap.Tests.ConsoleInput
{
    [TestClass]
    public class BasicConsoleTests
    {
        [TestMethod]
        public void DefaultObjectIsSingleton()
        {
            BasicConsole.Default.Should().NotBeNull();
            BasicConsole.Default.Should().BeSameAs(BasicConsole.Default);
        }

        [TestMethod]
        public void BasicProperties()
        {
            var con = BasicConsole.Default;

            con.CursorSize.Should().BeGreaterOrEqualTo(0).And.BeLessOrEqualTo(100);
            con.CursorLeft.Should().BeGreaterOrEqualTo(0);
            con.CursorTop.Should().BeGreaterOrEqualTo(0);

            // Acknowledge that the tests may be run headlessly, with no active console.
            con.WindowWidth.Should().BeGreaterOrEqualTo(0);
            con.WindowHeight.Should().BeGreaterOrEqualTo(0);

            con.BufferWidth.Should().BeGreaterOrEqualTo(con.WindowWidth);
            con.BufferHeight.Should().BeGreaterOrEqualTo(con.WindowHeight);

            con.Invoking(c => { var x = c.CursorVisible; }).Should().NotThrow();
            con.Invoking(c => { var x = c.TreatControlCAsInput; }).Should().NotThrow();
            con.Invoking(c => { var x = c.ForegroundColor; }).Should().NotThrow();
            con.Invoking(c => { var x = c.BackgroundColor; }).Should().NotThrow();
        }

        [TestMethod]
        public void TestThatExceptionThrownOnInvalidCursorPositions()
        {
            var con = BasicConsole.Default;

            con.SetCursorPosition(-1, 0).Should().BeFalse();
            con.SetCursorPosition(-1, 0).Should().BeFalse();
            con.SetCursorPosition(-1, -1).Should().BeFalse();
            con.SetCursorPosition(0, int.MaxValue).Should().BeFalse();
            con.SetCursorPosition(int.MaxValue, 0).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatCursorMoveNoOpIsOkay()
        {
            var con = BasicConsole.Default;

            if (con.BufferWidth > 0 && con.BufferHeight > 0)
            {
                con.SetCursorPosition(con.CursorLeft, con.CursorTop).Should().BeTrue();
            }
        }

        [TestMethod]
        public void TestThatPropertyUpdateNoOpIsOkay()
        {
            var con = BasicConsole.Default;

            con.Invoking(c => c.CursorVisible = c.CursorVisible).Should().NotThrow();
            con.Invoking(c => c.TreatControlCAsInput = c.TreatControlCAsInput).Should().NotThrow();
        }

        [TestMethod]
        public void TestThatBaseImplementationIsNotScrollable()
        {
            var con = BasicConsole.BaseInstance;
            con.IsScrollable.Should().BeFalse();
        }

        [TestMethod]
        public void TestThatScrollingThrowsWithBaseImplementation()
        {
            var con = BasicConsole.BaseInstance;
            con.Invoking(c => c.ScrollContents(1)).Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void TestThatDefaultImplementationIsAppropriatelySelectedForHostOs()
        {
            var con = BasicConsole.Default;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                con.Should().BeOfType<WindowsConsole>();
            }
            else
            {
                con.Should().BeOfType<BasicConsole>();
            }
        }

        [TestMethod]
        public void TestThatExceptionThrownWhenSettingInvalidCursorSize()
        {
            var con = BasicConsole.Default;
            con.Invoking(c => c.CursorSize = -1).Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestThatExceptionThrownWhenSettingInvalidCursorPosition()
        {
            var con = BasicConsole.Default;
            con.Invoking(c => c.CursorLeft = -1).Should().Throw<ArgumentOutOfRangeException>();
            con.Invoking(c => c.CursorTop = -1).Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestThatExceptionThrownWhenSettingInvalidWindowDimensions()
        {
            var con = BasicConsole.Default;
            con.Invoking(c => c.WindowWidth = -1).Should().Throw<ArgumentOutOfRangeException>();
            con.Invoking(c => c.WindowHeight = -1).Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestThatExceptionThrownWhenSettingInvalidBufferDimensions()
        {
            var con = BasicConsole.Default;
            con.Invoking(c => c.BufferWidth = -1).Should().Throw<ArgumentOutOfRangeException>();
            con.Invoking(c => c.BufferHeight = -1).Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void Writing()
        {
            var con = BasicConsole.Default;

            con.Write(new ColoredString("Sample string. ", ConsoleColor.White, ConsoleColor.Black));
            con.Write("Sample string. ");
            con.WriteLine("Sample string");
        }
    }
}
