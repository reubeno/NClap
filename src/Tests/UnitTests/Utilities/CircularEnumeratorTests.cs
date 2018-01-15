using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class CircularEnumeratorTests
    {
        [TestMethod]
        public void EnumerateEmptyList()
        {
            var list = Array.Empty<string>();
            var e = CircularEnumerator.Create(list);
            e.Should().NotBeNull();
            e.CursorIndex.Should().NotHaveValue();
            e.Values.Should().BeSameAs(list);
            e.Started.Should().BeFalse();

            Action currentItem = () => { var x = e.GetCurrentItem(); };
            currentItem.Should().Throw<IndexOutOfRangeException>();

            e.MoveNext();
            currentItem.Should().Throw<IndexOutOfRangeException>();

            e.MovePrevious();
            currentItem.Should().Throw<IndexOutOfRangeException>();
        }

        [TestMethod]
        public void EnumerateSingleItemList()
        {
            var list = new[] { 2 };
            var e = CircularEnumerator.Create(list);
            e.Should().NotBeNull();
            e.CursorIndex.Should().NotHaveValue();
            e.Values.Should().BeSameAs(list);
            e.Started.Should().BeFalse();

            Action currentItem = () => { var x = e.GetCurrentItem(); };
            currentItem.Should().Throw<InvalidOperationException>();

            e.MoveNext();
            e.GetCurrentItem().Should().Be(2);

            e.MovePrevious();
            e.GetCurrentItem().Should().Be(2);

            e.MovePrevious();
            e.GetCurrentItem().Should().Be(2);
        }

        [TestMethod]
        public void EnumerateList()
        {
            var list = new[] { 0, 1, 2 };
            var e = CircularEnumerator.Create(list);
            e.Should().NotBeNull();
            e.CursorIndex.Should().NotHaveValue();
            e.Values.Should().BeSameAs(list);
            e.Started.Should().BeFalse();

            Action currentItem = () => { var x = e.GetCurrentItem(); };
            currentItem.Should().Throw<InvalidOperationException>();

            e.MovePrevious();
            e.GetCurrentItem().Should().Be(2);

            e.MoveNext();
            e.GetCurrentItem().Should().Be(0);

            e.MoveNext();
            e.GetCurrentItem().Should().Be(1);

            e.MoveNext();
            e.GetCurrentItem().Should().Be(2);

            e.MoveNext();
            e.GetCurrentItem().Should().Be(0);
        }
    }
}
