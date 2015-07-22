using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class TokenTests
    {
        [TestMethod]
        public void SimpleCreateToken()
        {
            var token = new Token(new Substring("Hello", 0, 2));
            token.Contents.ToString().Should().Be("He");
            token.StartsWithQuote.Should().BeFalse();
            token.EndsWithQuote.Should().BeFalse();
            token.ToString().Should().Be("He");
            token.InnerLength.Should().Be(2);
            token.InnerStartingOffset.Should().Be(0);
            token.InnerEndingOffset.Should().Be(2);
            token.OuterLength.Should().Be(2);
            token.OuterStartingOffset.Should().Be(0);
            token.OuterEndingOffset.Should().Be(2);
        }

        [TestMethod]
        public void TokenWithQuotes()
        {
            var token = new Token(new Substring("Hi \"a b\" there", 4, 3), true, true);
            token.Contents.ToString().Should().Be("a b");
            token.StartsWithQuote.Should().BeTrue();
            token.EndsWithQuote.Should().BeTrue();
            token.ToString().Should().Be("a b");
            token.InnerLength.Should().Be(3);
            token.InnerStartingOffset.Should().Be(4);
            token.InnerEndingOffset.Should().Be(7);
            token.OuterLength.Should().Be(5);
            token.OuterStartingOffset.Should().Be(3);
            token.OuterEndingOffset.Should().Be(8);
        }

        [TestMethod]
        public void TokensThatAreEqual()
        {
            var token = new Token(new Substring("Hello", 0, 2));
            var token2 = token;

            token.Equals(token2).Should().BeTrue();
            token.Equals((object)token2).Should().BeTrue();
            (token == token2).Should().BeTrue();
            (token != token2).Should().BeFalse();
            token.GetHashCode().Should().Be(token2.GetHashCode());
        }

        [TestMethod]
        public void TokensThatAreDifferent()
        {
            var token = new Token(new Substring(" Hello ", 1, 5));
            var token2 = new Token(new Substring("\"Hello\"", 1, 5), true, true);

            token.Equals(token2).Should().BeFalse();
            token.Equals((object)token2).Should().BeFalse();
            (token == token2).Should().BeFalse();
            (token != token2).Should().BeTrue();
            token.GetHashCode().Should().NotBe(token2.GetHashCode());
        }

        [TestMethod]
        public void ATokenIsNotAString()
        {
            var token = new Token(new Substring("Hello", 0, 2));
            token.Equals("He").Should().BeFalse();
        }
    }
}
