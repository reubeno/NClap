using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Utilities;
using NSubstitute;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class AttributeUtilitiesTests
    {
        public bool PropertyWithNoAttributes { get; set; }

        [ArgumentSet(
            Description = "Hello",
            Examples = new string[] { })]
        class TestClassWithAttributes
        {
            [NamedArgument(
                ArgumentFlags.Required,
                LongName = "LongerFoo",
                ConflictsWith = new[] { nameof(Bar) })]
            [MustNotBeEmpty]
            public string Foo { get; set; }

            [NamedArgument]
            public string Bar { get; set; }
        }

        [TestMethod]
        public void TestThatPropertyWithNoAttributesYieldsNoAttributes()
        {
            var field = this.GetType().GetTypeInfo().GetMember(nameof(PropertyWithNoAttributes))[0];
            AttributeUtilities.GetAttributes<Attribute>(field).Should().BeEmpty();
            AttributeUtilities.GetAttributesForReflectionOnlyType<Attribute>(field).Should().BeEmpty();
        }

        [TestMethod]
        public void TestThatPropertyWithoutMatchingAttributesYieldsNoAttributes()
        {
            var property = typeof(TestClassWithAttributes).GetTypeInfo().GetMember(nameof(TestClassWithAttributes.Foo))[0];

            AttributeUtilities.GetAttributes<ArgumentSetAttribute>(property).Should().BeEmpty();
            AttributeUtilities.GetAttributesForReflectionOnlyType<ArgumentSetAttribute>(property).Should().BeEmpty();
        }

        [TestMethod]
        public void TestThatPropertyWithAttributesYieldsAttributes()
        {
            var property = typeof(TestClassWithAttributes).GetTypeInfo().GetMember(nameof(TestClassWithAttributes.Foo))[0];

            var attribsResults = new[]
            {
                AttributeUtilities.GetAttributes<NamedArgumentAttribute>(property),
                AttributeUtilities.GetAttributesForReflectionOnlyType<NamedArgumentAttribute>(property)
            };

            foreach (var attribs in attribsResults)
            {
                attribs.Should().HaveCount(1);
                var attrib = attribs.First();
                attrib.Flags.Should().Be(ArgumentFlags.Required);
                attrib.LongName.Should().Be("LongerFoo");
                attrib.ConflictsWith.Should().Equal(new[] { "Bar" });
            }
        }

        [TestMethod]
        public void TestThatTypeWithAttributesYieldsAttributes()
        {
            var type = typeof(TestClassWithAttributes);

            var attribsResults = new[]
            {
                AttributeUtilities.GetAttributes<ArgumentSetAttribute>(type),
                AttributeUtilities.GetAttributesForReflectionOnlyType<ArgumentSetAttribute>(type)
            };

            foreach (var attribs in attribsResults)
            {
                attribs.Should().HaveCount(1);
                var attrib = attribs.First();
                attrib.Description.Should().Be("Hello");
                attrib.Examples.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void TestThatMethodWithAttributeYieldsAttribute()
        {
            var method = this.GetType().GetTypeInfo().GetMember(nameof(TestThatMethodWithAttributeYieldsAttribute))[0];

            var attribsResults = new[]
            {
                AttributeUtilities.GetAttributes<TestMethodAttribute>(method).ToList(),
                AttributeUtilities.GetAttributesForReflectionOnlyType<TestMethodAttribute>(method).ToList(),
            };

            foreach (var attribs in attribsResults)
            {
                attribs.Should().ContainSingle();
                attribs[0].Should().BeOfType<TestMethodAttribute>();
            }
        }

        [TestMethod]
        public void TestThatCanRetrieveAttributeFromProviderThatFailsToReturnCustomAttribs()
        {
            var fooProp = typeof(TestClassWithAttributes).GetTypeInfo().GetProperty(nameof(TestClassWithAttributes.Foo));
            fooProp.Should().NotBeNull();

            var provider = Substitute.For<MemberInfo>();
            ((ICustomAttributeProvider)provider).GetCustomAttributes(Arg.Any<Type>(), Arg.Any<bool>())
                    .ReturnsForAnyArgs(call => { throw new InvalidOperationException(); });
            provider.CustomAttributes.ReturnsForAnyArgs(fooProp.CustomAttributes);

            var attribs = AttributeUtilities.GetAttributes<NamedArgumentAttribute>(provider).ToList();
            attribs.Should().ContainSingle();
            attribs[0].Should().BeOfType<NamedArgumentAttribute>();
            attribs[0].LongName.Should().Be("LongerFoo");
        }

        [TestMethod]
        public void CannotRetrieveAttributeFromNonMemberProviderThatFailsToReturnCustomAttribs()
        {
            var fooProp = typeof(TestClassWithAttributes).GetTypeInfo().GetProperty(nameof(TestClassWithAttributes.Foo));
            fooProp.Should().NotBeNull();

            var provider = Substitute.For<ICustomAttributeProvider>();
            provider.GetCustomAttributes(Arg.Any<Type>(), Arg.Any<bool>())
                    .ReturnsForAnyArgs(call => { throw new InvalidOperationException(); });

            Action a = () => AttributeUtilities.GetAttributes<NamedArgumentAttribute>(provider).ToList();
            a.Should().Throw<InvalidOperationException>();

            a = () => AttributeUtilities.GetAttributesForReflectionOnlyType<NamedArgumentAttribute>(provider).ToList();
            a.Should().Throw<NotSupportedException>();
        }
    }
}