using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.ConsoleInput;

namespace NClap.Tests.ConsoleInput
{

    [TestClass]
    public class TokenCompletionSetTests
    {
        [TestMethod]
        public void SimpleStringWithCompletions()
        {
            const string text = "a b";

            var completions = new[] { "abc", "aZx" };
            ITokenCompleter tokenCompleter = new TestTokenCompleter((tokens, index) => completions);

            var set = TokenCompletionSet.Create(text, 0, tokenCompleter);

            set.InputText.Should().Be(text);
            set.Completions.Should().HaveCount(2);
            set.Count.Should().Be(2);
            set.Empty.Should().BeFalse();
            set[0].Should().Be("abc");
            set[1].Should().Be("aZx");
        }

        [TestMethod]
        public void NullCompleter()
        {
            Action a = () => TokenCompletionSet.Create("hello", 0, null);
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void UntokenizableString()
        {
            const string text = "\"s\"x\"";

            ITokenCompleter tokenCompleter = new TestTokenCompleter((tokens, index) => Enumerable.Empty<string>());
            var set = TokenCompletionSet.Create(text, 0, tokenCompleter);

            set.InputText.Should().Be(text);
            set.Completions.Should().BeEmpty();
            set.Count.Should().Be(0);
            set.Empty.Should().BeTrue();
        }

        [TestMethod]
        public void EmptyString()
        {
            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create(string.Empty, 0, GetHandler(completionCalls));

            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().BeEmpty();
            completionCalls[0].TokenIndex.Should().Be(0);
        }

        [TestMethod]
        public void SpaceOnlyString()
        {
            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create("  ", 0, GetHandler(completionCalls));

            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().BeEmpty();
            completionCalls[0].TokenIndex.Should().Be(0);
        }

        [TestMethod]
        public void AtStartOfFirstToken()
        {
            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create("a b", 0, GetHandler(completionCalls));

            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().Equal("a", "b");
            completionCalls[0].TokenIndex.Should().Be(0);
        }

        [TestMethod]
        public void AtEndOfFirstToken()
        {
            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create("a b", 1, GetHandler(completionCalls));

            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().Equal("a", "b");
            completionCalls[0].TokenIndex.Should().Be(0);
        }

        [TestMethod]
        public void AtStartOfSecondToken()
        {
            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create("a b c", 2, GetHandler(completionCalls));

            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().Equal("a", "b", "c");
            completionCalls[0].TokenIndex.Should().Be(1);
        }

        [TestMethod]
        public void AtEndOfSecondToken()
        {
            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create("a b c", 3, GetHandler(completionCalls));

            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().Equal("a", "b", "c");
            completionCalls[0].TokenIndex.Should().Be(1);
        }

        [TestMethod]
        public void InLeadingSpace()
        {
            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create("   a b c", 0, GetHandler(completionCalls));

            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().Equal(string.Empty, "a", "b", "c");
            completionCalls[0].TokenIndex.Should().Be(0);
        }

        [TestMethod]
        public void InTrailingSpace()
        {
            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create("a b c   ", 6, GetHandler(completionCalls));

            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().Equal("a", "b", "c");
            completionCalls[0].TokenIndex.Should().Be(3);
        }

        [TestMethod]
        public void InInterstitialSpace()
        {
            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create("a   b   c", 6, GetHandler(completionCalls));

            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().Equal("a", "b", string.Empty, "c");
            completionCalls[0].TokenIndex.Should().Be(2);
        }

        [TestMethod]
        public void AtEndOfString()
        {
            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create("a b c", 5, GetHandler(completionCalls));

            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().Equal("a", "b", "c");
            completionCalls[0].TokenIndex.Should().Be(2);
        }

        [TestMethod]
        public void AtEndOfUnclosedQuotedToken()
        {
            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create("a \"b ", 5, GetHandler(completionCalls));

            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().Equal("a", "b ");
            completionCalls[0].TokenIndex.Should().Be(1);
        }

        [TestMethod]
        public void InMiddleOfClosedQuotedToken()
        {
            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create("a \"bc \"", 4, GetHandler(completionCalls));
            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().Equal("a", "bc ");
            completionCalls[0].TokenIndex.Should().Be(1);
        }

        [TestMethod]
        public void JustBeforeEndOfClosedQuotedToken()
        {
            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create("a \"b \"", 4, GetHandler(completionCalls));
            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().Equal("a", "b ");
            completionCalls[0].TokenIndex.Should().Be(1);
        }

        [TestMethod]
        public void AtEndOfClosedQuotedToken()
        {
            var text = "here " + '"' + @"c:\program files" + '"';

            var completionCalls = new List<TokenCompletionCall>();
            var set = TokenCompletionSet.Create(text, text.Length, GetHandler(completionCalls));
            completionCalls.Should().ContainSingle();
            completionCalls[0].Tokens.Should().Equal("here", @"c:\program files");
            completionCalls[0].TokenIndex.Should().Be(1);
        }

        private class TokenCompletionCall
        {
            public IEnumerable<string> Tokens { get; set; }

            public int TokenIndex { get; set; }
        }

        private static ITokenCompleter GetHandler(ICollection<TokenCompletionCall> calls)
        {
            return new TestTokenCompleter((tokens, index) =>
            {
                calls.Add(new TokenCompletionCall { Tokens = tokens, TokenIndex = index });
                return Enumerable.Empty<string>();
            });
        }
    }
}
