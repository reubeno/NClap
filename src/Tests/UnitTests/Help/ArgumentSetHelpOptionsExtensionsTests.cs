using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Help;

namespace NClap.Tests.Help
{
    [TestClass]
    public class ArgumentSetHelpOptionsExtensionsTests
    {
        [TestMethod]
        public void TestThatColumnWidthsThrowsWithTooManyWidths()
        {
            var options = new ArgumentSetHelpOptions()
                .With().TwoColumnLayout();

            options.Invoking(o => o.ColumnWidths(Any.Int(), Any.Int(), Any.Int()).Apply())
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestThatColumnWidthsThrowsWithWrongLayout()
        {
            var options = new ArgumentSetHelpOptions()
                .With().OneColumnLayout();

            options.Invoking(o => o.ColumnWidths(10).Apply())
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestThatColumnSeparatorThrowsWithWrongLayout()
        {
            var options = new ArgumentSetHelpOptions()
                .With().OneColumnLayout();

            options.Invoking(o => o.ColumnSeparator(" ").Apply())
                .Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
