using System;
using NClap.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

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
        }

        [TestMethod]
        public void ConvertibleValues()
        {
            typeof(int).IsImplicitlyConvertibleFrom(0.0).Should().BeTrue();
            typeof(int).IsImplicitlyConvertibleFrom(true).Should().BeTrue();
        }
    }
}
