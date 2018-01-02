using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class StringWrapperTests
    {
        [TestMethod]
        public void TestThatWrappingNullThrows()
        {
            Action a = () => new StringWrapper(null);
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatWrappedStringYieldsOriginalString()
        {
            const string anyString = "Something here";

            var wrapper = new StringWrapper(anyString);
            wrapper.Content.Should().Be(anyString);
            wrapper.ToString().Should().Be(anyString);
        }

        [TestMethod]
        public void TestThatImplicitOperatorsAreOkayWithNonNullStrings()
        {
            const string anyString = "Something here";

            StringWrapper wrapper = anyString;
            wrapper.Should().NotBeNull();

            string unwrapped = wrapper;
            unwrapped.Should().NotBeNull().And.Be(anyString);
        }

        [TestMethod]
        public void TestThatImplicitOperatorsPreserveNullness()
        {
            string nullString = null;

            StringWrapper wrapper = nullString;
            wrapper.Should().BeNull();

            string unwrapped = wrapper;
            unwrapped.Should().BeNull();
        }

        [TestMethod]
        public void TestThatTruncatingWrapperBuilderToLongerLengthThrows()
        {
            var builder = CreateStringBuilderWrapper();

            const string anyString = "Hello";
            builder.Append(anyString);

            builder.Invoking(b => b.Truncate(anyString.Length + 1))
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        private IStringBuilder CreateStringBuilderWrapper()
        {
            const string anyString = "Something here";

            var wrapper = new StringWrapper(anyString);
            return wrapper.CreateNewBuilder();
        }
    }
}
