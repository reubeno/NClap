using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Parser;
using System.Linq;
using System.Reflection;

namespace NClap.Tests.Parser
{
    [TestClass]
    public class ArgumentUsageInfoTests
    {
        private const string anySyntax = "Syntax";
        private const string anyDescription = "Description";

        private class TestArguments<T>
        {
            public T Value { get; set; }
        }

        [TestMethod]
        public void BasicUsage()
        {
            var help = new ArgumentUsageInfo(anySyntax, anyDescription, false);
            help.Syntax.Should().Be(anySyntax);
            help.Description.Should().Be(anyDescription);
        }

        [TestMethod]
        public void TestThatDefaultValueReturnsFalseForRequiredArgs()
        {
            var arg = CreateArg<int>(required: true, defaultValue: Any.PositiveInt());
            ArgumentUsageInfo.TryGetDefaultValue(arg, false, out object value).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatNoDefaultReturnedWhenNoSpecificDefaultAndOnlyExplicitDefaultsWanted()
        {
            var arg = CreateArg<int>();
            ArgumentUsageInfo.TryGetDefaultValue(arg, true, out object value).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatDefaultReturnedWhenNoSpecificDefaultButImplicitDefaultsWanted()
        {
            var arg = CreateArg<int>();
            ArgumentUsageInfo.TryGetDefaultValue(arg, false, out object value).Should().BeTrue();
            value.Should().Be(default(int));
        }

        [TestMethod]
        public void TestThatDefaultNeverReturnedWhenItIsNull()
        {
            var arg = CreateArg<string>();
            ArgumentUsageInfo.TryGetDefaultValue(arg, false, out object value).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatDefaultNeverReturnedWhenItIsFalse()
        {
            var arg = CreateArg<bool>();
            ArgumentUsageInfo.TryGetDefaultValue(arg, false, out object value).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatDefaultNeverReturnedWhenItIsEmptyString()
        {
            var arg = CreateArg<string>(defaultValue: string.Empty);
            ArgumentUsageInfo.TryGetDefaultValue(arg, false, out object value).Should().BeFalse();
        }
        
        [TestMethod]
        public void TestThatNonEdgeCaseDefaultIsReturned()
        {
            const string anyString = "Some default string";
            var arg = CreateArg<string>(defaultValue: anyString);

            ArgumentUsageInfo.TryGetDefaultValue(arg, false, out object value).Should().BeTrue();
            value.Should().Be(anyString);
        }

        private ArgumentDefinition CreateArg<T>(bool required = false, object defaultValue = null)
        {
            var argSet = new ArgumentSetDefinition();
            var attrib = new NamedArgumentAttribute(required ? ArgumentFlags.Required : ArgumentFlags.Optional);
            if (defaultValue != null)
            {
                attrib.DefaultValue = defaultValue;
            }

            return new ArgumentDefinition(
                typeof(TestArguments<T>).GetTypeInfo().GetMember(nameof(TestArguments<T>.Value)).Single(),
                attrib,
                argSet);
        }
    }
}
