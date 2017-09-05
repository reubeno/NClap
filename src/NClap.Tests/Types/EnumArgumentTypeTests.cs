using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Types;

namespace NClap.Tests.Types
{
    [TestClass]
    public class EnumArgumentTypeTests
    {
        enum CaselessSameMemberNames
        {
            Foo,
            foo
        }

        enum ConflictingAttributes
        {
            [ArgumentValue(ShortName = "s")]
            Foo,

            [ArgumentValue(ShortName = "s")]
            Bar
        }

        enum SampleEnum
        {
            [ArgumentValue(LongName = "Fo", ShortName = "f")]
            Foo,

            [ArgumentValue(ShortName = "o")]
            Other,

            [ArgumentValue(Flags = ArgumentValueFlags.Disallowed)]
            Unusable,

            [ArgumentValue(Flags = ArgumentValueFlags.Hidden)]
            NotPublicized
        }

        [TestMethod]
        public void EnumWithCustomLongAndShortNames()
        {
            var type = EnumArgumentType.Create(typeof(SampleEnum));
            type.SyntaxSummary.Should().Be("{Fo|o}");
        }

        [TestMethod]
        public void DisallowedEnumValue()
        {
            var type = EnumArgumentType.Create(typeof(SampleEnum));
            type.TryParse(ArgumentParseContext.Default, "Unusable", out object o).Should().BeFalse();
        }

        [TestMethod]
        public void EnumWithCaseInsensitivelyEqualMemberNames()
        {
            Action typeFactory = () => EnumArgumentType.Create(typeof(CaselessSameMemberNames));
            typeFactory.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void ConflictingAttributesOnMembers()
        {
            Action typeFactory = () => EnumArgumentType.Create(typeof(ConflictingAttributes));
            typeFactory.ShouldThrow<ArgumentOutOfRangeException>();
        }
    }
}
