using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Types;

namespace NClap.Tests.Types
{
    [TestClass]
    public class KeyValuePairArgumentTypeTests
    {
        [TestMethod]
        public void InvalidUseOfGetCompletions()
        {
            var type = (KeyValuePairArgumentType)ArgumentType.GetType(typeof(KeyValuePair<bool, bool>));
            var c = new ArgumentCompletionContext { ParseContext = ArgumentParseContext.Default };

            Action badContext = () => type.GetCompletions(null, "Tr");
            badContext.ShouldThrow<ArgumentNullException>();

            Action badValue = () => type.GetCompletions(c, null);
            badValue.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void GetCompletions()
        {
            var type = (KeyValuePairArgumentType)ArgumentType.GetType(typeof(KeyValuePair<bool, bool>));
            var c = new ArgumentCompletionContext { ParseContext = ArgumentParseContext.Default };

            type.GetCompletions(c, "Tr").Should().ContainInOrder("True");
            type.GetCompletions(c, string.Empty).Should().ContainInOrder("False", "True");
            type.GetCompletions(c, "False=f").Should().ContainInOrder("False=False");
            type.GetCompletions(c, "33=f").Should().ContainInOrder("33=False");
            type.GetCompletions(c, "True=").Should().ContainInOrder("True=False", "True=True");
        }
    }
}
