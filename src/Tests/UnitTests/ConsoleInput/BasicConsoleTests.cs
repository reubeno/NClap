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
            con.BufferWidth.Should().BeGreaterThan(0);
            con.BufferHeight.Should().BeGreaterThan(0);
            con.WindowWidth.Should().BeGreaterOrEqualTo(con.BufferWidth);
            con.WindowHeight.Should().BeGreaterOrEqualTo(con.BufferHeight);

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
            con.SetCursorPosition(con.CursorLeft, con.CursorTop).Should().BeTrue();
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
        public void TestThatClearDoesNotMoveCursor()
        {
            var con = BasicConsole.Default;
            var cursor = new { Left = con.CursorLeft, Top = con.CursorTop };

            con.Clear();

            con.CursorLeft.Should().Be(cursor.Left);
            con.CursorTop.Should().Be(cursor.Top);
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
