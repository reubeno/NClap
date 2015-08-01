using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Types;
using NSubstitute;

namespace NClap.Tests.Types
{
    [TestClass]
    public class EnumArgumentTypeTests
    {
        enum CaselessSameMemberNames
        {
            Foo,
            foo
        }

        enum ConflictingAttributes
        {
            [ArgumentValue(ShortName = "s")]
            Foo,

            [ArgumentValue(ShortName = "s")]
            Bar
        }

        [TestMethod]
        public void EnumWithCaseInsensitivelyEqualMemberNames()
        {
            Action typeFactory = () => EnumArgumentType.Create(typeof(CaselessSameMemberNames));
            typeFactory.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void ConflictingAttributesOnMembers()
        {
            Action typeFactory = () => EnumArgumentType.Create(typeof(ConflictingAttributes));
            typeFactory.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void EnumWithNoUnderlyingType()
        {
            var enumType = Substitute.For<Type>();
            enumType.IsEnum.Returns(true);
            enumType.GetEnumUnderlyingType().Returns((Type)null);

            Action typeFactory = () => EnumArgumentType.Create(enumType);
            typeFactory.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void EnumWithNoArgType()
        {
            var enumType = Substitute.For<Type>();
            enumType.IsEnum.Returns(true);
            enumType.GetEnumUnderlyingType().Returns(this.GetType());

            Action typeFactory = () => EnumArgumentType.Create(enumType);
            typeFactory.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void EnumWithNonIntegerArgType()
        {
            var enumType = Substitute.For<Type>();
            enumType.IsEnum.Returns(true);
            enumType.GetEnumUnderlyingType().Returns(typeof(string));

            Action typeFactory = () => EnumArgumentType.Create(enumType);
            typeFactory.ShouldThrow<ArgumentOutOfRangeException>();
        }
    }
}
