using System;
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
                get => default(T);
                set => throw new ArgumentOutOfRangeException();
            }
        }

        [TestMethod]
        public void ConversionFails()
        {
            var prop = new MutablePropertyInfo(typeof(TestObject<Guid>).GetProperty("Value"));
            var obj = new TestObject<Guid>();
            Action setter = () => prop.SetValue(obj, 3.0);
            setter.ShouldThrow<ArgumentException>();
        }

        [TestMethod]
        public void ConversionSucceeds()
        {
            var prop = new MutablePropertyInfo(typeof(TestObject<int>).GetProperty("Value"));
            var obj = new TestObject<int>();
            Action setter = () => prop.SetValue(obj, 3L);
            setter.ShouldNotThrow();
        }

        [TestMethod]
        public void ConversionSucceedsButSettingFails()
        {
            var prop = new MutablePropertyInfo(typeof(TestThrowingObject<int>).GetProperty("Value"));
            var obj = new TestThrowingObject<int>();
            Action setter = () => prop.SetValue(obj, 3L);
            setter.ShouldThrow<ArgumentException>();
        }
    }
}
