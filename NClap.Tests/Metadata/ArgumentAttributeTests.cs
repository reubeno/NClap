using System;
using NClap.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using NClap.Exceptions;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class ArgumentAttributeTests
    {
        [TestMethod]
        public void ParameterlessConstructorDefaults()
        {
            var namedAttribute = new NamedArgumentAttribute();
            namedAttribute.Flags.Should().Be(ArgumentFlags.AtMostOnce);

            var positionalAttribute = new PositionalArgumentAttribute();
            positionalAttribute.Flags.Should().Be(ArgumentFlags.AtMostOnce);
        }

        [TestMethod]
        public void EmptyLongNameThrowsOnRetrieval()
        {
            var attribute = new NamedArgumentAttribute(ArgumentFlags.AtMostOnce);

            Action setEmpty = () => attribute.LongName = string.Empty;
            setEmpty.ShouldNotThrow();

            Action getEmpty = () => { var x = attribute.LongName; };
            getEmpty.ShouldThrow<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void NullLongNameIsOkay()
        {
            var attribute = new NamedArgumentAttribute(ArgumentFlags.AtMostOnce) { LongName = null };
            attribute.LongName.Should().BeNull();
        }

        [TestMethod]
        public void NonEmptyLongNameIsOkay()
        {
            var attribute = new NamedArgumentAttribute(ArgumentFlags.AtMostOnce) { LongName = "Xyzzy" };
            attribute.LongName.Should().Be("Xyzzy");
        }

        [TestMethod]
        public void NullConflictArrayThrows()
        {
            var attribute = new NamedArgumentAttribute(ArgumentFlags.AtMostOnce);

            Action setNull = () => attribute.ConflictsWith = null;
            setNull.ShouldThrow<ArgumentNullException>();
        }
    }
}
