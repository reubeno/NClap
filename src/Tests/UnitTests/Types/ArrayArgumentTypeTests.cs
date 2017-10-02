using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Types;

namespace NClap.Tests.Types
{
    [TestClass]
    public class ArrayArgumentTypeTests
    {
        [TestMethod]
        public void Elements()
        {
            var type = (ArrayArgumentType)ArgumentType.GetType(typeof(int[]));
            type.Format(new[] { 0, 1, 2 }).Should().Be("0, 1, 2");
        }

        [TestMethod]
        public void InvalidUseOfGetCompletions()
        {
            var type = (ArrayArgumentType)ArgumentType.GetType(typeof(bool[]));
            var c = new ArgumentCompletionContext { ParseContext = ArgumentParseContext.Default };

            Action badContext = () => type.GetCompletions(null, "Tr");
            badContext.ShouldThrow<ArgumentNullException>();

            Action badValue = () => type.GetCompletions(c, null);
            badValue.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void GetCompletions()
        {
            var type = (ArrayArgumentType)ArgumentType.GetType(typeof(bool[]));
            var c = new ArgumentCompletionContext { ParseContext = ArgumentParseContext.Default };

            type.GetCompletions(c, "Tr").Should().ContainInOrder("True");
            type.GetCompletions(c, string.Empty).Should().ContainInOrder("False", "True");
            type.GetCompletions(c, "False,f").Should().ContainInOrder("False,False");
            type.GetCompletions(c, "33,f").Should().ContainInOrder("33,False");
            type.GetCompletions(c, "True,False,").Should().ContainInOrder("True,False,False", "True,False,True");
        }
    }
}
