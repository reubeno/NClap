using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Utilities;

namespace NClap.Tests.Metadata
{
    [TestClass]
    
    public class ArgumentValueAttributeTests
    {
        public enum SimpleEnum
        {
            [ArgumentValue(HelpText = "My value")]
            Value
        }

        [TestMethod]
        public void CompatibilityDescriptionPropertyWorks()
        {
            var value = typeof(SimpleEnum).GetTypeInfo().GetField(nameof(SimpleEnum.Value));
            var attribute = value.GetSingleAttribute<ArgumentValueAttribute>();

            attribute.Should().NotBeNull();
            attribute.Description.Should().NotBeNullOrEmpty();
            attribute.Description.Should().Be(attribute.HelpText);
        }
    }
}
