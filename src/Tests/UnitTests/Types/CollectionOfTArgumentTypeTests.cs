using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Types;
using System;
using System.Collections.Generic;

namespace NClap.Tests.Types
{
    [TestClass]
    public class CollectionOfTArgumentTypeTests
    {
        internal class NonCollectionType
        {
        }

        internal class TypeWithoutParameterlessConstructor
        {
            public TypeWithoutParameterlessConstructor(int x)
            {
            }
        }

        [TestMethod]
        public void TestThatConstructorThrowsOnTypeWithoutParameterlessConstructor()
        {
            Action a = () => new CollectionOfTArgumentType(typeof(TypeWithoutParameterlessConstructor));
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestThatConstructorThrowsOnNonCollectionType()
        {
            Action a = () => new CollectionOfTArgumentType(typeof(NonCollectionType));
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestThatTokenWithSingleElementParses()
        {
            var type = new CollectionOfTArgumentType(typeof(List<int>));
            var context = ArgumentParseContext.Default;

            type.TryParse(context, "15", out object value).Should().BeTrue();
            value.Should().BeOfType<List<int>>().Which.Should().BeEquivalentTo(new[] { 15 });
        }

        [TestMethod]
        public void TestThatTokenWithNoSeparatorsParsesAsOneElement()
        {
            var type = new CollectionOfTArgumentType(typeof(List<string>));

            var context = ArgumentParseContext.Default;
            context.ElementSeparators = null;

            type.TryParse(context, "a,b", out object value).Should().BeTrue();
            value.Should().BeOfType<List<string>>().Which.Should().BeEquivalentTo(new[] { "a,b" });
        }

        [TestMethod]
        public void TestThatTokenWithMatchingSeparatorsParsesAsMultipleElements()
        {
            var type = new CollectionOfTArgumentType(typeof(List<string>));

            var context = ArgumentParseContext.Default;
            context.ElementSeparators = new[] { ":" };

            type.TryParse(context, "a:b", out object value).Should().BeTrue();
            value.Should().BeOfType<List<string>>().Which.Should().BeEquivalentTo(new[] { "a", "b" });
        }
    }
}
