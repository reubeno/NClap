using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Help;
using NClap.Metadata;
using NClap.Parser;

namespace NClap.Tests.Help
{
    [TestClass]
    public class ArgumentSetUsageInfoTests
    {
        [TestMethod]
        public void TestThatGetLogoYieldsEmptyStringLogoCannotBeExpanded()
        {
            var attrib = new ArgumentSetAttribute
            {
                Logo = "{",
                ExpandLogo = true
            };

            var argSet = new ArgumentSetDefinition(attrib);
            var usageInfo = new ArgumentSetUsageInfo(argSet, null);

            var logo = usageInfo.Logo;
            logo.Should().BeEmpty();
        }
    }
}
