using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Exceptions;
using NClap.Metadata;
using NClap.Tests.Metadata;
using System;

namespace NClap.Tests.Exceptions
{
    [TestClass]
    public class InvalidArgumentSetExceptionTests
    {
        public class SampleArguments
        {
            [NamedArgument] public int Value;
        }

        [TestMethod]
        public void ParameterlessConstructor()
        {
            var exn = new InvalidArgumentSetException();
            exn.Message.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public void ArgumentConstructor()
        {
            const string innerMessage = "Something message-like.";

            var arg = ArgumentTests.GetArgument(typeof(SampleArguments), "Value");
            var exn = new InvalidArgumentSetException(arg, innerMessage);
            exn.Argument.Should().BeSameAs(arg);
            exn.InnerMessage.Should().Be(innerMessage);
            exn.Message.Should().Contain(innerMessage);
        }

        [TestMethod]
        public void TestThatProvidedInnerExceptionIsAccessibleInConstructedObject()
        {
            const string anyMessage = "Some message";
            var innerExn = new NotImplementedException();
            var exn = new InvalidArgumentSetException(anyMessage, innerExn);
            exn.InnerException.Should().BeSameAs(innerExn);
            exn.InnerMessage.Should().Be(anyMessage);
        }
    }
}
