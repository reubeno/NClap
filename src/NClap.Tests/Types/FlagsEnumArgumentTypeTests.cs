using System;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NClap.Types;

namespace NClap.Tests.Types
{
    [TestClass]
    public class FlagsEnumArgumentTypeTests
    {
        enum PlainEnum
        {
            SomeValue
        }

        [Flags]
        enum MyFlags
        {
            None = 0,
            SomeFlag = 0x1,
            SomeOtherFlag = 0x2,
            SomeThirdFlag = 0x4,
            All = SomeFlag | SomeOtherFlag | SomeThirdFlag
        }

        [TestMethod]
        public void NonEnum()
        {
            Action typeFactory = () => new FlagsEnumArgumentType(typeof(int));
            typeFactory.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void NonFlags()
        {
            Action typeFactory = () => new FlagsEnumArgumentType(typeof(PlainEnum));
            typeFactory.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void Flags()
        {
            Action typeFactory = () => new FlagsEnumArgumentType(typeof(MyFlags));
            typeFactory.ShouldNotThrow();
        }

        [TestMethod]
        public void Format()
        {
            var type = new FlagsEnumArgumentType(typeof(MyFlags));
            type.Format(MyFlags.None).Should().Be("None");
            type.Format(MyFlags.SomeFlag).Should().Be("SomeFlag");
            type.Format(MyFlags.SomeOtherFlag).Should().Be("SomeOtherFlag");
            type.Format(MyFlags.SomeThirdFlag).Should().Be("SomeThirdFlag");
            type.Format(MyFlags.SomeFlag | MyFlags.SomeOtherFlag).Should().Be("SomeFlag|SomeOtherFlag");
            type.Format(MyFlags.All).Should().Be("All");

            Action formatAction = () => type.Format(0xFF);
            formatAction.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void Parse()
        {
            var type = new FlagsEnumArgumentType(typeof(MyFlags));
            var c = ArgumentParseContext.Default;


            type.TryParse(c, string.Empty, out object flags).Should().BeFalse();
            flags.Should().BeNull();

            type.TryParse(c, "None", out flags).Should().BeTrue();
            flags.Should().Be(MyFlags.None);

            type.TryParse(c, "SomeFlag", out flags).Should().BeTrue();
            flags.Should().Be(MyFlags.SomeFlag);

            type.TryParse(c, "someFlag", out flags).Should().BeTrue();
            flags.Should().Be(MyFlags.SomeFlag);

            type.TryParse(c, "SomeOtherFlag", out flags).Should().BeTrue();
            flags.Should().Be(MyFlags.SomeOtherFlag);

            type.TryParse(c, "SomeThirdFlag", out flags).Should().BeTrue();
            flags.Should().Be(MyFlags.SomeThirdFlag);

            type.TryParse(c, "SomeOtherFlag|SomeOtherFlag", out flags).Should().BeTrue();
            flags.Should().Be(MyFlags.SomeOtherFlag);

            type.TryParse(c, "SomeFlag|SomeOtherFlag", out flags).Should().BeTrue();
            flags.Should().Be(MyFlags.SomeFlag | MyFlags.SomeOtherFlag);

            type.TryParse(c, "SomeOtherFlag|SomeFlag", out flags).Should().BeTrue();
            flags.Should().Be(MyFlags.SomeFlag | MyFlags.SomeOtherFlag);

            type.TryParse(c, "SomeFlag|SomeOtherFlag|SomeThirdFlag", out flags).Should().BeTrue();
            flags.Should().Be(MyFlags.All);

            type.TryParse(c, "All", out flags).Should().BeTrue();
            flags.Should().Be(MyFlags.All);
        }
    }
}
