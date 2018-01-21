using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Types;
using NSubstitute;
using System;
using System.Reflection;

namespace NClap.Tests.Types
{
    [TestClass]
    public class EnumArgumentValueTests
    {
        [TestMethod]
        public void TestThatConstructorSucceedsEvenIfFieldValueIsOnlyAvailableAsRawConstant()
        {
            var anyInt = Any.PositiveInt();

            var fieldInfo = Substitute.For<FieldInfo>();
            fieldInfo.GetValue(Arg.Any<object>()).Returns(o => throw new InvalidOperationException());
            fieldInfo.GetRawConstantValue().ReturnsForAnyArgs(anyInt);

            var value = new EnumArgumentValue(fieldInfo);
            value.Value.Should().Be(anyInt);
        }

        [TestMethod]
        public void TestThatConstructorThrowsWhenEvenRawConstantValueIsNotAvailable()
        {
            var anyInt = Any.PositiveInt();

            var fieldInfo = Substitute.For<FieldInfo>();
            fieldInfo.GetValue(Arg.Any<object>()).Returns(o => throw new InvalidOperationException());
            fieldInfo.When(f => f.GetRawConstantValue()).Do(f => throw new NotImplementedException());

            Action a = () => new EnumArgumentValue(fieldInfo);
            a.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public void TestThatConstructorThrowsWhenValueIsNull()
        {
            var fieldInfo = Substitute.For<FieldInfo>();
            fieldInfo.GetValue(Arg.Any<object>()).ReturnsForAnyArgs(null);

            Action a = () => new EnumArgumentValue(fieldInfo);
            a.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
