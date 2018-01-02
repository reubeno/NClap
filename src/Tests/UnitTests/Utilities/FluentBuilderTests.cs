using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class FluentBuilderTests
    {
        private class Boxed<T>
        {
            public T Value { get; set; }
        }

        [TestMethod]
        public void TestBuilderWithNoTransformationsAppliesToStartingState()
        {
            var startingValue = Any.Int();
            var builder = new FluentBuilder<int>(startingValue);
            builder.Apply().Should().Be(startingValue);
        }

        [TestMethod]
        public void TestBuilderImplicitlyCoercesFromStartingState()
        {
            var startingValue = Any.Int();
            FluentBuilder<int> builder = startingValue;
            builder.Apply().Should().Be(startingValue);
        }

        [TestMethod]
        public void TestBuilderImplicitlyCoercesToAppliedResult()
        {
            var startingValue = Any.Int();
            var builder = new FluentBuilder<int>(startingValue);
            int coerced = builder;
            builder.Apply().Should().Be(coerced);
        }

        [TestMethod]
        public void TestBuilderAppliesTransformationsInCorrectOrder()
        {
            var startingValue = 20;
            FluentBuilder<Boxed<int>> builder = new Boxed<int> { Value = startingValue };
            builder.AddTransformer(v => v.Value /= 10);
            builder.AddTransformer(v => v.Value -= 2);
            builder.Apply().Value.Should().Be(0);
        }
    }
}
