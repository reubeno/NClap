using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Help;
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

        private class TestClassWithField
        {
#pragma warning disable 0649 // Field is never assigned to, and will always have its default value
            public int MyValue;
#pragma warning restore 0649
        }

        private class TestClassWithEvent
        {
            public delegate void EventHandler();
            public event EventHandler MyEvent;
        }

        [TestMethod]
        public void TestThatExceptionThrownOnNullMember()
        {
            Action a = () => new ArgumentDefinition((MemberInfo)null, new NamedArgumentAttribute(), new ArgumentSetDefinition());
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatArgumentDefinitionCanBeBackedByField()
        {
            var argSet = new ArgumentSetDefinition();

            Action a = () => new ArgumentDefinition(
                typeof(TestClassWithField).GetField(nameof(TestClassWithField.MyValue)),
                new NamedArgumentAttribute(),
                argSet);

            a.Should().NotThrow();
        }

        [TestMethod]
        public void TestThatArgumentDefinitionCannotBeBackedByEvent()
        {
            var argSet = new ArgumentSetDefinition();

            Action a = () => new ArgumentDefinition(
                typeof(TestClassWithEvent).GetEvent(nameof(TestClassWithEvent.MyEvent)),
                new NamedArgumentAttribute(),
                argSet);

            a.Should().Throw<NotSupportedException>();
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

            var usage = new ArgumentUsageInfo(argDef);
            usage.GetSyntaxSummary().Should().Be("--my-value <int>");
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
