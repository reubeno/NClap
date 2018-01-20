using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Types;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class ArgumentTypeAttributeTests
    {
        private const string CustomDisplayName = "My custom display name";

        [ArgumentType(DisplayName = CustomDisplayName)]
        private enum MyCustomType
        {
            SomeValue
        }

        [TestMethod]
        public void TestThatCustomDisplayNameIsObserved()
        {
            var argType = ArgumentType.GetType(typeof(MyCustomType));
            argType.DisplayName.Should().Be(CustomDisplayName);
        }
    }
}
