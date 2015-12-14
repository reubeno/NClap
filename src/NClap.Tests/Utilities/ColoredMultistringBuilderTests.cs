using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class ColoredMultistringBuilderTests
    {
        [TestMethod]
        public void SimpleUsage()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append("Hello");
            builder.Append(", world");

            builder.ToString().Should().Be("Hello, world");
        }

        [TestMethod]
        public void AppendingMultipleStrings()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(new ColoredString[] {"Hello, ", "world"});

            builder.ToString().Should().Be("Hello, world");
        }

        [TestMethod]
        public void AppendingMultipleStringsWithLines()
        {
            var builder = new ColoredMultistringBuilder();
            builder.AppendLine(new ColoredString[] { "Hello, ", "world" });

            builder.ToString().Should().Be("Hello, world" + Environment.NewLine);
        }
    }
}
