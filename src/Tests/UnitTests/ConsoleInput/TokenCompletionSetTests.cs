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
            ConsoleCompletionHandler completionHandler = (tokens, index) => completions;

            var set = TokenCompletionSet.Create(text, 0, completionHandler);

            set.InputText.Should().Be(text);
            set.Completions.Should().HaveCount(2);
            set.Count.Should().Be(2);
            set.Empty.Should().BeFalse();
            set[0].Should().Be("abc");
            set[1].Should().Be("aZx");
        }

        [TestMethod]
        public void UntokenizableString()
        {
            const string text = "\"s\"x\"";

            ConsoleCompletionHandler completionHandler = (tokens, index) => Enumerable.Empty<string>();
            var set = TokenCompletionSet.Create(text, 0, completionHandler);

            set.InputText.Should().Be(text);
            set.Completions.Should().HaveCount(0);
            set.Count.Should().Be(0);
            set.Empty.Should().BeTrue();
        }

        [TestMethod]
        public void EmptyString()
        {
            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create(string.Empty, 0, GetHandler(completionCalls));

            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().BeEmpty();
            completionCalls[0].Item2.Should().Be(0);
        }

        [TestMethod]
        public void SpaceOnlyString()
        {
            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create("  ", 0, GetHandler(completionCalls));

            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().BeEmpty();
            completionCalls[0].Item2.Should().Be(0);
        }

        [TestMethod]
        public void AtStartOfFirstToken()
        {
            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create("a b", 0, GetHandler(completionCalls));

            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().ContainInOrder("a", "b");
            completionCalls[0].Item2.Should().Be(0);
        }

        [TestMethod]
        public void AtEndOfFirstToken()
        {
            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create("a b", 1, GetHandler(completionCalls));

            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().ContainInOrder("a", "b");
            completionCalls[0].Item2.Should().Be(0);
        }

        [TestMethod]
        public void AtStartOfSecondToken()
        {
            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create("a b c", 2, GetHandler(completionCalls));

            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().ContainInOrder("a", "b");
            completionCalls[0].Item2.Should().Be(1);
        }

        [TestMethod]
        public void AtEndOfSecondToken()
        {
            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create("a b c", 3, GetHandler(completionCalls));

            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().ContainInOrder("a", "b");
            completionCalls[0].Item2.Should().Be(1);
        }

        [TestMethod]
        public void InLeadingSpace()
        {
            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create("   a b c", 0, GetHandler(completionCalls));

            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().ContainInOrder(string.Empty, "a", "b", "c");
            completionCalls[0].Item2.Should().Be(0);
        }

        [TestMethod]
        public void InTrailingSpace()
        {
            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create("a b c   ", 6, GetHandler(completionCalls));

            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().ContainInOrder("a", "b", "c");
            completionCalls[0].Item2.Should().Be(3);
        }

        [TestMethod]
        public void InInterstitialSpace()
        {
            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create("a   b   c", 6, GetHandler(completionCalls));

            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().ContainInOrder("a", "b", string.Empty, "c");
            completionCalls[0].Item2.Should().Be(2);
        }

        [TestMethod]
        public void AtEndOfString()
        {
            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create("a b c", 5, GetHandler(completionCalls));

            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().ContainInOrder("a", "b");
            completionCalls[0].Item2.Should().Be(2);
        }

        [TestMethod]
        public void AtEndOfUnclosedQuotedToken()
        {
            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create("a \"b ", 5, GetHandler(completionCalls));

            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().ContainInOrder("a", "b ");
            completionCalls[0].Item2.Should().Be(1);
        }

        [TestMethod]
        public void InMiddleOfClosedQuotedToken()
        {
            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create("a \"bc \"", 4, GetHandler(completionCalls));
            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().ContainInOrder("a", "bc ");
            completionCalls[0].Item2.Should().Be(1);
        }

        [TestMethod]
        public void JustBeforeEndOfClosedQuotedToken()
        {
            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create("a \"b \"", 4, GetHandler(completionCalls));
            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().ContainInOrder("a", "b ");
            completionCalls[0].Item2.Should().Be(1);
        }

        [TestMethod]
        public void AtEndOfClosedQuotedToken()
        {
            var text = "here " + '"' + @"c:\program files" + '"';

            var completionCalls = new List<Tuple<string[], int>>();
            var set = TokenCompletionSet.Create(text, text.Length, GetHandler(completionCalls));
            completionCalls.Should().HaveCount(1);
            completionCalls[0].Item1.Should().ContainInOrder("here", @"c:\program files");
            completionCalls[0].Item2.Should().Be(1);
        }

        private static ConsoleCompletionHandler GetHandler(ICollection<Tuple<string[], int>> callList)
        {
            return (tokens, index) =>
            {
                callList.Add(Tuple.Create(tokens.ToArray(), index));
                return Enumerable.Empty<string>();
            };
        }
    }
}
