using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class ColoredMultistringTests
    {
        [TestMethod]
        public void ConstructorCanTakeString()
        {
            var s = new ColoredMultistring("xyz");
            s.Content.Should().HaveCount(1);
            s.Content[0].Content.Should().Be("xyz");
        }

        [TestMethod]
        public void FromStringThrowsOnNull()
        {
            Action action = () => ColoredMultistring.FromString(null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void LengthIsCorrect()
        {
            var s = CreateCMS("xy", "zzy");
            s.Length.Should().Be(5);
        }

        [TestMethod]
        public void IndexingMethodWorks()
        {
            var s = CreateCMS("xy", "zzy");
            s[0].Should().Be('x');
            s[1].Should().Be('y');
            s[2].Should().Be('z');
            s[3].Should().Be('z');
            s[4].Should().Be('y');
        }

        [TestMethod]
        public void InvalidIndexThrows()
        {
            var s = CreateCMS("xy", "zzy");

            Action badAccess = () => { var x = s[-1]; };
            badAccess.ShouldThrow<IndexOutOfRangeException>();

            badAccess = () => { var x = s[5]; };
            badAccess.ShouldThrow<IndexOutOfRangeException>();
        }

        [TestMethod]
        public void TrimEndDoesNothingIfNoTrailingWhitespace()
        {
            var s = CreateCMS("xy", "zzy");

            var trimmed = s.TrimEnd();
            var trimmedContent = trimmed.Should().BeOfType<ColoredMultistring>().Which.Content;

            trimmedContent[0].Content.Should().Be("xy");
            trimmedContent[1].Content.Should().Be("zzy");
        }

        [TestMethod]
        public void TrimEndRemoves0LenPiecesFromEnd()
        {
            var s = CreateCMS(string.Empty, "xy", string.Empty, string.Empty);

            var trimmed = (ColoredMultistring)s.TrimEnd();
            trimmed.Content.Should().HaveCount(2);
            trimmed.Content[0].Content.Should().Be(string.Empty);
            trimmed.Content[1].Content.Should().Be("xy");
        }

        [TestMethod]
        public void TrimEndRemovesWhiteSpaceOnlyPiecesFromEnd()
        {
            var s = CreateCMS("xy", "  ", "\t\t");

            var trimmed = (ColoredMultistring)s.TrimEnd();
            trimmed.Content.Should().HaveCount(1);
            trimmed.Content[0].Content.Should().Be("xy");
        }

        [TestMethod]
        public void TrimEndRemovesWhiteSpaceFromEndOfMixedPiece()
        {
            var s = CreateCMS("x yz \n");

            var trimmed = (ColoredMultistring)s.TrimEnd();
            trimmed.Content.Should().HaveCount(1);
            trimmed.Content[0].Content.Should().Be("x yz");
        }

        [TestMethod]
        public void TrimEndRemovesWhiteSpaceAcrossMultiplePieces()
        {
            var s = CreateCMS("x yz \n", "\r\n");

            var trimmed = (ColoredMultistring)s.TrimEnd();
            trimmed.Content.Should().HaveCount(1);
            trimmed.Content[0].Content.Should().Be("x yz");
        }

        [TestMethod]
        public void SubstringThrowsOnLengthTooLong()
        {
            var s = CreateCMS("xyz");
            Action action = () => { var x = s.Substring(1, 4); };
            action.ShouldThrow<IndexOutOfRangeException>();
        }

        [TestMethod]
        public void LastIndexOfAnyThrowsOnNegativeStartIndex()
        {
            var s = CreateCMS("xyz");
            Action action = () => s.LastIndexOfAny(new[] { 'x' }, -1, 1);
            action.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void LastIndexOfAnyThrowsOnStartIndexPastEndOfString()
        {
            var s = CreateCMS("xyz");
            Action action = () => s.LastIndexOfAny(new[] { 'x' }, 10, 2);
            action.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void LastIndexOfAnyThrowsOnNegativeCount()
        {
            var s = CreateCMS("xyz");
            Action action = () => s.LastIndexOfAny(new[] { 'x' }, 1, -1);
            action.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void LastIndexOfAnyCanMatchOnLastChar()
        {
            var s = CreateCMS("xyz");
            s.LastIndexOfAny(new[] { 'z' }, 2, 1).Should().Be(2);
        }

        [TestMethod]
        public void LastIndexOfAnyReturnsNegativeValueOnNoMatch()
        {
            var s = CreateCMS("xyz");
            s.LastIndexOfAny(new[] { 'w' }, 2, 1).Should().BeNegative();
        }

        [TestMethod]
        public void LastIndexOfAnyWorksAcrossPieces()
        {
            var s = CreateCMS("xyz", "zy");
            s.LastIndexOfAny(new[] { 'x' }, 4, 5).Should().Be(0);
        }

        [TestMethod]
        public void LastIndexOfAnyFindsLastInstance()
        {
            var s = CreateCMS("xyz", "zy");
            s.LastIndexOfAny(new[] { 'z' }, 4, 5).Should().Be(3);
        }

        [TestMethod]
        public void LastIndexOfAnyStartsLookingAtStartIndex()
        {
            var s = CreateCMS("xyz", "zy");
            s.LastIndexOfAny(new[] { 'z' }, 2, 3).Should().Be(2);
        }

        private static ColoredMultistring CreateCMS(params string[] values) =>
            new ColoredMultistring(values.Select(s => new ColoredString(s)));
    }
}
