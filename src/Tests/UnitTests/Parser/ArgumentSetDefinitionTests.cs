using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Exceptions;
using NClap.Metadata;
using NClap.Parser;
using System;
using System.Collections.Generic;

namespace NClap.Tests.Parser
{
    [TestClass]
    public class ArgumentSetDefinitionTests
    {
        private class TestArgumentSet
        {
            public int SomeProperty { get; set; }
        }

        [TestMethod]
        public void TestThatExceptionThrownWhenAddingShortNameThatIsTooLong()
        {
            const string anyNameLongerThanOneChar = "SomeString";

            var set = DefineArgSet(setAttrib: new ArgumentSetAttribute
            {
                NamedArgumentPrefixes = new[] { "--" },
                AllowMultipleShortNamesInOneToken = true
            });

            var arg = DefineArg(set, new NamedArgumentAttribute { ShortName = anyNameLongerThanOneChar });

            set.Invoking(s => s.Add(arg)).Should().Throw<InvalidArgumentSetException>();
        }

        [TestMethod]
        public void TestThatArgumentNamesAreRetrievableFromSet()
        {
            var set = DefineArgSet(new ArgumentBaseAttribute[]
                {
                    new NamedArgumentAttribute { ShortName = "short", LongName = "foo" },
                    new NamedArgumentAttribute { ShortName = "S", LongName = "LONG" }
                });

            set.GetArgumentNames(ArgumentNameType.ShortName).Should().BeEquivalentTo("short", "S");
            set.GetArgumentNames(ArgumentNameType.LongName).Should().BeEquivalentTo("foo", "LONG");
            set.Invoking(s => s.GetArgumentNames((ArgumentNameType)0x1000))
                .Should().Throw<ArgumentOutOfRangeException>();

            set.GetAllArgumentNames().Should().BeEquivalentTo("short", "S", "foo", "LONG");
        }

        private ArgumentDefinition DefineArg(ArgumentSetDefinition set, ArgumentBaseAttribute argAttrib) =>
            new ArgumentDefinition(
                typeof(TestArgumentSet).GetProperty(nameof(TestArgumentSet.SomeProperty)),
                argAttrib,
                set);

        private ArgumentSetDefinition DefineArgSet(IEnumerable<ArgumentBaseAttribute> argAttribs = null, ArgumentSetAttribute setAttrib = null)
        {
            var set = new ArgumentSetDefinition(setAttrib);
            if (argAttribs != null)
            {
                foreach (var argAttrib in argAttribs)
                {
                    var arg = DefineArg(set, argAttrib);
                    set.Add(arg);
                }
            }

            return set;
        }
    }
}
