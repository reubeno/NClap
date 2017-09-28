using System;
using System.Collections.Generic;
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

            Action badContext = () => type.GetCompletions(null, "Tr");
            badContext.ShouldThrow<ArgumentNullException>();

            Action badValue = () => type.GetCompletions(c, null);
            badValue.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void GetCompletions()
        {
            var type = (TupleArgumentType)ArgumentType.GetType(typeof(Tuple<bool, int, bool>));
            var c = new ArgumentCompletionContext { ParseContext = ArgumentParseContext.Default };

            type.GetCompletions(c, "Tr").Should().ContainInOrder("True");
            type.GetCompletions(c, string.Empty).Should().ContainInOrder("False", "True");
            type.GetCompletions(c, "False,3").Should().BeEmpty();
            type.GetCompletions(c, "False,3,").Should().ContainInOrder("False,3,False", "False,3,True");
        }
    }
}
