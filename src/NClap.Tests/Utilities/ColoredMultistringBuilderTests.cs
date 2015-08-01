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
    }
}
