using System;
using NClap.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using NClap.Types;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class ReflectionUtilitiesTests
    {
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
            object value;

            typeof(int).IsImplicitlyConvertibleFrom(0xFFFFFFFFFFFF).Should().BeFalse();
            typeof(int).TryConvertFrom(0xFFFFFFFFFFFF, out value).Should().BeFalse();
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
            object obj;

            typeof(int).TryConvertFrom(0.0, out obj).Should().BeTrue();
            obj.Should().BeOfType<int>().And.Be(0);

            typeof(int).TryConvertFrom(true, out obj).Should().BeTrue();
            obj.Should().BeOfType<int>().And.Be(1);

            typeof(string).TryConvertFrom(new FileSystemPath("MyPath"), out obj).Should().BeTrue();
            obj.Should().BeOfType<string>().And.Be("MyPath");

            typeof(FileSystemPath).TryConvertFrom("MyPath", out obj).Should().BeTrue();
            obj.Should().BeOfType(typeof(FileSystemPath)).And.Be(new FileSystemPath("MyPath"));
        }
    }
}
