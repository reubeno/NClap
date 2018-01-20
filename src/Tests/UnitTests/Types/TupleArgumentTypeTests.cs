using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Types;

namespace NClap.Tests.Types
{
    [TestClass]
    public class TupleArgumentTypeTests
    {
        [TestMethod]
        public void InvalidUseOfGetCompletions()
        {
            var type = (TupleArgumentType)ArgumentType.GetType(typeof(Tuple<bool, bool>));
            var c = new ArgumentCompletionContext { ParseContext = ArgumentParseContext.Default };

            type.Invoking(t => t.GetCompletions(null, "Tr")).Should().Throw<ArgumentNullException>();
            type.Invoking(t => t.GetCompletions(c, null)).Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetCompletions()
        {
            var type = (TupleArgumentType)ArgumentType.GetType(typeof(Tuple<bool, int, bool>));
            var c = new ArgumentCompletionContext { ParseContext = ArgumentParseContext.Default };

            type.GetCompletions(c, "Tr").Should().Equal("True");
            type.GetCompletions(c, string.Empty).Should().Equal("False", "True");
            type.GetCompletions(c, "False,3").Should().BeEmpty();
            type.GetCompletions(c, "False,3,").Should().Equal("False,3,False", "False,3,True");
        }

        [TestMethod]
        public void TestThatDependentTypesListIsCorrect()
        {
            var type = (TupleArgumentType)ArgumentType.GetType(typeof(Tuple<bool, int, bool>));
            type.DependentTypes.Should().BeEquivalentTo(
                new[] { typeof(bool), typeof(int) }.Select(ArgumentType.GetType));
        }
    }
}
