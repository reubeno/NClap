using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Types;

namespace NClap.Tests.Types
{
    [TestClass]
    public class ArgumentTypeBaseTests
    {
        private class TestType : ArgumentTypeBase
        {
            public TestType(Type t) : base(t)
            {
            }

            public Func<ArgumentParseContext, string, object> ParseHandler { get; set; } =
                (context, stringToParse) => throw new NotImplementedException();

            protected override object Parse(ArgumentParseContext context, string stringToParse) =>
                ParseHandler(context, stringToParse);
        }

        [TestMethod]
        public void TestThatTryParseThrowsOnNullString()
        {
            var ty = new TestType(GetType());
            ty.Invoking(t => t.TryParse(new ArgumentParseContext(), null, out object value))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatTryParseThrowsOnNullContext()
        {
            const string anyString = "Any string";

            var ty = new TestType(GetType());
            ty.Invoking(t => t.TryParse(null, anyString, out object value))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatTryParseDoesNotThrowOnWellKnownParseExceptions()
        {
            const string anyString = "Any string";

            var allowedExceptions = new Exception[]
            {
                new OverflowException(),
                new ArgumentException(),
                new FormatException()
            };

            foreach (var ex in allowedExceptions)
            {
                var ty = new TestType(GetType())
                {
                    ParseHandler = (ctx, s) => throw ex
                };

                ty.TryParse(new ArgumentParseContext(), anyString, out object value)
                    .Should().BeFalse();
                value.Should().BeNull();
            }
        }

        [TestMethod]
        public void TestThatTryParseDoesNotSwallowDisallowedParseExceptions()
        {
            const string anyString = "Any string";
            var anyDisallowedException = new NotImplementedException();

            var ty = new TestType(GetType())
            {
                ParseHandler = (ctx, s) => throw anyDisallowedException
            };

            ty.Invoking(t => t.TryParse(new ArgumentParseContext(), anyString, out object value))
                .Should().Throw<NotImplementedException>();
        }
    }
}
