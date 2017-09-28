using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.ConsoleInput;
using NClap.Utilities;

using NSubstitute;

namespace NClap.Tests.ConsoleInput
{
    [TestClass]
    public class ConsoleLineInputTests
    {
        [TestMethod]
        public void MoveToStart()
        {
            const string s = "something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);

            input.MoveCursorToStart();
            input.AtEnd.Should().BeFalse();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void MoveToEnd()
        {
            const string s = "something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInput(console);

            input.Insert(s);
            input.Contents.Should().Be(s);
            GetContents(console).Should().Be(s);

            input.MoveCursorToEnd();
            input.AtEnd.Should().BeTrue();
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);
        }

        [TestMethod]
        public void MoveBackOneChar()
        {
            const string s = "something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.MoveCursorBackward(1).Should().BeTrue();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length - 1);

            input.MoveCursorBackward(s.Length - 1).Should().BeTrue();
            input.MoveCursorBackward(1).Should().BeFalse();
        }

        [TestMethod]
        public void MoveForwardOneChar()
        {
            const string s = "something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.MoveCursorBackward(1).Should().BeTrue();
            input.MoveCursorForward(1).Should().BeTrue();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);

            input.MoveCursorForward(1).Should().BeFalse();
        }

        [TestMethod]
        public void MoveBackOneWord()
        {
            const string s = "Hello World";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.MoveCursorBackwardOneWord();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(6);

            input.MoveCursorBackwardOneWord();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);

            input.MoveCursorBackwardOneWord();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void MoveBackOneWordWithTrailingWhitespace()
        {
            const string s = "Hello World   ";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.MoveCursorBackwardOneWord();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(6);
        }

