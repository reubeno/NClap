using System;
using NClap.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace NClap.Tests.Types
{
    [TestClass]
    public class IntegerTypeTests
    {
        [TestMethod]
        public void AddTests()
        {
            Add(3, 5).Should().Be(8);
            Add(-1, 7).Should().Be(6);
            Add((byte)255, (byte)1).Should().Be(0);
        }

        [TestMethod]
        public void AddWithOverflowCheckTests()
        {
            AddWithOverflowCheck(3, 5).Should().Be(8);

            Action add = () => AddWithOverflowCheck(0xFFFFFFFF, 1U);
            add.Should().Throw<OverflowException>();
        }

        [TestMethod]
        public void SubtractTests()
        {
            Subtract(3, 5).Should().Be(-2);
            Subtract(-1, 7).Should().Be(-8);
            Subtract((byte)0, (byte)1).Should().Be((byte)255);
        }

        [TestMethod]
        public void SubtractWithOverflowCheckTests()
        {
            SubtractWithOverflowCheck(3, 5).Should().Be(-2);

            Action subtract = () => SubtractWithOverflowCheck(3U, 5U);
            subtract.Should().Throw<OverflowException>();
        }

        [TestMethod]
        public void MultiplyTests()
        {
            Multiply(3, 5).Should().Be(15);
            Multiply(-1, 5).Should().Be(-5);
            Multiply(0xFFFFFFFFFFUL, 0xFFFFFFFFFFUL).Should().Be(0xFFFFFE0000000001);
        }

        [TestMethod]
        public void MultiplyWithOverflowCheckTests()
        {
            MultiplyWithOverflowCheck(3, 5).Should().Be(15);

            Action multiply = () => MultiplyWithOverflowCheck(0xFFFFFFFF, 2U);
            multiply.Should().Throw<OverflowException>();
        }

        [TestMethod]
        public void DivideTests()
        {
            Divide(0, 1).Should().Be(0);
            Divide(42, 7).Should().Be(6);
        }

        [TestMethod]
        public void OrTests()
        {
            Or(0, 0x1).Should().Be(0x1);
            Or(0x1, 0x4).Should().Be(0x5);
            Or(0x3, 0x7).Should().Be(0x7);
        }

        [TestMethod]
        public void AndTests()
        {
            And(0, 0x1).Should().Be(0x0);
            And(0x1, 0x4).Should().Be(0x0);
            And(0x3, 0x7).Should().Be(0x3);
        }

        [TestMethod]
        public void XorTests()
        {
            Xor(0, 0x1).Should().Be(0x1);
            Xor(0x1, 0x4).Should().Be(0x5);
            Xor(0x3, 0x7).Should().Be(0x4);
        }

        [TestMethod]
        public void LessThanTests()
        {
            IsLessThan(0, 1).Should().BeTrue();
            IsLessThan(0, 0).Should().BeFalse();
            IsLessThan(1, 0).Should().BeFalse();

            IsLessThanOrEqualTo(0, 1).Should().BeTrue();
            IsLessThanOrEqualTo(0, 0).Should().BeTrue();
            IsLessThanOrEqualTo(1, 0).Should().BeFalse();
        }

        [TestMethod]
        public void GreaterThanTests()
        {
            IsGreaterThan(-7, -5).Should().BeFalse();
            IsGreaterThan(-5, -7).Should().BeTrue();
            IsGreaterThan(-6, -6).Should().BeFalse();

            IsGreaterThanOrEqualTo(-7, -5).Should().BeFalse();
            IsGreaterThanOrEqualTo(-5, -7).Should().BeTrue();
            IsGreaterThanOrEqualTo(-6, -6).Should().BeTrue();
        }

        [TestMethod]
        public void EqualToTests()
        {
            IsEqualTo(11, 11).Should().BeTrue();
            IsEqualTo(11, -11).Should().BeFalse();

            IsNotEqualTo(11, 11).Should().BeFalse();
            IsNotEqualTo(11, -11).Should().BeTrue();
        }

        private static bool IsLessThan<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return ty.IsLessThan(operand0, operand1);
        }

        private static bool IsLessThanOrEqualTo<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return ty.IsLessThanOrEqualTo(operand0, operand1);
        }

        private static bool IsGreaterThan<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return ty.IsGreaterThan(operand0, operand1);
        }

        private static bool IsGreaterThanOrEqualTo<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return ty.IsGreaterThanOrEqualTo(operand0, operand1);
        }

        private static bool IsEqualTo<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return ty.IsEqualTo(operand0, operand1);
        }

        private static bool IsNotEqualTo<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return ty.IsNotEqualTo(operand0, operand1);
        }
        
        private static T Add<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return (T)ty.Add(operand0, operand1);
        }

        private static T AddWithOverflowCheck<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return (T)ty.Add(operand0, operand1, true);
        }

        private static T Subtract<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return (T)ty.Subtract(operand0, operand1);
        }

        private static T SubtractWithOverflowCheck<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return (T)ty.Subtract(operand0, operand1, true);
        }

        private static T Multiply<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return (T)ty.Multiply(operand0, operand1);
        }

        private static T MultiplyWithOverflowCheck<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return (T)ty.Multiply(operand0, operand1, true);
        }

        private static T Divide<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return (T)ty.Divide(operand0, operand1);
        }

        private static T Or<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return (T)ty.Or(operand0, operand1);
        }

        private static T And<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return (T)ty.And(operand0, operand1);
        }

        private static T Xor<T>(T operand0, T operand1)
        {
            var ty = (IntegerArgumentType)ArgumentType.GetType(operand0.GetType());
            return (T)ty.Xor(operand0, operand1);
        }
    }
}
