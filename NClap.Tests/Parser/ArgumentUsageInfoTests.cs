using NClap.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace NClap.Tests.Parser
{
    [TestClass]
    public class ArgumentUsageInfoTests
    {
        [TestMethod]
        public void BasicUsage()
        {
            var help = new ArgumentUsageInfo("Syntax", "Help", false);
            help.Syntax.Should().Be("Syntax");
            help.Description.Should().Be("Help");
        }
    }
}
