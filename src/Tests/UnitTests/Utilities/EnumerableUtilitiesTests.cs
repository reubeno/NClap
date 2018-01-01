using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;
using System.Linq;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class EnumerableUtilitiesTests
    {
        [TestMethod]
        public void TestThatInsertBetweenLeavesEmptyEnumerationUnmodified()
        {
            Enumerable.Empty<string>().InsertBetween("x").Should().BeEmpty();
        }

        [TestMethod]
        public void TestThatInsertBetweenLeavesOneElementEnumerationUnmodified()
        {
            var input = new[] { "elt" };
            input.InsertBetween("x").Should().Equal(input);
        }

        [TestMethod]
        public void TestThatInsertBetweenCanInsertOnce()
        {
            var input = new[] { "first", "last" };
            input.InsertBetween("x").Should().Equal("first", "x", "last");
        }

        [TestMethod]
        public void TestThatInsertBetweenCanInsertMultipleTimes()
        {
            var input = new[] { "first", "second", "third" };
            input.InsertBetween("x").Should().Equal("first", "x", "second", "x", "third");
        }
    }
}
