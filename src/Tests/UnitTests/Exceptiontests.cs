using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NClap.Tests
{
    public class ExceptionTests<T> where T : Exception
    {
        private const string AnyMessage = "Any message";

        [TestMethod]
        public void TestThatClassHasParameterlessConstructorAndItDoesNotThrow()
        {
            var constructor = Type.GetConstructor(Array.Empty<Type>());
            constructor.Invoking(c => c.Invoke(Array.Empty<object>())).Should().NotThrow();
        }

        [TestMethod]
        public void TestThatParameterlessConstructorYieldsExceptionWithNonEmptyMessage()
        {
            var ex = (T)Type.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
            ex.Message.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public void TestThatClassHasConstructorTakingAMessageAndItDoesNotThrow()
        {
            var constructor = Type.GetConstructor(new[] { typeof(string) });
            constructor.Invoking(c => c.Invoke(new[] { AnyMessage })).Should().NotThrow();
        }

        [TestMethod]
        public void TestThatMessageConstructorCorrectlyEmbedsMessage()
        {
            var ex = (T)Type.GetConstructor(new[] { typeof(string) }).Invoke(new[] { AnyMessage });
            ex.Message.Should().Contain(AnyMessage);
        }

        [TestMethod]
        public void TestThatClassHasConstructorTakingAMessageAndInnerExceptionAndItDoesNotThrow()
        {
            var constructor = Type.GetConstructor(new[] { typeof(string), typeof(Exception) });
            constructor.Invoking(c => c.Invoke(new object[] { AnyMessage, AnyInnerException })).Should().NotThrow();
        }

        [TestMethod]
        public void TestThatProvidedInnerExceptionIsAccessibleInConstructedObject()
        {
            var ex = (T)Type.GetConstructor(new[] { typeof(string), typeof(Exception) })
                .Invoke(new object[] { AnyMessage, AnyInnerException } );
            ex.InnerException.Should().BeSameAs(AnyInnerException);
            ex.Message.Should().Contain(AnyMessage);
        }

        private static Exception AnyInnerException { get; } = new NotImplementedException();

        private static Type Type => typeof(T);
    }
}
