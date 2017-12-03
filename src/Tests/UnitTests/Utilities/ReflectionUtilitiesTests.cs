using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class ReflectionUtilitiesTests
    {
        public const int TestField = 12;

        public int TestProperty { get; set; }

        class MyValue
        {
            public static implicit operator MyValue(int x)
            {
                throw new NotImplementedException();
            }
        }

        class MyOtherValue
        {
            public static implicit operator MyValue(MyOtherValue x)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void DefaultsOfBasicTypes()
        {
            typeof(int).GetDefaultValue().Should().Be(0);
            typeof(string).GetDefaultValue().Should().Be(null);
            typeof(Guid).GetDefaultValue().Should().Be(Guid.Empty);
        }

        [TestMethod]
        public void NonConvertibleValues()
        {

            typeof(int).IsImplicitlyConvertibleFrom(0xFFFFFFFFFFFF).Should().BeFalse();
            typeof(int).TryConvertFrom(0xFFFFFFFFFFFF, out object value).Should().BeFalse();
            value.Should().BeNull();

            typeof(int).IsImplicitlyConvertibleFrom(null).Should().BeFalse();
            typeof(int).TryConvertFrom(null, out value).Should().BeFalse();
            value.Should().BeNull();

            typeof(int).IsImplicitlyConvertibleFrom("abcd").Should().BeFalse();
            typeof(int).TryConvertFrom("abcd", out value).Should().BeFalse();
            value.Should().BeNull();

            typeof(MyValue).IsImplicitlyConvertibleFrom(7).Should().BeTrue();
            typeof(MyValue).TryConvertFrom((object)7, out value).Should().BeFalse();
            value.Should().BeNull();

            typeof(MyValue).IsImplicitlyConvertibleFrom("a").Should().BeFalse();
            typeof(MyValue).TryConvertFrom((object)"a", out value).Should().BeFalse();
            value.Should().BeNull();

            typeof(MyValue).IsImplicitlyConvertibleFrom(new MyOtherValue()).Should().BeTrue();
            typeof(MyValue).TryConvertFrom(new MyOtherValue(), out value).Should().BeFalse();
            value.Should().BeNull();
        }

        [TestMethod]
        public void ConvertibleValues()
        {
            typeof(int).IsImplicitlyConvertibleFrom(0.0).Should().BeTrue();
            typeof(int).IsImplicitlyConvertibleFrom(true).Should().BeTrue();

            typeof(string).IsImplicitlyConvertibleFrom(new FileSystemPath("MyPath")).Should().BeTrue();
            typeof(FileSystemPath).IsImplicitlyConvertibleFrom("MyPath").Should().BeTrue();
        }

        [TestMethod]
        public void ConvertValues()
        {

            typeof(int).TryConvertFrom(0.0, out object obj).Should().BeTrue();
            obj.Should().BeOfType<int>().And.Be(0);

            typeof(int).TryConvertFrom(true, out obj).Should().BeTrue();
            obj.Should().BeOfType<int>().And.Be(1);

            typeof(string).TryConvertFrom(new FileSystemPath("MyPath"), out obj).Should().BeTrue();
            obj.Should().BeOfType<string>().And.Be("MyPath");

            typeof(FileSystemPath).TryConvertFrom("MyPath", out obj).Should().BeTrue();
            obj.Should().BeOfType(typeof(FileSystemPath)).And.Be(new FileSystemPath("MyPath"));
        }

        [TestMethod]
        public void ToMutableMemberWorksOnFields()
        {
            var field = this.GetType().GetTypeInfo().GetField(nameof(TestField));
            field.Should().NotBeNull();

            var mutableInfo = field.ToMutableMemberInfo();

            mutableInfo.Should().NotBeNull();
            mutableInfo.Should().BeOfType<MutableFieldInfo>();
        }

        [TestMethod]
        public void ToMutableMemberWorksOnProperties()
        {
            var prop = this.GetType().GetTypeInfo().GetProperty(nameof(TestProperty));
            prop.Should().NotBeNull();

            var mutableInfo = prop.ToMutableMemberInfo();

            mutableInfo.Should().NotBeNull();
            mutableInfo.Should().BeOfType<MutablePropertyInfo>();
        }

        [TestMethod]
        public void ToMutableMemberThrowsOnMethods()
        {
            var method = this.GetType().GetTypeInfo().GetMethod(nameof(ToMutableMemberThrowsOnMethods));
            Action a = () => method.ToMutableMemberInfo();
            a.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
