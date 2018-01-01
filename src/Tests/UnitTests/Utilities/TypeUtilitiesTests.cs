using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;
using System;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class TypeUtilitiesTests
    {
        private class TestClass
        {
            public TestClass()
            {
            }

            public TestClass(TestClass tc, string st)
            {
            }

            public TestClass(string st, TestClass tc)
            {
            }
        }

        [TestMethod]
        public void TestThatATypeIsEffectivelyTheSameAsItself()
        {
            GetType().IsEffectivelySameAs(GetType()).Should().BeTrue();
        }

        [TestMethod]
        public void TestThatATypeIsNotEffectivelyTheSameAsAVeryDifferentType()
        {
            GetType().IsEffectivelySameAs(typeof(int)).Should().BeFalse();
        }

        [TestMethod]
        public void GetConstructorIgnoresParameterlessConstructorWhenAsked()
        {
            Action find = () => typeof(TestClass).GetConstructor<TestClass>(
                new object[] { }, considerParameterlessConstructor: false);

            find.Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void GetConstructorFindsParameterlessConstructorWhenAsked()
        {
            var constructor = typeof(TestClass).GetConstructor<TestClass>(
                new object[] { }, considerParameterlessConstructor: true);

            constructor.Should().NotBeNull();

            var instance = constructor;
            constructor().Should().NotBeNull().And.BeOfType<TestClass>();
        }

        [TestMethod]
        public void GetConstructorThrowsWhenTooMultipleConstructorsMatch()
        {
            Action find = () => typeof(TestClass).GetConstructor<TestClass>(
                new object[] { new TestClass(), "test" },
                considerParameterlessConstructor: false);

            find.Should().Throw<NotSupportedException>();
        }
    }
}
