using System;
using NClap.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class SubstringTests
    {
        [TestMethod]
        public void EmptyString()
        {
            var s = new Substring(string.Empty, 0);
            s.Base.Should().Be(string.Empty);
            s.Length.Should().Be(0);
            s.StartingOffset.Should().Be(0);
            s.EndingOffset.Should().Be(0);
            s.ToString().Should().BeEmpty();
        }

        [TestMethod]
        public void FullSubstring()
        {
            var s = new Substring("Hello", 0);
            s.Base.Should().Be("Hello");
            s.Length.Should().Be("Hello".Length);
            s.StartingOffset.Should().Be(0);
            s.EndingOffset.Should().Be(s.Length);
            s.ToString().Should().Be("Hello");
        }

        [TestMethod]
        public void StrictSubstring()
        {
            var s = new Substring("Hello", 2, 2);
            s.Base.Should().Be("Hello");
            s.Length.Should().Be(2);
            s.StartingOffset.Should().Be(2);
            s.EndingOffset.Should().Be(4);
            s.ToString().Should().Be("ll");
        }

        [TestMethod]
        public void EmptySubstrings()
        {
            var s = new Substring("Hello", 2, 0);
            s.Base.Should().Be("Hello");
            s.Length.Should().Be(0);
            s.StartingOffset.Should().Be(2);
            s.EndingOffset.Should().Be(2);
            s.ToString().Should().BeEmpty();

            var s2 = new Substring("Hello", 5, 0);
            s2.Base.Should().Be("Hello");
            s2.Length.Should().Be(0);
            s2.StartingOffset.Should().Be(5);
            s2.EndingOffset.Should().Be(5);
            s2.ToString().Should().BeEmpty();
        }

        [TestMethod]
        public void InvalidString()
        {
            Action a = () => new Substring(null, 0);
            a.Should().Throw<ArgumentNullException>();

            Action a2 = () => new Substring(null, 0, 0);
            a2.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void NegativeStartingOffset()
        {
            Action a = () => new Substring("Hello", -1);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void StartingOffsetAfterEndOfString()
        {
            Action a = () => new Substring("Hello", 10, 0);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void LengthPastEndOfString()
        {
            Action a = () => new Substring("Hello", 0, 6);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void NegativeLength()
        {
            Action a = () => new Substring("Hello", 2, -1);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void SubstringOfSubstring()
        {
            var s = new Substring("Hello World", 3, 6);
            var t = new Substring(s, 1);
            t.Base.Should().Be("Hello World");
            t.Length.Should().Be(5);
            t.StartingOffset.Should().Be(4);
            t.EndingOffset.Should().Be(9);
            t.ToString().Should().Be("o Wor");
        }

        [TestMethod]
        public void SubstringOfSubstringWithInvalidLength()
        {
            var s = new Substring("Hello", 1);
            Action a = () => new Substring(s, 1, -1);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void SubstringOfSubstringWithInvalidStartOffset()
        {
            var s = new Substring("Hello", 1);
            Action a = () => new Substring(s, -1, 1);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void SubstringContains()
        {
            var s = new Substring("_Hello", 1, 2);
            s.Contains('_').Should().BeFalse();
            s.Contains('H').Should().BeTrue();
            s.Contains('e').Should().BeTrue();
            s.Contains('l').Should().BeFalse();
            s.Contains('o').Should().BeFalse();
            s.Contains('m').Should().BeFalse();
        }

        [TestMethod]
        public void SubstringContainsWithInvalidFunc()
        {
            var s = new Substring("Hello", 0);
            Action contains = () => s.Contains(null);
            contains.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void SubstringContainsWithFunc()
        {
            var s = new Substring("_Hello ", 2, 2);
            s.Contains(char.IsWhiteSpace).Should().BeFalse();
            s.Contains(char.IsUpper).Should().BeFalse();
            s.Contains(char.IsLower).Should().BeTrue();
            s.Contains(c => c == 'e').Should().BeTrue();
        }

        [TestMethod]
        public void SubstringIndexOf()
        {
            var s = new Substring("_Hello", 1, 2);
            s.IndexOf('_').Should().BeNegative();
            s.IndexOf('H').Should().Be(1);
            s.IndexOf('e').Should().Be(2);
            s.IndexOf('l').Should().BeNegative();
            s.IndexOf('o').Should().BeNegative();
            s.IndexOf('m').Should().BeNegative();
        }

        [TestMethod]
        public void DifferentSubstringsAreNotEqual()
        {
            var ss1 = new Substring("Hello", 2, 1);
            ss1.ToString().Should().Be("l");

            var ss2 = new Substring("Hello", 3, 1);
            ss2.ToString().Should().Be("l");

            ss1.Equals(ss2).Should().BeFalse();
            ss1.Equals(ss2 as object).Should().BeFalse();
            (ss1 == ss2).Should().BeFalse();
            (ss1 != ss2).Should().BeTrue();
        }

        [TestMethod]
        public void EquivalentSubstringsAreEqual()
        {
            const string s1 = "Hello";
            const string s2 = "Hello";

            var ss1 = new Substring(s1, 2);
            var ss2 = new Substring(s2, 2);

            ss1.Equals(ss2).Should().BeTrue();
            ss1.Equals(ss2 as object).Should().BeTrue();
            (ss1 == ss2).Should().BeTrue();
            (ss1 != ss2).Should().BeFalse();
        }

        [TestMethod]
        public void SubstringDoesNotEqualEquivalentString()
        {
            const string s = "Hello";
            var ss = new Substring(s, 0);

            ss.Equals(s).Should().BeFalse();
        }

        [TestMethod]
        public void GetHashCodeIsDifferentForDifferentSubstrings()
        {
            var ss1 = new Substring("Hello", 2, 1);
            ss1.ToString().Should().Be("l");

            var ss2 = new Substring("Hello", 3, 1);
            ss2.ToString().Should().Be("l");

            ss1.GetHashCode().Should().NotBe(ss2.GetHashCode());
        }

        [TestMethod]
        public void GetHashCodeIsSameForEquivalentSubstrings()
        {
            const string s1 = "Hello";
            const string s2 = "Hello";

            var ss1 = new Substring(s1, 2);
            var ss2 = new Substring(s2, 2);

            ss1.GetHashCode().Should().Be(ss2.GetHashCode());
        }
    }
}
