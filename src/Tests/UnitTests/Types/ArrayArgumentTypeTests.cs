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
            badContext.Should().Throw<ArgumentNullException>();

            Action badValue = () => type.GetCompletions(c, null);
            badValue.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetCompletionsWithValidSeparators()
        {
            var type = (ArrayArgumentType)ArgumentType.GetType(typeof(bool[]));
            var c = new ArgumentCompletionContext { ParseContext = ArgumentParseContext.Default };

            type.GetCompletions(c, "Tr").Should().Equal("True");
            type.GetCompletions(c, string.Empty).Should().Equal("False", "True");
            type.GetCompletions(c, "False,f").Should().Equal("False,False");
            type.GetCompletions(c, "33,f").Should().Equal("33,False");
            type.GetCompletions(c, "True,False,").Should().Equal("True,False,False", "True,False,True");
        }

        [TestMethod]
        public void GetCompletionsWithoutValidSeparators()
        {
            var type = (ArrayArgumentType)ArgumentType.GetType(typeof(bool[]));
            var c = new ArgumentCompletionContext
            {
                ParseContext = new ArgumentParseContext
                {
                    ElementSeparators = Array.Empty<string>()
                }
            };

            type.GetCompletions(c, "Tr").Should().Equal("True");
            type.GetCompletions(c, string.Empty).Should().Equal("False", "True");
            type.GetCompletions(c, "False,f").Should().BeEmpty();
            type.GetCompletions(c, "33,f").Should().BeEmpty();
            type.GetCompletions(c, "True,False,").Should().BeEmpty();
        }
    }
}
