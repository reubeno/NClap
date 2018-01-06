﻿using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class MutablePropertyInfoTests
    {
        class TestObject<T>
        {
            public T Value { get; set; }
        }

        class TestThrowingObject<T>
        {
            public T Value
            {
                get => default;
                set => throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        [TestMethod]
        public void ConversionFails()
        {
            var prop = new MutablePropertyInfo(typeof(TestObject<Guid>).GetTypeInfo().GetProperty("Value"));
            var obj = new TestObject<Guid>();
            Action setter = () => prop.SetValue(obj, 3.0);
            setter.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ConversionSucceeds()
        {
            var prop = new MutablePropertyInfo(typeof(TestObject<int>).GetTypeInfo().GetProperty("Value"));
            var obj = new TestObject<int>();
            Action setter = () => prop.SetValue(obj, 3L);
            setter.Should().NotThrow();
        }

        [TestMethod]
        public void ConversionSucceedsButSettingFails()
        {
            var prop = new MutablePropertyInfo(typeof(TestThrowingObject<int>).GetTypeInfo().GetProperty("Value"));
            var obj = new TestThrowingObject<int>();
            Action setter = () => prop.SetValue(obj, 3L);
            setter.Should().Throw<ArgumentException>();
        }
    }
}
