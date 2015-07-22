using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Parser;

namespace NClap.Tests.Parser
{
    [TestClass]
    public class TokenizeTests
    {
        [TestMethod]
        public void Tokenize()
        {
            var tokens = CommandLineParser.Tokenize("a b cd e").ToArray();
            tokens.Select(token => token.ToString()).Should().ContainInOrder("a", "b", "cd", "e");
        }
    }
}
