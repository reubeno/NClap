using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;
using System;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class PropertyWithSimulatedFallbackTests
    {
        [TestMethod]
        public void TestThatExceptionThrownWhenDefaultValueIsInvalid()
        {
            Action a = () =>
            {
                var prop = new PropertyWithSimulatedFallback<int>(
                    () => throw new NotImplementedException(),
                    value => throw new NotImplementedException(),
                    ex => true,
                    initialFallbackValue: Any.NegativeInt(),
                    fallbackValidator: value => value > 0);
            };

            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestThatRetrievalWorksWhenGetterSupportsIt()
        {
            var anyInt = Any.Int();
            var prop = new PropertyWithSimulatedFallback<int>(
                () => anyInt, value => throw new NotSupportedException(), ex => false);

            prop.Value.Should().Be(anyInt);
        }

        [TestMethod]
        public void TestThatRetrievalThrowsWhenGetterThrowsExceptionNotAllowed()
        {
            var prop = new PropertyWithSimulatedFallback<int>(
                () => throw new NotSupportedException(), value => { }, ex => false);

            prop.Invoking(p => { var _ = p.Value; }).Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void TestThatRetrievalYieldsDefaultValueWhenGetterThrowsAllowedException()
        {
            var anyInt = Any.Int();
            var prop = new PropertyWithSimulatedFallback<int>(
                () => throw new NotSupportedException(), value => { }, ex => true, anyInt);

            prop.Value.Should().Be(anyInt);
        }

        [TestMethod]
        public void TestThatStoringWorksWhenSetterSupportsIt()
        {
            var anyInt = Any.PositiveInt();
            int storedValue = 0;

            var prop = new PropertyWithSimulatedFallback<int>(
                () => throw new NotSupportedException(), value => { storedValue = value; }, ex => false);

            prop.Value = anyInt;
            storedValue.Should().Be(anyInt);
        }

        [TestMethod]
        public void TestThatStoringThrowsWhenSetterThrowsExceptionNotAllowed()
        {
            var prop = new PropertyWithSimulatedFallback<int>(
                () => throw new NotImplementedException(), value => throw new NotSupportedException(), ex => ex is NotImplementedException);

            prop.Invoking(p => p.Value = Any.Int()).Should().Throw<NotSupportedException>();
            prop.Value.Should().Be(0);
        }

        [TestMethod]
        public void TestThatStoringDoesNotThrowWhenSetterThrowsAllowedException()
        {
            var anyInt = Any.Int();
            var anyOtherInt = anyInt + 1;
            var prop = new PropertyWithSimulatedFallback<int>(
                () => throw new NotSupportedException(), value => throw new NotSupportedException(), ex => true, anyOtherInt);

            prop.Value = anyInt;
            prop.Value.Should().Be(anyInt);
        }

        [TestMethod]
        public void TestThatStoringThrowsWhenInputValueDoesNotPassValidation()
        {
            var prop = new PropertyWithSimulatedFallback<int>(
                () => throw new NotSupportedException(),
                value => throw new NotSupportedException(),
                ex => true,
                fallbackValidator: value => value >= 0);

            prop.Invoking(p => p.Value = Any.NegativeInt())
                .Should().Throw<ArgumentOutOfRangeException>();

            prop.Value.Should().Be(default(int));
        }
    }
}
