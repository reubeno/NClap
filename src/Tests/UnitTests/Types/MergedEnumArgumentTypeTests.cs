using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Types;
using System;
using System.Linq;

namespace NClap.Tests.Types
{
    [TestClass]
    public class MergedEnumArgumentTypeTests
    {
        private enum TestEnum1
        {
            SomeValue
        }

        private enum TestEnum2
        {
            SomeOtherValue
        }

        [TestMethod]
        public void TestThatExceptionThrownWhenMergingZeroTypes()
        {
            Action a = () => new MergedEnumArgumentType(Array.Empty<Type>());
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestThatMergedEnumArgumentTypesAreCreatableFromSingleEnumType()
        {
            var argType = new MergedEnumArgumentType(new[] { typeof(TestEnum1) });
            argType.Type.Should().Be(typeof(object));
        }

        [TestMethod]
        public void TestThatExceptionThrownWhenMergingNonEnumType()
        {
            Action a = () => new MergedEnumArgumentType(new[] { typeof(TestEnum1), typeof(int) });
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestThatMergingMultipleEnumTypesIsAllowed()
        {
            Action a = () => new MergedEnumArgumentType(new[] { typeof(TestEnum1), typeof(TestEnum2) });
            a.Should().NotThrow();
        }

        [TestMethod]
        public void TestThatAnEnumTypeMayNotBeMergedWithItself()
        {
            Action a = () => new MergedEnumArgumentType(new[] { typeof(TestEnum1), typeof(TestEnum1) });
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestThatMergedEnumTypesContainUnionOfAllValues()
        {
            var argType = new MergedEnumArgumentType(new[] { typeof(TestEnum1), typeof(TestEnum2) });
            var values = argType.GetValues()
                .OrderBy(v => v.LongName)
                .ToList();

            values[0].LongName.Should().Be(nameof(TestEnum2.SomeOtherValue));
            values[1].LongName.Should().Be(nameof(TestEnum1.SomeValue));
        }
    }
}
