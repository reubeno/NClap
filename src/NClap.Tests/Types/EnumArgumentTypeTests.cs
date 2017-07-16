using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Types;
using NSubstitute;
using System.Reflection;

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
