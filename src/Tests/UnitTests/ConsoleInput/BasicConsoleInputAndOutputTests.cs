using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.ConsoleInput;
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

        [TestMethod, Ignore] // TODO: Disabled because it relies on having a console handy.
        public void BasicProperties()
        {
            var con = BasicConsole.Default;

            con.CursorSize.Should().BeGreaterOrEqualTo(0).And.BeLessOrEqualTo(100);
            con.CursorLeft.Should().BeGreaterOrEqualTo(0);
            con.CursorTop.Should().BeGreaterOrEqualTo(0);
            con.BufferWidth.Should().BeGreaterOrEqualTo(0);
            con.BufferHeight.Should().BeGreaterOrEqualTo(0);

            con.Invoking(c => { var x = c.CursorVisible; }).Should().NotThrow();
            con.Invoking(c => { var x = c.TreatControlCAsInput; }).Should().NotThrow();
            con.Invoking(c => { var x = c.ForegroundColor; }).Should().NotThrow();
            con.Invoking(c => { var x = c.BackgroundColor; }).Should().NotThrow();
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
