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
        public class SimpleTestClass
        {
            [NamedArgument(HelpText = "My value")]
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

        [TestMethod]
        public void CompatibilityDescriptionPropertyWorks()
        {
            var property = typeof(SimpleTestClass).GetTypeInfo().GetProperty(nameof(SimpleTestClass.Value));
            var attribute = property.GetSingleAttribute<NamedArgumentAttribute>();

            attribute.Should().NotBeNull();
            attribute.Description.Should().NotBeNullOrEmpty();
            attribute.Description.Should().Be(attribute.HelpText);
        }
    }
}
