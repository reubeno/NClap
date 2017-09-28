using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace NClap.Tests
{
    [TestClass]
    public class StringsTests
    {
        [TestMethod]
        public void Instantiate()
        {
            var s = new Strings();
            s.Should().NotBeNull();
        }

        [TestMethod]
        public void Culture()
        {
            // The culture is initially null.
            Strings.Culture.Should().BeNull();

            // Should be okay to set to null too.
            Strings.Culture = null;
        }

        [TestMethod]
        public void DefaultPrompt()
        {
            Strings.DefaultPrompt.Should().NotBeNullOrWhiteSpace();
        }
    }
}
