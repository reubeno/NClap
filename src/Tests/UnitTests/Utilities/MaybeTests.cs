using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class MaybeTests
    {
        private const string AnyString = "Any string";

        [TestMethod]
        public void TestThatNoneValueAsMaybeYieldsNoValue()
        {
            Maybe<string> value = new None();
            value.HasValue.Should().BeFalse();
            value.IsNone.Should().BeTrue();
            value.Invoking(v => { var _ = v.Value; }).Should().Throw<InvalidOperationException>();
            value.GetValueOrDefault(AnyString).Should().Be(AnyString);
            value.GetValueOrDefault().Should().BeNull();
        }

        [TestMethod]
        public void TestThatSomeNullValueAsMaybeYieldsNullValue()
        {
            var value = new Maybe<string>(null);
            value.HasValue.Should().BeTrue();
            value.IsNone.Should().BeFalse();
            value.Value.Should().BeNull();
            value.GetValueOrDefault(AnyString).Should().BeNull();
            value.GetValueOrDefault().Should().BeNull();
        }

        [TestMethod]
        public void TestThatSomeNonNulValueAsMaybeYieldsCorrectValue()
        {
            const string anyOtherString = "Any other string";

            var value = new Maybe<string>(AnyString);
            value.HasValue.Should().BeTrue();
            value.IsNone.Should().BeFalse();
            value.Value.Should().BeSameAs(AnyString);
            value.GetValueOrDefault(anyOtherString).Should().BeSameAs(AnyString);
            value.GetValueOrDefault().Should().BeSameAs(AnyString);
        }
    }
}
