using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Exceptions;

namespace NClap.Tests.Exceptions
{
    [TestClass]
    public class InvalidCommandExceptionTests
    {
        [TestMethod]
        public void TestThatInnerMessageIsEmbeddedInConstructedObject()
        {
            const string anyMessage = "Some message";
            var exn = new InvalidCommandException(anyMessage);
            exn.Message.Should().Be(anyMessage);
        }
    }
}
