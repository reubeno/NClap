using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class ColoredStringExtensionMethodsTests
    {
        [TestMethod]
        public void TestThatTransformThrowsOnNullFunc()
        {
            var anyCs = AnyColoredString();
            anyCs.Invoking(cs => cs.Transform(null)).Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatTransformPreservesColor()
        {
            var anyCs = AnyColoredString();
            const string anyString = "Something different";
            var updated = anyCs.Transform(_ => anyString);

            updated.IsSameColorAs(anyCs).Should().BeTrue();
        }

        private ColoredString AnyColoredString() =>
            new ColoredString("Some text", Any.Enum<ConsoleColor>(), Any.Enum<ConsoleColor>());
    }
}
