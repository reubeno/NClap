using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Utilities;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class StringValidationAttributeTests
    {
        [TestMethod]
        public void TryValidateThrowsOnNonString()
        {
            var attrib = new MustNotBeEmptyAttribute();

            Action a = () => attrib.TryValidate(CreateContext(), new StringValidationAttributeTests(), out string reason);
            a.ShouldThrow<InvalidCastException>();
        }

        [TestMethod]
        public void NonEmptyStringCorrectlyChecksAsNotBeingEmpty()
        {
            var attrib = new MustNotBeEmptyAttribute();
            attrib.TryValidate(CreateContext(), "non-empty", out string reason)
                  .Should().BeTrue();
            reason.Should().BeNull();
        }

        [TestMethod]
        public void EmptyStringCorrectlyFailsAtNotBeingEmpty()
        {
            var attrib = new MustNotBeEmptyAttribute();
            attrib.TryValidate(CreateContext(), string.Empty, out string reason)
                  .Should().BeFalse();
            reason.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void ObjectConvertibleToStringValidatesCorrectly()
        {
            var attrib = new MustNotBeEmptyAttribute();

            attrib.TryValidate(CreateContext(), new ColoredString("non-empty"), out string reason)
                  .Should().BeTrue();
            reason.Should().BeNull();

            attrib.TryValidate(CreateContext(), ColoredString.Empty, out reason)
                  .Should().BeFalse();
            reason.Should().NotBeNullOrEmpty();
        }

        private ArgumentValidationContext CreateContext() =>
            new ArgumentValidationContext(null);
    }
}
