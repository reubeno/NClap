using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.ConsoleInput;
using NSubstitute;

namespace NClap.Tests.ConsoleInput
{
    [TestClass]
    public class ConsoleReaderUnitTests
    {
        [TestMethod]
        public void SimpleKeyPress()
        {
            Check(
                'h',
                input =>
                {
                    input.Insert('h');
                    input.MoveCursorForward(1);
                },
                input => { input.AtEnd.Returns(true); });
        }

        [TestMethod]
        public void Movement()
        {
            Check(ConsoleKey.LeftArrow, input => { input.MoveCursorBackward(1); });
            Check(ConsoleKey.RightArrow, input => { input.MoveCursorForward(1); });
            Check(ConsoleKey.UpArrow, input => { input.ReplaceWithLastLineInHistory(); });
            Check(ConsoleKey.DownArrow, input => { input.ReplaceWithNextLineInHistory(); });
            Check(ConsoleKey.Escape, input => { input.ClearLine(false); });
            Check(ConsoleKey.Backspace, input => { input.DeletePrecedingChar(); });
            Check(ConsoleKey.Delete, input => { input.Delete(); });
            Check(ConsoleKey.Home, input => { input.MoveCursorToStart(); });
            Check(ConsoleKey.End, input => { input.MoveCursorToEnd(); });
            Check(ConsoleKey.Enter, input => { }, expectedResult: ConsoleInputOperationResult.EndOfInputLine);

            var lessThan = new ConsoleKeyInfo('<', ConsoleKey.OemComma, true, true, false);
            Check(lessThan, input => { input.ReplaceWithOldestLineInHistory(); });

            var greaterThan = new ConsoleKeyInfo('>', ConsoleKey.OemPeriod, true, true, false);
            Check(greaterThan, input => { input.ReplaceWithYoungestLineInHistory(); });
        }

        [TestMethod]
        public void Capitalize()
        {
            ConsoleReader.Capitalize(string.Empty).Should().BeEmpty();
            ConsoleReader.Capitalize("a").Should().Be("A");
            ConsoleReader.Capitalize("A").Should().Be("A");
            ConsoleReader.Capitalize(" a").Should().Be(" a");
            ConsoleReader.Capitalize("abcd").Should().Be("Abcd");
            ConsoleReader.Capitalize("ab cd").Should().Be("Ab cd");
            ConsoleReader.Capitalize("AB CD").Should().Be("Ab cd");
        }

        [TestMethod]
        public void BogusOp()
        {
            var reader = CreateReader();

            Action processAction = () => reader.Process((ConsoleInputOperation)0xFFFF, new ConsoleKeyInfo());
            processAction.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void UnimplementedOp()
        {
            var reader = CreateReader(
                ConsoleKey.RightArrow.AsInfo().Concat(ConsoleKey.Enter.AsInfo()));

            reader.LineInput.MoveCursorForward(Arg.Any<int>()).ReturnsForAnyArgs(
                call => { throw new NotImplementedException(); });

            reader.ReadLine().Should().BeEmpty();
            reader.LineInput.Received().MoveCursorForward(1);
        }

        private static void Check(
            char value,
            Action<IConsoleLineInput> expectedCalls,
            Action<IConsoleLineInput> inputSetup = null,
            ConsoleInputOperationResult expectedResult = ConsoleInputOperationResult.Normal)
        {
            Check(value.ToKeyInfo(), expectedCalls, inputSetup, expectedResult);
        }

        private static void Check(
            ConsoleKey key,
            Action<IConsoleLineInput> expectedCalls,
            Action<IConsoleLineInput> inputSetup = null,
            ConsoleInputOperationResult expectedResult = ConsoleInputOperationResult.Normal)
        {
            Check(key.ToKeyInfo(), expectedCalls, inputSetup, expectedResult);
        }

        private static void Check(
            ConsoleKeyInfo keyInfo,
            Action<IConsoleLineInput> expectedCalls,
            Action<IConsoleLineInput> inputSetup = null,
            ConsoleInputOperationResult expectedResult = ConsoleInputOperationResult.Normal)
        {
            var reader = CreateReader();
            inputSetup?.Invoke(reader.LineInput);
            reader.ProcessKey(keyInfo).Should().Be(expectedResult);
            Received.InOrder(() => expectedCalls(reader.LineInput));
        }

        private static ConsoleReader CreateReader(IEnumerable<ConsoleKeyInfo> keyStream = null, ConsoleCompletionHandler completionHandler = null)
        {
            var consoleOutput = new SimulatedConsoleOutput();
            var consoleInput = new SimulatedConsoleInput(keyStream ?? Enumerable.Empty<ConsoleKeyInfo>());
            var input = Substitute.For<IConsoleLineInput>();
            return new ConsoleReader(input, consoleInput, consoleOutput, null);
        }
    }
}
