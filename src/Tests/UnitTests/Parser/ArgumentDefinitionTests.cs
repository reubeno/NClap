using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Parser;
using System;
using System.Linq;
using System.Reflection;

namespace NClap.Tests.Parser
{
    [TestClass]
    public class ArgumentDefinitionTests
    {
        private class TestArguments
        {
            [NamedArgument(ArgumentFlags.Required)]
            public int MyValue { get; set; }
        }

        [TestMethod]
        public void TestThatExceptionThrownOnNullMember()
        {
            Action a = () => new ArgumentDefinition((MemberInfo)null, new NamedArgumentAttribute(), new ArgumentSetDefinition());
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatToStringYieldsNonEmptyResult()
        {
            var argDef = GetArgumentDefinition();
            argDef.ToString().Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void TestThatSyntaxSummaryUsesPreferenceForArgumentValue()
        {
            var argDef = GetArgumentDefinition(new ArgumentSetAttribute
            {
                Style = ArgumentSetStyle.GetOpt,
                PreferNamedArgumentValueAsSucceedingToken = true
            });

            argDef.GetSyntaxSummary().Should().Be("--my-value <int>");
        }

        [TestMethod]
        public void TestThatGetNameThrowsOnInvalidNameType()
        {
            var argDef = GetArgumentDefinition();
            argDef.Invoking(a => a.GetName((ArgumentNameType)int.MaxValue))
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        private ArgumentDefinition GetArgumentDefinition(ArgumentSetAttribute attrib = null)
        {
            var argSet = ReflectionBasedParser.CreateArgumentSet(typeof(TestArguments), attrib);
            return argSet.AllArguments.Single();
        }
    }
}
