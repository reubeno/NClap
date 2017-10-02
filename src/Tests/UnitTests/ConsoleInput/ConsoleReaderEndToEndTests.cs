using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.ConsoleInput;

namespace NClap.Tests.ConsoleInput
{
    [TestClass]
    public class ConsoleReaderEndToEndTests
    {
        [TestMethod, Ignore] // TODO: Disabled because it relies on having a console handy.
        public void NoArgsGetsDefaultReader()
        {
            var reader = new ConsoleReader();
            reader.Should().NotBeNull();
        }

        [TestMethod]
        public void SimpleTextLine() => 
            Process("hello".AsKeys()).Should().Be("hello");

#if NET461
        [TestMethod]
        public void NoOp()
        {
            Process(
                ConsoleKey.LeftWindows.AsInfo(),
                ConsoleKey.RightWindows.AsInfo())
                .Should().BeEmpty();
        }
#endif

        [TestMethod]
        public void Replace() => Process(
            "hello".AsKeys(),
            ConsoleKey.Home.AsInfo(),
            ConsoleKey.Insert.AsInfo(),
            "nope".AsKeys())
            .Should().Be("nopeo");
        
        [TestMethod]
        public void EndOfInput()
        {
            Process(ConsoleKey.D.WithCtrl()).Should().BeNull();
            Process("h".AsKeys(), ConsoleKey.D.WithCtrl()).Should().Be("h");
        }

        [TestMethod]
        public void Backspace()
        {
            Process("hi".AsKeys(), ConsoleKey.Backspace.AsInfo()).Should().Be("h");

            Process(
                "hello".AsKeys(),
                ConsoleKey.Home.AsInfo(),
                ConsoleKey.Backspace.AsInfo())
                .Should().Be("hello");

            Process(
                "hello".AsKeys(),
                ConsoleKey.LeftArrow.AsInfo(),
                ConsoleKey.Backspace.AsInfo())
                .Should().Be("helo");
        }

        [TestMethod]
        public void Delete()
        {
            Process("hi".AsKeys().Concat(ConsoleKey.Delete))
                .Should().Be("hi");

            Process("hello".AsKeys().Concat(ConsoleKey.Home, ConsoleKey.Delete))
                .Should().Be("ello");

            Process("hello".AsKeys().Concat(ConsoleKey.LeftArrow, ConsoleKey.LeftArrow, ConsoleKey.Delete))
                .Should().Be("helo");
        }

        [TestMethod]
        public void ClearLine()
        {
            Process("hi".AsKeys().Concat(ConsoleKey.Escape))
                .Should().BeEmpty();

            Process("hello".AsKeys().Concat(ConsoleKey.Home, ConsoleKey.Escape))
                .Should().BeEmpty();

            Process("hello".AsKeys().Concat(ConsoleKey.LeftArrow, ConsoleKey.LeftArrow, ConsoleKey.Escape))
                .Should().BeEmpty();
        }

        [TestMethod]
        public void ClearScreen() => Process(
            "hi".AsKeys(),
            ConsoleKey.L.WithCtrl(),
            ConsoleKey.Enter.AsInfo())
            .Should().Be("hi");

        [TestMethod]
        public void DeleteWord() =>
            Process("hello world".AsKeys(),
                ConsoleKey.Home.AsInfo(),
                ConsoleKey.Delete.WithCtrl())
                .Should().Be(" world");

        [TestMethod]
        public void DeleteBackWord() => Process(
            "hello world".AsKeys(),
            ConsoleKey.W.WithCtrl())
            .Should().Be("hello ");

        [TestMethod]
        public void InsertTab() =>
            Process(ConsoleKey.Tab.WithAlt()).Should().Be("\t");

        [TestMethod]
        public void Abort() => Process(
            "hello world".AsKeys(),
            ConsoleKey.G.WithCtrl())
            .Should().BeEmpty();

        [TestMethod]
        public void DeleteLastWord() => Process(
            "hi".AsKeys(),
            ConsoleKey.Backspace.WithCtrl())
            .Should().BeEmpty();

