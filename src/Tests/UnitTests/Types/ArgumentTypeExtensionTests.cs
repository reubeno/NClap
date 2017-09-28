using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NClap.Types;

namespace NClap.Tests.Types
{
    [TestClass]
    public class ArgumentTypeExtensionTests
    {
        [TestMethod]
        public void NonOverriddenExtensionConstructedFromType()
        {
            var argType = new ArgumentTypeExtension(typeof(int));
            argType.InnerType.Type.Should().Be(typeof(int));
            argType.DisplayName.Should().Be(argType.InnerType.DisplayName);
            argType.Type.Should().Be(argType.InnerType.Type);
            argType.SyntaxSummary.Should().Be(argType.InnerType.SyntaxSummary);
        }

        [TestMethod]
        public void NonOverriddenExtensionConstructedFromArgType()
        {
            var argType = new ArgumentTypeExtension(ArgumentType.Int);
            argType.InnerType.Should().Be(ArgumentType.Int);
            argType.DisplayName.Should().Be(argType.InnerType.DisplayName);
            argType.Type.Should().Be(argType.InnerType.Type);
            argType.SyntaxSummary.Should().Be(argType.InnerType.SyntaxSummary);
        }
    }
}
