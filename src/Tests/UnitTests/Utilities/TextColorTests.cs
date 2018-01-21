using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;
using System;
using System.Collections.Generic;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class TextColorTests
    {
        [TestMethod]
        public void TestThatDefaultTextColorsAreEqual()
        {
            var tc1 = new TextColor();
            var tc2 = new TextColor();

            tc1.Equals(tc2).Should().BeTrue();
            tc1.Equals((object)tc2).Should().BeTrue();

            (tc1 == tc2).Should().BeTrue();
            (tc1 != tc2).Should().BeFalse();
        }
        
        [TestMethod]
        public void TestThatTextColorsWithDifferentForegroundColorsAreNotEqual()
        {
            var bgColor = Any.Enum<ConsoleColor>();
            var tc1 = new TextColor { Foreground = ConsoleColor.Green, Background = bgColor };
            var tc2 = new TextColor { Foreground = ConsoleColor.Blue, Background = bgColor };

            tc1.Equals(tc2).Should().BeFalse();
            tc1.Equals((object)tc2).Should().BeFalse();

            (tc1 == tc2).Should().BeFalse();
            (tc1 != tc2).Should().BeTrue();
        }

        [TestMethod]
        public void TestThatTextColorsWithDifferentBackgroundColorsAreNotEqual()
        {
            var fgColor = Any.Enum<ConsoleColor>();
            var tc1 = new TextColor { Foreground = fgColor, Background = ConsoleColor.Green };
            var tc2 = new TextColor { Foreground = fgColor, Background = ConsoleColor.Blue };

            tc1.Equals(tc2).Should().BeFalse();
            tc1.Equals((object)tc2).Should().BeFalse();

            (tc1 == tc2).Should().BeFalse();
            (tc1 != tc2).Should().BeTrue();
        }

        [TestMethod]
        public void TestThatATextColorEqualsItself()
        {
            var tc = new TextColor { Foreground = Any.Enum<ConsoleColor>(), Background = Any.Enum<ConsoleColor>() };

            tc.Equals(tc).Should().BeTrue();
            tc.Equals((object)tc).Should().BeTrue();

        #pragma warning disable CS1718 // Comparison made to same variable
            (tc == tc).Should().BeTrue();
            (tc != tc).Should().BeFalse();
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [TestMethod]
        public void TestThatTextColorsMayBeStoredInDictionary()
        {
            const string testString = "Test string";

            var dict = new Dictionary<TextColor, string>();
            var tc = new TextColor { Foreground = Any.Enum<ConsoleColor>(), Background = Any.Enum<ConsoleColor>() };

            dict[tc] = testString;
            dict[tc].Should().Be(testString);
            dict.Should().NotContainKey(new TextColor());
        }
    }
}