        [TestMethod]
        public void MoveBackOneWordWithOnlyWhitespace()
        {
            const string s = "       ";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.MoveCursorBackwardOneWord();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void MoveForwardOneWord()
        {
            const string s = "Hello World";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();

            input.MoveCursorForwardOneWord();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(5);

            input.MoveCursorForwardOneWord();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);

            input.MoveCursorForwardOneWord();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);
        }

        [TestMethod]
        public void MoveForwardOneWordWithLeadingWhitespace()
        {
            const string s = "   Hello World";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();

            input.MoveCursorForwardOneWord();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(8);

            input.MoveCursorForwardOneWord();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);
        }

        [TestMethod]
        public void MoveForwardOneWordWithOnlyWhitespace()
        {
            const string s = "       ";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();

            input.MoveCursorForwardOneWord();
            input.Contents.Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);
        }

        [TestMethod]
        public void Delete()
        {
            const string s = "something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();

            input.Delete();
            GetContents(console).TrimEnd().Should().Be("omething");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);

            input.Delete();
            GetContents(console).TrimEnd().Should().Be("mething");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void DeleteFromEnd()
        {
            const string s = "something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.Delete();
            GetContents(console).Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);
        }

        [TestMethod]
        public void Backspace()
        {
            const string s = "something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.DeletePrecedingChar();
            input.Contents.Should().Be("somethin");
            GetContents(console).TrimEnd().Should().Be("somethin");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length - 1);
        }

        [TestMethod]
        public void BackspaceFromBeginning()
        {
            const string s = "something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();

            input.DeletePrecedingChar();
            input.Contents.Should().Be(s);
            GetContents(console).Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void DeleteBackOneWord()
        {
            const string s = "Hello World";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.DeleteBackwardThroughLastWord();
            input.Contents.Should().Be("Hello ");
            GetContents(console).TrimEnd().Should().Be("Hello");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be("Hello ".Length);

            input.DeleteBackwardThroughLastWord();
            input.Contents.Should().BeEmpty();
            GetContents(console).TrimEnd().Should().BeEmpty();
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void DeleteBackOneWordThroughWhitespace()
        {
            const string s = "     ";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.DeleteBackwardThroughLastWord();
            input.Contents.Should().BeEmpty();
            GetContents(console).TrimEnd().Should().BeEmpty();
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void DeleteForwardOneWord()
        {
            const string s = "Hello World";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();

            input.DeleteForwardToNextWord();
            input.Contents.Should().Be(" World");
            GetContents(console).TrimEnd().Should().Be(" World");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);

            input.DeleteForwardToNextWord();
            input.Contents.Should().BeEmpty();
            GetContents(console).TrimEnd().Should().BeEmpty();
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void DeleteForwardOneWordThroughWhitespace()
        {
            const string s = "     ";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();

            input.DeleteForwardToNextWord();
            input.Contents.Should().BeEmpty();
            GetContents(console).TrimEnd().Should().BeEmpty();
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void ClearLineAndBufferFromEnd()
        {
            const string s = "Hello world";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.ClearLine(false);
            input.Contents.Should().BeEmpty();
            GetContents(console).TrimEnd().Should().BeEmpty();
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void ClearLineAndBufferFromMiddle()
        {
            const string s = "Hello world";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorBackwardOneWord();

            input.ClearLine(false);
            input.Contents.Should().BeEmpty();
            GetContents(console).TrimEnd().Should().BeEmpty();
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void ClearBufferOnlyFromEnd()
        {
            const string s = "Hello world";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.ClearLine(true);
            input.Contents.Should().BeEmpty();
            GetContents(console).Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);
        }

        [TestMethod]
        public void ClearScreen()
        {
            const string s = "Hello world";
            const string prompt = "Prompt>";

            var console = new SimulatedConsoleOutput { CursorTop = 10 };
            var input = CreateInputWithText(console, s);
            input.Prompt = prompt;
            input.MoveCursorBackward(2).Should().BeTrue();

            input.ClearScreen();
            input.Contents.Should().Be(s);
            GetContents(console).Should().Be(prompt + s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(prompt.Length + s.Length - 2);
        }

        [TestMethod]
        public void InsertNullString()
        {
            var input = CreateInput(new SimulatedConsoleOutput());

            Action insertion = () => input.Insert(null);
            insertion.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void InsertCharIntoEmptyBuffer()
        {
            var console = new SimulatedConsoleOutput();
            var input = CreateInput(console);

            input.Contents.Should().BeEmpty();
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);

            input.Insert('c');
            input.Contents.Should().Be("c");
            GetContents(console).Should().Be("c");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void InsertCharInMiddle()
        {
            const string s = "Something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();
            input.MoveCursorForward(4).Should().BeTrue();

            input.Insert('X');

            input.Contents.Should().Be("SomeXthing");
            GetContents(console).Should().Be("SomeXthing");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(4);
        }

        [TestMethod]
        public void InsertStringIntoEmptyBuffer()
        {
            var console = new SimulatedConsoleOutput();
            var input = CreateInput(console);

            input.Contents.Should().BeEmpty();
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);

            input.Insert("cd");
            input.Contents.Should().Be("cd");
            GetContents(console).Should().Be("cd");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void InsertStringInMiddle()
        {
            const string s = "Something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();
            input.MoveCursorForward(4).Should().BeTrue();

            input.Insert("very");

            input.Contents.Should().Be("Someverything");
            GetContents(console).Should().Be("Someverything");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(4);
        }

        [TestMethod]
        public void InsertStringAcrossLines()
        {
            const string s = "012345";

            var console = new SimulatedConsoleOutput(width: 4);
            var input = CreateInput(console);

            input.Insert(s);
            input.MoveCursorToEnd();

            input.Contents.Should().Be(s);
            GetContents(console).Should().Be(s);
            console.CursorTop.Should().Be(1);
            console.CursorLeft.Should().Be(s.Length - console.BufferWidth);

            input.MoveCursorBackward(s.Length).Should().BeTrue();
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);

            input.MoveCursorForward(s.Length).Should().BeTrue();
            console.CursorTop.Should().Be(1);
            console.CursorLeft.Should().Be(s.Length - console.BufferWidth);

            input.MoveCursorToStart();
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);

            for (var index = 0; index < s.Length; ++index)
            {
                input.MoveCursorForward(1).Should().BeTrue();

                var stringIndex = index + 1;
                console.CursorTop.Should().Be(stringIndex / console.BufferWidth);
                console.CursorLeft.Should().Be(stringIndex % console.BufferWidth);
            }

            for (var index = 0; index < s.Length; ++index)
            {
                input.MoveCursorBackward(1).Should().BeTrue();

                var stringIndex = s.Length - (index + 1);
                console.CursorTop.Should().Be(stringIndex / console.BufferWidth);
                console.CursorLeft.Should().Be(stringIndex % console.BufferWidth);
            }

            input.MoveCursorForward(3).Should().BeTrue();
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(3);

            input.MoveCursorForward(3).Should().BeTrue();
            input.AtEnd.Should().BeTrue();
            console.CursorTop.Should().Be(1);
            console.CursorLeft.Should().Be(2);
        }

        [TestMethod]
        public void InsertCharToScrollConsoleOutputBuffer()
        {
            var console = new SimulatedConsoleOutput(width: 4, height: 2) { CursorTop = 1 };
            var input = CreateInput(console);

            input.Insert("012");
            input.MoveCursorToEnd();
            console.CursorTop.Should().Be(1);
            console.CursorLeft.Should().Be(3);

            input.Insert("3");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(3);

            input.MoveCursorToEnd();
            console.CursorTop.Should().Be(1);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void CannotScrollPastBeginningOfConsoleOutputBuffer()
        {
            const string s = "0123456789";

            var console = new SimulatedConsoleOutput(width: 4, height: 2);
            var input = CreateInput(console);

            foreach (var c in s)
            {
                input.Insert(c);
                input.MoveCursorToEnd();
            }

            console.CursorTop.Should().Be(1);
            console.CursorLeft.Should().Be(2);
            input.Buffer.CursorIndex.Should().Be(s.Length);

            input.MoveCursorToStart();

            console.CursorTop.Should().Be(1);
            console.CursorLeft.Should().Be(2);

            input.Buffer.CursorIndex.Should().Be(s.Length);
        }

        [TestMethod]
        public void InsertStringPastEndOfConsoleOutputBuffer()
        {
            const string s = "012345";

            var console = new SimulatedConsoleOutput(width: 4, height: 2) { CursorTop = 1 };

            var input = CreateInput(console);

            input.Insert(s);
            input.MoveCursorToEnd();

            input.Contents.Should().Be(s);
            GetContents(console).Should().Be("012345");
            console.CursorTop.Should().Be(1);
            console.CursorLeft.Should().Be(s.Length - console.BufferWidth);
        }

        [TestMethod]
        public void InsertStringLongerThanConsoleOutputBuffer()
        {
            const string s = "0123456789";

            var console = new SimulatedConsoleOutput(width: 4, height: 2);

            var input = CreateInput(console);

            Action insertion = () => input.Insert(s);
            insertion.ShouldThrow<NotImplementedException>();
        }

        [TestMethod]
        public void ReplaceCharInMiddle()
        {
            const string s = "Something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();
            input.MoveCursorForward(1).Should().BeTrue();

            input.Replace('X');

            input.Contents.Should().Be("SXmething");
            GetContents(console).Should().Be("SXmething");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(1);
        }

        [TestMethod]
        public void ReplaceCharAtEnd()
        {
            const string s = "Something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            Action replacement = () => input.Replace('X');
            replacement.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void ReplaceWithLastLineButEmptyHistory()
        {
            const string s = "Something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorBackward(1).Should().BeTrue();

            input.ReplaceWithLastLineInHistory();

            input.Contents.Should().Be(s);
            GetContents(console).Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length - 1);
        }

        [TestMethod]
        public void ReplaceWithShorterLastLine()
        {
            const string s = "Something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.History.Add("xyz");
            input.History.Add("Hello");

            input.ReplaceWithLastLineInHistory();

            input.Contents.Should().Be("Hello");
            GetContents(console).TrimEnd().Should().Be("Hello");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be("Hello".Length);
        }

        [TestMethod]
        public void ReplaceWithLongerLastLine()
        {
            const string s = "Something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.History.Add("xyz");
            input.History.Add("Hello world!");

            input.ReplaceWithLastLineInHistory();

            input.Contents.Should().Be("Hello world!");
            GetContents(console).TrimEnd().Should().Be("Hello world!");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be("Hello world!".Length);
        }

        [TestMethod]
        public void ReplaceWithNextLine()
        {
            const string s = "Something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.History.Add("xyz");
            input.History.Add("abcd");
            input.ReplaceWithLastLineInHistory();
            input.ReplaceWithLastLineInHistory();

            input.ReplaceWithNextLineInHistory();
            input.Contents.Should().Be("abcd");
            GetContents(console).TrimEnd().Should().Be("abcd");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be("abcd".Length);

            input.ReplaceWithNextLineInHistory();
            input.Contents.Should().Be("abcd");
            GetContents(console).TrimEnd().Should().Be("abcd");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be("abcd".Length);
        }

        [TestMethod]
        public void ReplaceWithOldestLine()
        {
            const string s = "Something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.History.Add("xyz");
            input.History.Add("abcd");

            input.ReplaceWithOldestLineInHistory();
            input.Contents.Should().Be("xyz");
            GetContents(console).TrimEnd().Should().Be("xyz");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be("xyz".Length);
        }

        [TestMethod]
        public void ReplaceWithYoungestLine()
        {
            const string s = "Something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.History.Add("xyz");
            input.History.Add("abcd");

            input.ReplaceWithYoungestLineInHistory();
            input.Contents.Should().Be("abcd");
            GetContents(console).TrimEnd().Should().Be("abcd");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be("abcd".Length);
        }

        [TestMethod]
        public void SaveToHistory()
        {
            const string s = "Something";

            var input = CreateInputWithText(new SimulatedConsoleOutput(), s);
            input.History.EntryCount.Should().Be(0);
            
            input.SaveToHistory();
            input.History.EntryCount.Should().Be(1);
            input.History.MoveCursor(SeekOrigin.End, -1);
            input.History.CurrentEntry.Should().Be(s);
        }

        [TestMethod]
        public void CutFromEndToEnd()
        {
            const string s = "Something";

            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.CutToEnd();
            input.PasteBuffer.Should().BeEmpty();

            input.Contents.Should().Be(s);
            GetContents(console).Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);
        }

        [TestMethod]
        public void CutFromMiddleToEnd()
        {
            const string s = "Something";
            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorBackward(2).Should().BeTrue();

            input.CutToEnd();
            input.PasteBuffer.Should().Be("ng");

            input.Contents.Should().Be("Somethi");
            GetContents(console).TrimEnd().Should().Be("Somethi");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be("Somethi".Length);
        }

        [TestMethod]
        public void PasteWhenBufferIsNull()
        {
            const string s = "Something";
            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.Paste();

            input.Contents.Should().Be(s);
            GetContents(console).Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);
        }

        [TestMethod]
        public void PasteAtEndOfBuffer()
        {
            const string s = "Something";
            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorBackward(3).Should().BeTrue();

            input.CutToEnd();
            input.Insert('x');
            input.MoveCursorToEnd();

            input.Paste();

            input.Contents.Should().Be("Somethxing");
            GetContents(console).Should().Be("Somethxing");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be("Somethxing".Length);
        }

        [TestMethod]
        public void PasteInMiddleOfBuffer()
        {
            const string s = "Something";
            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorBackward(3).Should().BeTrue();

            input.CutToEnd();
            input.MoveCursorBackward(1).Should().BeTrue();

            input.Paste();

            input.Contents.Should().Be("Sometingh");
            GetContents(console).Should().Be("Sometingh");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be("Someting".Length);
        }

        [TestMethod]
        public void ReplaceWithPreviousCompletionWithNoCompletionHandler()
        {
            const string s = "Something";
            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            
            input.ReplaceCurrentTokenWithPreviousCompletion(false);

            input.Contents.Should().Be(s);
            GetContents(console).Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);
        }

        [TestMethod]
        public void ReplaceWithPreviousCompletionWithNoCompletions()
        {
            const string s = "Something";
            var console = new SimulatedConsoleOutput();
            ConsoleCompletionHandler completionHandler = (tokens, index) => Enumerable.Empty<string>();
            var input = CreateInputWithText(console, s, completionHandler);

            input.ReplaceCurrentTokenWithPreviousCompletion(false);

            input.Contents.Should().Be(s);
            GetContents(console).Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);
        }

        [TestMethod]
        public void ReplaceWithPreviousCompletion()
        {
            const string text = "S";
            string[] completions = { "S", "Sa", "sc", "szy" };

            var console = new SimulatedConsoleOutput();
            ConsoleCompletionHandler completionHandler = (tokens, index) => completions;
            var input = CreateInputWithText(console, text, completionHandler);

            ValidateCompletion(input, 1, true, false, "szy");
            ValidateCompletion(input, null, true, true, "sc");

            ValidateCompletion(input, null, true, false, "szy");
            ValidateCompletion(input, null, true, true, "sc");
            ValidateCompletion(input, null, true, true, "Sa");
            ValidateCompletion(input, null, true, true, "S");
            ValidateCompletion(input, null, true, true, "szy");
        }

        [TestMethod]
        public void ReplaceWithPreviousCompletionEncountersEmptyString()
        {
            const string text = "S";
            string[] completions = { "S", string.Empty, "szy" };

            var console = new SimulatedConsoleOutput();
            ConsoleCompletionHandler completionHandler = (tokens, index) => completions;
            var input = CreateInputWithText(console, text, completionHandler);

            ValidateCompletion(input, 1, true, false, "szy");
            ValidateCompletion(input, null, true, true, StringUtilities.QuoteIfNeeded(string.Empty));
            ValidateCompletion(input, null, true, true, "S");
        }

        [TestMethod]
        public void ReplaceWithNextCompletionWithNoCompletionHandler()
        {
            const string s = "Something";
            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);

            input.ReplaceCurrentTokenWithNextCompletion(false);

            input.Contents.Should().Be(s);
            GetContents(console).Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);
        }

        [TestMethod]
        public void ReplaceWithNextCompletionWithNoCompletions()
        {
            const string s = "Something";
            var console = new SimulatedConsoleOutput();
            ConsoleCompletionHandler completionHandler = (tokens, index) => Enumerable.Empty<string>();
            var input = CreateInputWithText(console, s, completionHandler);

            input.ReplaceCurrentTokenWithNextCompletion(false);

            input.Contents.Should().Be(s);
            GetContents(console).Should().Be(s);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(s.Length);
        }

        [TestMethod]
        public void ReplaceWithNextCompletion()
        {
            const string text = "S";
            string[] completions = { "S", "Sa", "sc", "szy" };

            var console = new SimulatedConsoleOutput();
            ConsoleCompletionHandler completionHandler = (tokens, index) => completions;
            var input = CreateInputWithText(console, text, completionHandler);

            ValidateCompletion(input, 1, false, false, "S");
            ValidateCompletion(input, null, false, true, "Sa");

            ValidateCompletion(input, null, false, false, "S");
            ValidateCompletion(input, null, false, true, "Sa");
            ValidateCompletion(input, null, false, true, "sc");
            ValidateCompletion(input, null, false, true, "szy");
            ValidateCompletion(input, null, false, true, "S");
        }

        [TestMethod]
        public void ReplaceWithNextCompletionEncountersEmptyString()
        {
            const string text = "S";
            string[] completions = { "S", string.Empty, "szy" };

            var console = new SimulatedConsoleOutput();
            ConsoleCompletionHandler completionHandler = (tokens, index) => completions;
            var input = CreateInputWithText(console, text, completionHandler);

            ValidateCompletion(input, 1, false, false, "S");
            ValidateCompletion(input, null, false, true, StringUtilities.QuoteIfNeeded(string.Empty));
            ValidateCompletion(input, null, false, true, "szy");
        }

        [TestMethod]
        public void ReplaceWithAllCompletionsButNullHandler()
        {
            var input = CreateInput(new SimulatedConsoleOutput());

            input.ReplaceCurrentTokenWithAllCompletions();
            input.Contents.Should().BeEmpty();
        }

        [TestMethod]
        public void ReplaceWithAllCompletionsButNoCompletions()
        {
            var console = new SimulatedConsoleOutput();
            ConsoleCompletionHandler completionHandler = (tokens, tokenIndex) => Enumerable.Empty<string>();
            var input = CreateInput(console, completionHandler);

            input.ReplaceCurrentTokenWithAllCompletions();
            input.Contents.Should().BeEmpty();
            GetContents(console).Should().BeEmpty();
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void ReplaceWithAllCompletionsAtEnd()
        {
            var console = new SimulatedConsoleOutput();
            var completions = new[] { "abcd", "aXYZ", "aw | i" };
            ConsoleCompletionHandler completionHandler = (tokens, tokenIndex) => completions;
            var input = CreateInput(console, completionHandler);
            input.Insert("a");
            input.MoveCursorToEnd();

            var completionsAsText = string.Join(" ", completions.Select(StringUtilities.QuoteIfNeeded)) + " ";

            input.ReplaceCurrentTokenWithAllCompletions();

            input.Contents.Should().Be(completionsAsText);
            GetContents(console).Should().Be(completionsAsText);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(completionsAsText.Length);
        }

        [TestMethod]
        public void ReplaceWithAllCompletionsInMiddleOfToken()
        {
            ReplaceWithAllCompletions(
                "Hello world",
                2,
                new[] {"Hello", "world"},
                0);
        }

        [TestMethod]
        public void ReplaceWithAllCompletionsAtEndOfToken()
        {
            ReplaceWithAllCompletions(
                "Hello world",
                5,
                new[] { "Hello", "world" },
                0);

            ReplaceWithAllCompletions(
                "Hello world",
                11,
                new[] { "Hello", "world" },
                1);
        }

        [TestMethod]
        public void ReplaceWithAllCompletionsInWhitespace()
        {
            ReplaceWithAllCompletions(
                "  Hello world",
                1,
                new[] { string.Empty, "Hello", "world" },
                0);

            ReplaceWithAllCompletions(
                "Hello   world",
                6,
                new[] { "Hello", string.Empty, "world" },
                1);

            ReplaceWithAllCompletions(
                "Hello world   ",
                12,
                new[] { "Hello", "world" },
                2);
        }

        [TestMethod]
        public void DisplayAllCompletions()
        {
            const string s = "So";
            var console = new SimulatedConsoleOutput(width: 10);
            const string prompt = "Prompt>";

            var completions = new[] { "Some", "somebody", "Something", "soy" };
            ConsoleCompletionHandler completionHandler = (tokens, tokenIndex) => completions;

            var input = CreateInputWithText(console, s, completionHandler);
            input.Prompt = prompt;

            input.MoveCursorBackward(1).Should().BeTrue();
            input.DisplayAllCompletions();

            input.Contents.Should().Be(s);
            GetContents(console).Replace('\0', ' ').TrimEnd().Should().Be(
                "So        " +
                "Some      " +
                "somebody  " +
                "Something " +
                "soy       " +
                prompt + s);
            console.CursorTop.Should().Be(5);
            console.CursorLeft.Should().Be(prompt.Length + s.Length - 1);
        }

        [TestMethod]
        public void DisplayAllCompletionsWithNoCompletions()
        {
            const string s = "Hello world something";
            var console = new SimulatedConsoleOutput(width: 10);

            ConsoleCompletionHandler completionHandler = (tokens, tokenIndex) => Enumerable.Empty<string>();

            var input = CreateInputWithText(console, s, completionHandler);
            var previousConsoleContents = GetContents(console);

            input.DisplayAllCompletions();

            input.Contents.Should().Be(s);
            GetContents(console).Should().Be(previousConsoleContents);
        }

        [TestMethod]
        public void TransformWithBogusFunction()
        {
            const string s = "Something";
            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();

            Action transform = () => input.TransformCurrentWord(null);
            transform.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void TransformCurrentWordToSameLength()
        {
            const string s = "Something here";
            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();

            input.TransformCurrentWord(value => value.ToUpperInvariant());

            input.Contents.Should().Be("SOMETHING here");
            GetContents(console).Should().Be("SOMETHING here");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void TransformCurrentWordToShorterValue()
        {
            const string s = "Something here";
            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();

            input.TransformCurrentWord(value => value.Substring(1));

            input.Contents.Should().Be("omething here");
            GetContents(console).TrimEnd().Should().Be("omething here");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void TransformCurrentWordToLongerValue()
        {
            const string s = "Something here";
            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();

            input.TransformCurrentWord(value => "X" + value + "Z");

            input.Contents.Should().Be("XSomethingZ here");
            GetContents(console).TrimEnd().Should().Be("XSomethingZ here");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void TransformCurrentWordFromMiddle()
        {
            const string s = "Something here";
            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();
            input.MoveCursorForward(2).Should().BeTrue();

            input.TransformCurrentWord(value => value.ToUpperInvariant());

            input.Contents.Should().Be("SoMETHING here");
            GetContents(console).Should().Be("SoMETHING here");
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be("So".Length);
        }

        [TestMethod]
        public void TransformCurrentWordWithLeadingWhitespace()
        {
            const string s = "   Something";
            var console = new SimulatedConsoleOutput();
            var input = CreateInputWithText(console, s);
            input.MoveCursorToStart();

            input.TransformCurrentWord(value => value.ToUpperInvariant());

            input.Contents.Should().Be(s.ToUpperInvariant());
            GetContents(console).Should().Be(s.ToUpperInvariant());
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(0);
        }

        [TestMethod]
        public void DisplayInColumns()
        {
            var console = new SimulatedConsoleOutput(width: 12);
            var input = CreateInput(console);

            input.DisplayInColumns(new[] { "abcd", "ef", "ghi", "j", "klmn" });
            GetContents(console).Replace('\0', ' ').TrimEnd().Should().Be(
                "abcd  j     " +
                "ef    klmn  " +
                "ghi");
        }

        [TestMethod]
        public void DisplayInColumnsWithTextWiderThanConsole()
        {
            var console = new SimulatedConsoleOutput(width: 8);
            var input = CreateInput(console);

            input.DisplayInColumns(new[] {"Shorter", "LongEnough", "x"});
            GetContents(console).Replace('\0', ' ').TrimEnd().Should().Be(
                "Shorter " +
                "LongEnou" +
                "gh      " +
                "x");
        }

        [TestMethod]
        public void DisplayInColumnsWithEmptyList()
        {
            var console = new SimulatedConsoleOutput();
            var input = CreateInput(console);

            input.DisplayInColumns(new string[] { });
            GetContents(console).Should().BeEmpty();
        }

        private static void ValidateCompletion(
            ConsoleLineInput input,
            int? cursorIndex,
            bool reverseOrder,
            bool lastOperationWasCompletion,
            string expectedResult)
        {
            var console = input.ConsoleOutput;

            if (cursorIndex.HasValue)
            {
                input.MoveCursorToStart();
                input.MoveCursorForward(cursorIndex.Value).Should().BeTrue();
            }

            if (reverseOrder)
            {
                input.ReplaceCurrentTokenWithPreviousCompletion(lastOperationWasCompletion);
            }
            else
            {
                input.ReplaceCurrentTokenWithNextCompletion(lastOperationWasCompletion);
            }

            input.Contents.Should().Be(expectedResult);
            GetContents((SimulatedConsoleOutput)console).TrimEnd().Should().Be(expectedResult);
            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(expectedResult.Length);
        }

        private void ReplaceWithAllCompletions(string text, int cursorIndex, IEnumerable<string> textAsTokens, int? expectedCompletionTokenIndex)
        {
            var calls = new List<Tuple<List<string>, int>>();

            var console = new SimulatedConsoleOutput();
            ConsoleCompletionHandler completionHandler = (tokens, tokenIndex) =>
            {
                calls.Add(Tuple.Create(tokens.ToList(), tokenIndex));
                return Enumerable.Empty<string>();
            };

            var input = CreateInputWithText(console, text, completionHandler);
            input.MoveCursorToStart();
            input.MoveCursorForward(cursorIndex).Should().BeTrue();

            input.ReplaceCurrentTokenWithAllCompletions();

            calls.Should().HaveCount(expectedCompletionTokenIndex.HasValue ? 1 : 0);

            if (expectedCompletionTokenIndex.HasValue)
            {
                calls[0].Item1.Should().ContainInOrder(textAsTokens.ToArray());
                calls[0].Item2.Should().Be(expectedCompletionTokenIndex.Value);
            }
        }

        private ConsoleLineInput CreateInputWithText(IConsoleOutput consoleOutput, string text, ConsoleCompletionHandler completionHandler = null)
        {
            var input = CreateInput(consoleOutput, completionHandler);
            input.Insert(text);
            input.MoveCursorToEnd();

            return input;
        }

        private ConsoleLineInput CreateInput(IConsoleOutput consoleOutput = null, ConsoleCompletionHandler completionHandler = null)
        {
            consoleOutput = consoleOutput ?? Substitute.For<IConsoleOutput>();
            var buffer = new ConsoleInputBuffer();
            var history = new ConsoleHistory();
            var input = new ConsoleLineInput(consoleOutput, buffer, history, completionHandler);
            input.ConsoleOutput.Should().BeSameAs(consoleOutput);
            input.Buffer.Should().BeSameAs(buffer);
            input.History.Should().BeSameAs(history);
            input.CompletionHandler.Should().BeSameAs(completionHandler);
            input.InsertMode.Should().BeTrue();

            return input;
        }

        private static string GetContents(SimulatedConsoleOutput consoleOutput)
        {
            var buffer = consoleOutput.OutputBuffer.ToArray();
            return new string(buffer).Trim('\0');
        }
    }
}
