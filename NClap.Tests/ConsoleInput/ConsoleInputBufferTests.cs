using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.ConsoleInput;

namespace NClap.Tests.ConsoleInput
{
    [TestClass]
    public class ConsoleInputBufferTests
    {
        [TestMethod]
        public void EmptyBuffer()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Contents.Should().BeEmpty();
            buffer.CursorIndex.Should().Be(0);
            buffer.Length.Should().Be(0);
            buffer.CursorIsAtEnd.Should().BeTrue();
            buffer.ToString().Should().BeEmpty();
        }

        [TestMethod]
        public void InsertCharAtEnd()
        {
            var buffer = new ConsoleInputBuffer();

            buffer.Insert('x');

            buffer.Contents.Should().Be("x");
            buffer.CursorIndex.Should().Be(0);
            buffer.Length.Should().Be(1);
            buffer.CursorIsAtEnd.Should().BeFalse();
            buffer.ToString().Should().Be("x");
            buffer[0].Should().Be('x');
        }

        [TestMethod]
        public void InsertCharInMiddle()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");
            buffer.MoveCursor(SeekOrigin.Begin, 2);

            buffer.Insert('x');

            buffer.Contents.Should().Be("abxcd");
            buffer.CursorIndex.Should().Be(2);
            buffer.Length.Should().Be(5);
            buffer.CursorIsAtEnd.Should().BeFalse();
            buffer.ToString().Should().Be("abxcd");
        }

        [TestMethod]
        public void InsertStringAtEnd()
        {
            const string s = "xyzzy";
            var buffer = new ConsoleInputBuffer();

            buffer.Insert(s);

            buffer.Contents.Should().Be(s);
            buffer.CursorIndex.Should().Be(0);
            buffer.Length.Should().Be(s.Length);
            buffer.CursorIsAtEnd.Should().BeFalse();
            buffer.ToString().Should().Be(s);
        }

        [TestMethod]
        public void InsertStringInMiddle()
        {
            var buffer = new ConsoleInputBuffer();

            buffer.Insert("abcd");
            buffer.MoveCursor(SeekOrigin.Begin, 2);

            buffer.Insert("xyz");

            buffer.Contents.Should().Be("abxyzcd");
            buffer.CursorIndex.Should().Be(2);
            buffer.Length.Should().Be(7);
            buffer.CursorIsAtEnd.Should().BeFalse();
            buffer.ToString().Should().Be("abxyzcd");
        }

        [TestMethod]
        public void ReplaceOneChar()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");
            buffer.MoveCursor(SeekOrigin.Current, 1).Should().BeTrue();
            buffer.Replace('x');
            buffer.Contents.Should().Be("axcd");
        }

        [TestMethod]
        public void ReplacingAtEnd()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");
            buffer.MoveCursor(SeekOrigin.End, 0);

            Action replacementAction = () => buffer.Replace('x');
            replacementAction.ShouldThrow<Exception>();
        }

        [TestMethod]
        public void ReplaceWithNullString()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            Action replacement = () => buffer.Replace(null);
            replacement.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void ReplaceWithTooLongString()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            Action replacement = () => buffer.Replace("xyzzy");
            replacement.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void ReplaceWithEmptyString()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            buffer.Replace(string.Empty);
            buffer.Contents.Should().Be("abcd");
        }

        [TestMethod]
        public void ReplacePartOfString()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            buffer.MoveCursor(SeekOrigin.Begin, 1);
            buffer.Replace("xy");
            buffer.Contents.Should().Be("axyd");
        }

        [TestMethod]
        public void ReplaceEntireString()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            buffer.Replace("xyzw");
            buffer.Contents.Should().Be("xyzw");
        }

        [TestMethod]
        public void Indexing()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");
            buffer[0].Should().Be('a');
            buffer[3].Should().Be('d');

            Action action1 = () => { var x = buffer[-1]; };
            action1.ShouldThrow<IndexOutOfRangeException>();

            Action action2 = () => { var x = buffer[4]; };
            action2.ShouldThrow<IndexOutOfRangeException>();
        }

        [TestMethod]
        public void MovingCursorBogusly()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            Action movement = () => buffer.MoveCursor((SeekOrigin)0x10, 1);
            movement.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void MovingCursorRelativeToBegin()
        {
            int delta;

            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            buffer.MoveCursor(SeekOrigin.Begin, -1, out delta).Should().BeFalse();
            buffer.CursorIndex.Should().Be(0);
            delta.Should().Be(0);

            buffer.MoveCursor(SeekOrigin.Begin, 0, out delta).Should().BeTrue();
            buffer.CursorIndex.Should().Be(0);
            delta.Should().Be(0);

            buffer.MoveCursor(SeekOrigin.Begin, 2, out delta).Should().BeTrue();
            buffer.CursorIndex.Should().Be(2);
            delta.Should().Be(2);

            buffer.MoveCursor(SeekOrigin.Begin, 4, out delta).Should().BeTrue();
            buffer.CursorIndex.Should().Be(4);
            delta.Should().Be(2);

            buffer.MoveCursor(SeekOrigin.Begin, 5, out delta).Should().BeFalse();
            buffer.CursorIndex.Should().Be(4);
            delta.Should().Be(0);
        }

        [TestMethod]
        public void MovingCursorRelativeToCurrent()
        {
            int delta;

            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            buffer.MoveCursor(SeekOrigin.Current, -1, out delta).Should().BeFalse();
            buffer.CursorIndex.Should().Be(0);
            delta.Should().Be(0);

            buffer.MoveCursor(SeekOrigin.Current, 0, out delta).Should().BeTrue();
            buffer.CursorIndex.Should().Be(0);
            delta.Should().Be(0);

            buffer.MoveCursor(SeekOrigin.Current, 1, out delta).Should().BeTrue();
            buffer.CursorIndex.Should().Be(1);
            delta.Should().Be(1);

            buffer.MoveCursor(SeekOrigin.Current, 1, out delta).Should().BeTrue();
            buffer.CursorIndex.Should().Be(2);
            delta.Should().Be(1);

            buffer.MoveCursor(SeekOrigin.Current, -2, out delta).Should().BeTrue();
            buffer.CursorIndex.Should().Be(0);
            delta.Should().Be(-2);

            buffer.MoveCursor(SeekOrigin.Current, 10, out delta).Should().BeFalse();
            buffer.CursorIndex.Should().Be(0);
            delta.Should().Be(0);
        }

        [TestMethod]
        public void MovingCursorRelativeToEnd()
        {
            int delta;

            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            buffer.MoveCursor(SeekOrigin.End, 1, out delta).Should().BeFalse();
            buffer.CursorIndex.Should().Be(0);
            delta.Should().Be(0);

            buffer.MoveCursor(SeekOrigin.End, 0, out delta).Should().BeTrue();
            buffer.CursorIndex.Should().Be(4);
            delta.Should().Be(4);

            buffer.MoveCursor(SeekOrigin.End, -2, out delta).Should().BeTrue();
            buffer.CursorIndex.Should().Be(2);
            delta.Should().Be(-2);

            buffer.MoveCursor(SeekOrigin.End, 4, out delta).Should().BeFalse();
            buffer.CursorIndex.Should().Be(2);
            delta.Should().Be(0);

            buffer.MoveCursor(SeekOrigin.End, -10, out delta).Should().BeFalse();
            buffer.CursorIndex.Should().Be(2);
            delta.Should().Be(0);
        }

        [TestMethod]
        public void Reading()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            buffer.Read(0).Should().BeEmpty();
            buffer.Read(1).Should().ContainInOrder('a');
            buffer.Read(2).Should().ContainInOrder('a', 'b');
            buffer.Read(3).Should().ContainInOrder('a', 'b', 'c');
            buffer.Read(4).Should().ContainInOrder('a', 'b', 'c', 'd');
            ((Action)(() => buffer.Read(5))).ShouldThrow<ArgumentException>();
        }

        [TestMethod]
        public void ReadingAt()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            buffer.ReadAt(0, 2).Should().ContainInOrder('a', 'b');
            buffer.ReadAt(2, 2).Should().ContainInOrder('c', 'd');

            ((Action)(() => buffer.ReadAt(2, 4))).ShouldThrow<ArgumentException>();
            ((Action)(() => buffer.ReadAt(3, 2))).ShouldThrow<ArgumentException>();
        }

        [TestMethod]
        public void ReadingAtWithExplicitBuffer()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            var outBuffer = new char[4];

            buffer.ReadAt(0, outBuffer, 0, 2);
            outBuffer[0].Should().Be('a');
            outBuffer[1].Should().Be('b');

            buffer.ReadAt(2, outBuffer, 0, 2);
            outBuffer[0].Should().Be('c');
            outBuffer[1].Should().Be('d');

            outBuffer[0] = 'x';
            buffer.ReadAt(0, outBuffer, 1, 1);
            outBuffer[0].Should().Be('x');
            outBuffer[1].Should().Be('a');

            ((Action)(() => buffer.ReadAt(0, null, 0, 2))).ShouldThrow<ArgumentNullException>();
            ((Action)(() => buffer.ReadAt(2, outBuffer, 0, 4))).ShouldThrow<ArgumentException>();
            ((Action)(() => buffer.ReadAt(2, outBuffer, 3, 2))).ShouldThrow<ArgumentException>();
        }

        [TestMethod]
        public void Clear()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");
            buffer.MoveCursor(SeekOrigin.Begin, 2);

            buffer.Clear();

            buffer.Contents.Should().BeEmpty();
            buffer.CursorIndex.Should().Be(0);
            buffer.Length.Should().Be(0);
            buffer.CursorIsAtEnd.Should().BeTrue();
            buffer.ToString().Should().BeEmpty();
        }

        [TestMethod]
        public void Truncate()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");
            buffer.MoveCursor(SeekOrigin.Begin, 2);

            buffer.Truncate();

            buffer.Contents.Should().Be("ab");
            buffer.CursorIndex.Should().Be(2);
            buffer.Length.Should().Be(2);
            buffer.CursorIsAtEnd.Should().BeTrue();
            buffer.ToString().Should().Be("ab");
        }

        [TestMethod]
        public void RemovingAtEnd()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");
            buffer.MoveCursor(SeekOrigin.End, 0);

            buffer.Remove().Should().BeFalse();
            buffer.Contents.Should().Be("abcd");
        }

        [TestMethod]
        public void Remove()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");
            buffer.MoveCursor(SeekOrigin.Begin, 1);

            buffer.Remove().Should().BeTrue();

            buffer.Contents.Should().Be("acd");
            buffer.CursorIndex.Should().Be(1);
            buffer.Length.Should().Be(3);
            buffer.CursorIsAtEnd.Should().BeFalse();
            buffer.ToString().Should().Be("acd");
        }

        [TestMethod]
        public void RemoveLastChar()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");
            buffer.MoveCursor(SeekOrigin.Begin, 3);

            buffer.Remove().Should().BeTrue();

            buffer.Contents.Should().Be("abc");
            buffer.CursorIndex.Should().Be(3);
            buffer.Length.Should().Be(3);
            buffer.CursorIsAtEnd.Should().BeTrue();
            buffer.ToString().Should().Be("abc");
        }

        [TestMethod]
        public void RemoveMultipleChars()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");
            buffer.MoveCursor(SeekOrigin.Begin, 1);

            buffer.Remove(2).Should().BeTrue();

            buffer.Contents.Should().Be("ad");
            buffer.CursorIndex.Should().Be(1);
            buffer.Length.Should().Be(2);
            buffer.CursorIsAtEnd.Should().BeFalse();
            buffer.ToString().Should().Be("ad");
        }

        [TestMethod]
        public void RemovePastEnd()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            buffer.Remove(5).Should().BeFalse();
            buffer.Contents.Should().Be("abcd");
        }

        [TestMethod]
        public void RemoveCharBeforeStart()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            buffer.RemoveCharBeforeCursor().Should().BeFalse();

            buffer.Contents.Should().Be("abcd");
        }

        [TestMethod]
        public void RemoveCharBeforeEnd()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            buffer.MoveCursor(SeekOrigin.End, 0);
            buffer.RemoveCharBeforeCursor().Should().BeTrue();

            buffer.Contents.Should().Be("abc");
            buffer.Length.Should().Be(3);
            buffer.CursorIndex.Should().Be(buffer.Length);
        }

        [TestMethod]
        public void RemoveCharBeforeInMiddle()
        {
            var buffer = new ConsoleInputBuffer();
            buffer.Insert("abcd");

            buffer.MoveCursor(SeekOrigin.End, -1);
            buffer.RemoveCharBeforeCursor().Should().BeTrue();

            buffer.Contents.Should().Be("abd");
            buffer.Length.Should().Be(3);
            buffer.CursorIndex.Should().Be(2);
        }
    }
}
