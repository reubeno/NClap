using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Exceptions;
using NClap.Metadata;
using NClap.Utilities;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class ArgumentAttributeTests
    {
        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class SimpleTestClass
        {
            [NamedArgument(Description = "My value")]
            public int Value { get; set; }
        }

        [TestMethod]
        public void ParameterlessConstructorDefaults()
        {
            var namedAttribute = new NamedArgumentAttribute();
            namedAttribute.Flags.Should().Be(ArgumentFlags.Optional);

            var positionalAttribute = new PositionalArgumentAttribute();
            positionalAttribute.Flags.Should().Be(ArgumentFlags.Optional);
        }

        [TestMethod]
        public void EmptyLongNameThrowsOnRetrieval()
        {
            var attribute = new NamedArgumentAttribute(ArgumentFlags.AtMostOnce);

            Action setEmpty = () => attribute.LongName = string.Empty;
            setEmpty.Should().NotThrow();

            Action getEmpty = () => { var x = attribute.LongName; };
            getEmpty.Should().Throw<InvalidArgumentSetException>();
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
            setNull.Should().Throw<ArgumentNullException>();
        }
    }
}
