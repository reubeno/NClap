using System;
using System.IO;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.ConsoleInput;

namespace NClap.Tests.ConsoleInput
{
    [TestClass]
    public class ConsoleHistoryTests
    {
        [TestMethod]
        public void InvalidHistory()
        {
            Action create = () => { var x = new ConsoleHistory(0); };
            create.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void EmptyHistory()
        {
            var history = new ConsoleHistory();
            history.EntryCount.Should().Be(0);
            history.CurrentEntry.Should().BeNull();
            history.MoveCursor(SeekOrigin.Current, 0).Should().BeTrue();
            history.MoveCursor(SeekOrigin.Current, 1).Should().BeFalse();
            history.MoveCursor(SeekOrigin.Current, -1).Should().BeFalse();
        }

        [TestMethod]
        public void SingleEntry()
        {
            var history = new ConsoleHistory();
            history.Add("Something");
            history.EntryCount.Should().Be(1);
            history.CurrentEntry.Should().BeNull();
            history.MoveCursor(SeekOrigin.Current, 0).Should().BeTrue();
            history.MoveCursor(SeekOrigin.Current, 1).Should().BeFalse();
            history.MoveCursor(SeekOrigin.Current, -2).Should().BeFalse();
            history.MoveCursor(SeekOrigin.Current, -1).Should().BeTrue();
            history.CurrentEntry.Should().Be("Something");
        }

        [TestMethod]
        public void MultipleEntries()
        {
            var history = new ConsoleHistory();
            history.Add("Older");
            history.Add("Newer");
            history.EntryCount.Should().Be(2);

            history.CurrentEntry.Should().BeNull();

            history.MoveCursor(SeekOrigin.Current, -1).Should().BeTrue();
            history.CurrentEntry.Should().Be("Newer");

            history.MoveCursor(SeekOrigin.Current, -1).Should().BeTrue();
            history.CurrentEntry.Should().Be("Older");

            history.MoveCursor(SeekOrigin.Current, 1).Should().BeTrue();
            history.CurrentEntry.Should().Be("Newer");

            history.MoveCursor(SeekOrigin.Current, 1).Should().BeTrue();
            history.MoveCursor(SeekOrigin.Current, -2).Should().BeTrue();
            history.CurrentEntry.Should().Be("Older");
        }

        [TestMethod]
        public void MovingCursor()
        {
            var history = new ConsoleHistory();
            history.Add("Oldest");
            history.Add("Middle");
            history.Add("Youngest");

            history.MoveCursor(SeekOrigin.Begin, 0);
            history.CurrentEntry.Should().Be("Oldest");

            history.MoveCursor(SeekOrigin.Begin, 1);
            history.CurrentEntry.Should().Be("Middle");

            history.MoveCursor(SeekOrigin.End, 0);
            history.CurrentEntry.Should().BeNull();

            history.MoveCursor(SeekOrigin.End, -1);
            history.CurrentEntry.Should().Be("Youngest");

            history.MoveCursor(SeekOrigin.End, -2);
            history.CurrentEntry.Should().Be("Middle");

            history.MoveCursor(SeekOrigin.End, -3);
            history.CurrentEntry.Should().Be("Oldest");

            history.MoveCursor(1);
            history.CurrentEntry.Should().Be("Middle");

            Action bogusMovement = () => history.MoveCursor((SeekOrigin)0x10, 0);
            bogusMovement.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void AddEmptyEntry()
        {
            var history = new ConsoleHistory();
            history.Add("Hello");
            history.EntryCount.Should().Be(1);

            history.Add(null);
            history.EntryCount.Should().Be(1);

            history.Add(string.Empty);
            history.EntryCount.Should().Be(1);
        }

        [TestMethod]
        public void HistoryWithMax()
        {
            var history = new ConsoleHistory(2);
            history.Add("First");
            history.EntryCount.Should().Be(1);
            history.Add("Second");
            history.EntryCount.Should().Be(2);
            history.Add("Third");
            history.EntryCount.Should().Be(2);
        }
    }
}
