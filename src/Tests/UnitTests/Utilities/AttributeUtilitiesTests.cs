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

        class TestClassWithAttributes
        {
            [NamedArgument(ArgumentFlags.Required, LongName = "LongerFoo")]
            [MustNotBeEmpty]
            public string Foo { get; set; }
        }

        [TestMethod]
        public void PropertyWithNoAttributesYieldsNoAttributes()
        {
            var field = this.GetType().GetTypeInfo().GetMember(nameof(PropertyWithNoAttributes))[0];
            AttributeUtilities.GetAttributes<Attribute>(field).Should().BeEmpty();
        }

        [TestMethod]
        public void MethodWithAttributeYieldsAttribute()
        {
            var method = this.GetType().GetTypeInfo().GetMember(nameof(MethodWithAttributeYieldsAttribute))[0];
            var attribs = AttributeUtilities.GetAttributes<TestMethodAttribute>(method).ToList();
            attribs.Should().HaveCount(1);
            attribs[0].Should().BeOfType<TestMethodAttribute>();
        }

        [TestMethod]
        public void CanRetrieveAttributeFromProviderThatFailsToReturnCustomAttribs()
        {
            var fooProp = typeof(TestClassWithAttributes).GetTypeInfo().GetProperty(nameof(TestClassWithAttributes.Foo));
            fooProp.Should().NotBeNull();

            var provider = Substitute.For<MemberInfo>();
            ((ICustomAttributeProvider)provider).GetCustomAttributes(Arg.Any<Type>(), Arg.Any<bool>())
                    .ReturnsForAnyArgs(call => { throw new InvalidOperationException(); });
            provider.CustomAttributes.ReturnsForAnyArgs(fooProp.CustomAttributes);

            var attribs = AttributeUtilities.GetAttributes<NamedArgumentAttribute>(provider).ToList();
            attribs.Should().HaveCount(1);
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
            a.ShouldThrow<InvalidOperationException>();
        }
    }
}