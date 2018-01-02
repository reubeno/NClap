using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Exceptions;
using NClap.Metadata;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class ArgumentValueAttributeTests
    {
        [TestMethod]
        public void TestThatLongNameAcceptsNull()
        {
            var attrib = new ArgumentValueAttribute();

            attrib.Invoking(a => a.LongName = null)
                .Should().NotThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void TestThatLongNameThrowsOnEmptyString()
        {
            var attrib = new ArgumentValueAttribute();

            attrib.Invoking(a => a.LongName = string.Empty)
                .Should().Throw<InvalidArgumentSetException>();
        }
    }
}
