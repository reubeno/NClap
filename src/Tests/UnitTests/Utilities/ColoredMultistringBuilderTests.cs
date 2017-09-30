using System;
using System.Linq;
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

        [TestMethod]
        public void AppendingMultipleMultistrings()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append("Foo");
            builder.Append(CreateCMS(", bar", ", baz!"));

            builder.ToString().Should().Be("Foo, bar, baz!");
        }

        [TestMethod]
        public void AppendingMultipleMultistringsWithLines()
        {
            var builder = new ColoredMultistringBuilder();
            builder.AppendLine("Foo");
            builder.AppendLine(CreateCMS(", bar", ", baz!"));

            builder.ToString().Should().Be("Foo" + Environment.NewLine + ", bar, baz!" + Environment.NewLine);
        }

        private static ColoredMultistring CreateCMS(params string[] values) =>
            new ColoredMultistring(values.Select(s => new ColoredString(s)));
    }
}
