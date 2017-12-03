using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class ColoredStringTests
    {
        [TestMethod]
        public void BogusString()
        {
            Action factory = () => new ColoredString(null, null, null);
            factory.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void ConstructorWorks()
        {
            var s = new ColoredString("text");
            s.Content.Should().Be("text");
            s.ForegroundColor.Should().BeNull();
            s.BackgroundColor.Should().BeNull();

            s = new ColoredString("text", ConsoleColor.Blue);
            s.Content.Should().Be("text");
            s.ForegroundColor.Should().Be(ConsoleColor.Blue);
            s.BackgroundColor.Should().BeNull();
        }

        [TestMethod]
        public void ConstructorThrowsOnNullString()
        {
            Action a = () => { var x = new ColoredString(null); };
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void ImplicitConstructionWorks()
        {
            ColoredString s = "text";
            s.Content.Should().Be("text");
            s.ForegroundColor.Should().BeNull();
            s.BackgroundColor.Should().BeNull();
        }

        [TestMethod]
        public void ImplicitStringifyingWorks()
        {
            var s = new ColoredString("text", ConsoleColor.Blue);

            string str = s;
            str.Should().Be("text");
        }

        [TestMethod]
        public void StringEqualsItself()
        {
            var s = new ColoredString("text", ConsoleColor.Blue, ConsoleColor.DarkGray);
        #pragma warning disable CS1718 // Comparison made to same variable
            (s == s).Should().BeTrue();
            (s != s).Should().BeFalse();
        #pragma warning restore CS1718 // Comparison made to same variable
            s.Equals(s).Should().BeTrue();
            s.Equals((object)s).Should().BeTrue();
            s.Equals(s, StringComparison.Ordinal).Should().BeTrue();
            s.GetHashCode().Should().Be(s.GetHashCode());
        }

        [TestMethod]
        public void StringsWithDifferentColorsAreNotEqual()
        {
            var s1 = new ColoredString("text", ConsoleColor.Blue, ConsoleColor.DarkGray);
            var s2 = new ColoredString("text", ConsoleColor.Green, ConsoleColor.Red);
        #pragma warning disable CS1718 // Comparison made to same variable
            (s1 == s2).Should().BeFalse();
            (s1 != s2).Should().BeTrue();
        #pragma warning restore CS1718 // Comparison made to same variable
            s1.Equals(s2).Should().BeFalse();
            s1.Equals((object)s2).Should().BeFalse();
            s1.Equals(s2, StringComparison.Ordinal).Should().BeFalse();
            s1.GetHashCode().Should().NotBe(s2.GetHashCode());
        }

        [TestMethod]
        public void StringsWithDifferentCases()
        {
            var s1 = new ColoredString("text", ConsoleColor.Blue, ConsoleColor.DarkGray);
            var s2 = new ColoredString("TEXT", ConsoleColor.Blue, ConsoleColor.DarkGray);
            (s1 == s2).Should().BeFalse();
            (s1 != s2).Should().BeTrue();
            s1.Equals(s2).Should().BeFalse();
            s1.Equals((object)s2).Should().BeFalse();
            s1.Equals(s2, StringComparison.Ordinal).Should().BeFalse();
            s1.Equals(s2, StringComparison.OrdinalIgnoreCase).Should().BeTrue();
            s1.GetHashCode().Should().NotBe(s2.GetHashCode());
        }

        [TestMethod]
        public void StringsDoNotEqualObjectsOfDifferentTypes()
        {
            var s = new ColoredString("text");
            s.Equals(3).Should().BeFalse();
        }

        [TestMethod]
        public void EmptyShouldHaveZeroLength()
        {
            ColoredString.Empty.Length.Should().Be(0);
        }

        [TestMethod]
        public void EmptyShouldHaveNoColor()
        {
            ColoredString.Empty.ForegroundColor.Should().BeNull();
            ColoredString.Empty.BackgroundColor.Should().BeNull();
        }
    }
}
