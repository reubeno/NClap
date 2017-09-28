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

        private static ColoredMultistring CreateCMS(params string[] values) =>
            new ColoredMultistring(values.Select(s => new ColoredString(s)));
    }
}