        [TestMethod]
        public void Upcase() => Process(
            "hello".AsKeys(),
            ConsoleKey.Home.AsInfo(),
            ConsoleKey.U.WithAlt())
            .Should().Be("HELLO");

        [TestMethod]
        public void Completion()
        {
            var completions = new[] {  "Hello there", "Hello, world!" };
            ConsoleCompletionHandler completionHandler = (tokens, index) => completions;

            Process(
                completionHandler,
                "hello".AsKeys(),
                ConsoleKey.Home.AsInfo(),
                ConsoleKey.Tab.WithShift())
                .Should().Be("\"Hello, world!\"");

            Process(
                completionHandler,
                "hello".AsKeys(),
                ConsoleKey.Home.AsInfo(),
                ConsoleKey.Tab.AsInfo())
                .Should().Be("\"Hello there\"");

            Process(
                completionHandler,
                "hello".AsKeys(),
                ConsoleKey.Oem2.WithAlt().WithShift())
                .Should().Be("hello");

            Process(
                completionHandler,
                "hello".AsKeys(),
                ConsoleKey.D8.WithAlt().WithShift())
                .Should().Be("\"Hello there\" \"Hello, world!\" ");
        }

        [TestMethod]
        public void AnUnimplementedCtrlAltOp() => Process(
            "hello".AsKeys(),
            ConsoleKey.Home.AsInfo(),
            ConsoleKey.Y.WithCtrl().WithAlt())
            .Should().Be("hello");

        [TestMethod]
        public void LotsOfMovement() => Process(
            "hello".AsKeys(),
            ConsoleKey.A.WithCtrl(),
            ConsoleKey.Delete.AsInfo(),
            "abc ".AsKeys(),
            ConsoleKey.E.WithCtrl(),
            ConsoleKey.Backspace.AsInfo(),
            ConsoleKey.LeftArrow.WithCtrl(),
            "foo".AsKeys(),
            ConsoleKey.C.WithAlt(),
            ConsoleKey.RightArrow.WithCtrl(),
            "ZZZ".AsKeys(),
            ConsoleKey.LeftArrow.AsInfo(),
            ConsoleKey.LeftArrow.AsInfo(),
            ConsoleKey.L.WithAlt())
            .Should().Be("abc fooEllZzz");

        [TestMethod]
        public void InsertComment() => Process(
            " hello world".AsKeys(),
            ConsoleKey.D3.WithAlt().WithShift(),
            " there".AsKeys())
            .Should().Be(" hello world there");

        [TestMethod]
        public void CutAndPaste() => Process(
            "hello world".AsKeys(),
            ConsoleKey.LeftArrow.WithCtrl(),
            ConsoleKey.K.WithCtrl(),
            ConsoleKey.Home.AsInfo(),
            ConsoleKey.Y.WithCtrl())
            .Should().Be("worldhello ");

        private static string Process(params ConsoleKeyInfo[] keyInfo) =>
            Process(keyInfo, completionHandler: null);

        private static string Process(params IEnumerable<ConsoleKeyInfo>[] keyInfo) =>
            Process(keyInfo.SelectMany(x => x), completionHandler: null);

        private static string Process(ConsoleCompletionHandler completionHandler, params IEnumerable<ConsoleKeyInfo>[] keyInfo) =>
            Process(keyInfo.SelectMany(x => x), completionHandler: completionHandler);

        private static string Process(IEnumerable<ConsoleKeyInfo> keyInfo, ConsoleCompletionHandler completionHandler = null) =>
            CreateReader(keyInfo.Concat(ConsoleKey.Enter), completionHandler).ReadLine();

        private static ConsoleReader CreateReader(IEnumerable<ConsoleKeyInfo> keyStream, ConsoleCompletionHandler completionHandler = null)
        {
            var consoleOutput = new SimulatedConsoleOutput();
            var consoleInput = new SimulatedConsoleInput(keyStream);
            var input = new ConsoleLineInput(consoleOutput, new ConsoleInputBuffer(), new ConsoleHistory(), completionHandler);
            return new ConsoleReader(input, consoleInput, consoleOutput, null);
        }
    }
}
