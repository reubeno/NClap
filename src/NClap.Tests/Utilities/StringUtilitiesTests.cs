using System;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class StringUtilitiesTests
    {
        [TestMethod]
        public void AppendWrappedWithBogusArgs()
        {
            Action nullBuilder = () => StringUtilities.AppendWrapped(null, "hello", 10);
            nullBuilder.ShouldThrow<ArgumentNullException>();

            var builder = new StringBuilder();
            Action nullText = () => builder.AppendWrapped(null, 10);
            nullText.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void AppendWrappedLineThrowsOnZeroWidth()
        {
            var builder = new StringBuilder();

            Action append = () => builder.AppendWrappedLine("text", 0);
            append.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void AppendWrappedLineShortText()
        {
            var builder = new StringBuilder();
            builder.AppendWrappedLine("hello", 80);
            builder.ToString().Should().Be("hello\r\n");
        }

        [TestMethod]
        public void AppendWrappedLineNeedsToWrap()
        {
            var builder = new StringBuilder();
            builder.AppendWrappedLine("hello", 3);
            builder.ToString().Should().Be("hel\r\nlo\r\n");
        }

        [TestMethod]
        public void AppendWrappedLineIgnoresContentsOfBuilder()
        {
            var builder = new StringBuilder();
            builder.Append("PREFIX");
            builder.AppendWrappedLine("hello", 3);
            builder.ToString().Should().Be("PREFIXhel\r\nlo\r\n");
        }

        [TestMethod]
        public void AppendWrappedLineWithIndent()
        {
            var builder = new StringBuilder();
            builder.AppendWrappedLine("hello", 3, 2);
            builder.ToString().Should().Be("  h\r\n  e\r\n  l\r\n  l\r\n  o\r\n");
        }

        [TestMethod]
        public void AppendWrappedLineWithNewLineTerminatedString()
        {
            var builder = new StringBuilder();
            builder.AppendWrappedLine("hello\n", 80);
            builder.ToString().Should().Be("hello\r\n\r\n");
        }

        [TestMethod]
        public void AppendWrappedLineWithMultipleTerminatingNewLines()
        {
            var builder = new StringBuilder();
            builder.AppendWrappedLine("hello\n\n\n", 80);
            builder.ToString().Should().Be("hello\r\n\r\n\r\n\r\n");
        }

        [TestMethod]
        public void AppendWrappedLineIndentsAroundEmbeddedNewLines()
        {
            var builder = new StringBuilder();
            builder.AppendWrappedLine("hello\nworld", 80, 4);
            builder.ToString().Should().Be("    hello\r\n    world\r\n");
        }

        [TestMethod]
        public void WrapThrowsOnNullString()
        {
            const int wrapWidth = 10;

            Action wrapNullAction = () => StringUtilities.Wrap((string)null, wrapWidth);
            wrapNullAction.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void WrapThrowsOnNonPositiveWidth()
        {
            const string text = "Hello";

            Action wrapZeroAction = () => StringUtilities.Wrap(text, 0);
            wrapZeroAction.ShouldThrow<ArgumentOutOfRangeException>();

            Action wrapNegativeAction = () => StringUtilities.Wrap(text, -1);
            wrapNegativeAction.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void WrapThrowsOnWidthLessThanOrEqualToIndent()
        {
            const string text = "Hello";

            Action wrapAction = () => StringUtilities.Wrap(text, 10, 20);
            wrapAction.ShouldThrow<ArgumentOutOfRangeException>();

            wrapAction = () => StringUtilities.Wrap(text, 10, 10);
            wrapAction.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void WrapThrowsOnHangingIndentLargerThanIndent()
        {
            const string text = "Hello";

            Action wrapAction = () => StringUtilities.Wrap(text, 80, 10, 11);
            wrapAction.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void WrapRemovesCarriageReturn()
        {
            StringUtilities.Wrap("Foo\rBar", 30).Should().Be("FooBar");
        }

        [TestMethod]
        public void EmptyStringIsUnaffectedByWrap()
        {
            StringUtilities.Wrap(string.Empty, 10).Should().Be(string.Empty);
        }

        [TestMethod]
        public void ShortStringDoesNotNeedWrapping()
        {
            StringUtilities.Wrap("a b ", 10).Should().Be("a b");
        }

        [TestMethod]
        public void SimpleStringsWrapCorrectly()
        {
            StringUtilities.Wrap("123 456 789 012", 3).Should().Be(
                "123" + Environment.NewLine +
                "456" + Environment.NewLine +
                "789" + Environment.NewLine +
                "012");

            StringUtilities.Wrap("123 456 789 012", 4).Should().Be(
                "123" + Environment.NewLine +
                "456" + Environment.NewLine +
                "789" + Environment.NewLine +
                "012");
        }

        [TestMethod]
        public void WrapTextWordsWiderThanWidth()
        {
            StringUtilities.Wrap("1234", 1).Should().Be(
                "1" + Environment.NewLine +
                "2" + Environment.NewLine +
                "3" + Environment.NewLine +
                "4");

            StringUtilities.Wrap("123 456 789 012", 2).Should().Be(
                "12" + Environment.NewLine +
                "3"  + Environment.NewLine +
                "45" + Environment.NewLine +
                "6"  + Environment.NewLine +
                "78" + Environment.NewLine +
                "9"  + Environment.NewLine +
                "01" + Environment.NewLine +
                "2");

            StringUtilities.Wrap("1234 5678", 2).Should().Be(
                "12" + Environment.NewLine +
                "34" + Environment.NewLine +
                "56" + Environment.NewLine +
                "78");

            StringUtilities.Wrap("1234 5678", 3).Should().Be(
                "123" + Environment.NewLine +
                "4"   + Environment.NewLine +
                "567" + Environment.NewLine +
                "8");
        }

        [TestMethod]
        public void WrapWithEmptyLines()
        {
            StringUtilities.Wrap("1234\r\n\r\n", 10, 4).Should().Be(
                "    1234" + Environment.NewLine +
                "    "     + Environment.NewLine +
                "    ");
        }

        [TestMethod]
        public void TokenizeEmptyString()
        {
            StringUtilities.Tokenize(string.Empty).ToArray().Should().BeEmpty();
            StringUtilities.Tokenize(string.Empty, true).ToArray().Should().BeEmpty();
            StringUtilities.Tokenize(string.Empty, false).ToArray().Should().BeEmpty();
        }

        [TestMethod]
        public void TokenizeSingleToken()
        {
            var tokens = StringUtilities.Tokenize("hello").ToArray();
            tokens.Length.Should().Be(1);
            tokens[0].Contents.Length.Should().Be("hello".Length);
            tokens[0].Contents.StartingOffset.Should().Be(0);
            tokens[0].Contents.ToString().Should().Be("hello");
            tokens[0].StartsWithQuote.Should().BeFalse();
            tokens[0].EndsWithQuote.Should().BeFalse();
        }

        [TestMethod]
        public void TokenizeSingleQuotedToken()
        {
            var tokens = StringUtilities.Tokenize("\"hello world\"").ToArray();
            tokens.Length.Should().Be(1);
            tokens[0].Contents.Length.Should().Be("hello world".Length);
            tokens[0].Contents.StartingOffset.Should().Be(1);
            tokens[0].Contents.ToString().Should().Be("hello world");
            tokens[0].StartsWithQuote.Should().BeTrue();
            tokens[0].EndsWithQuote.Should().BeTrue();
        }

        [TestMethod]
        public void TokenizingEmptyToken()
        {
            var tokens = StringUtilities.Tokenize("a \"\" b").ToArray();
            tokens.Length.Should().Be(3);

            tokens[0].Contents.Length.Should().Be("a".Length);
            tokens[0].Contents.StartingOffset.Should().Be(0);
            tokens[0].Contents.ToString().Should().Be("a");
            tokens[0].StartsWithQuote.Should().BeFalse();
            tokens[0].EndsWithQuote.Should().BeFalse();

            tokens[1].Contents.Length.Should().Be(0);
            tokens[1].Contents.StartingOffset.Should().Be(3);
            tokens[1].Contents.ToString().Should().Be(string.Empty);
            tokens[1].StartsWithQuote.Should().BeTrue();
            tokens[1].EndsWithQuote.Should().BeTrue();

            tokens[2].Contents.Length.Should().Be("b".Length);
            tokens[2].Contents.StartingOffset.Should().Be(5);
            tokens[2].Contents.ToString().Should().Be("b");
            tokens[2].StartsWithQuote.Should().BeFalse();
            tokens[2].EndsWithQuote.Should().BeFalse();
        }

        [TestMethod]
        public void TokenizingIgnoresLeadingAndTrailingSpace()
        {
            var tokens = StringUtilities.Tokenize("   \" a b \"  ").ToArray();
            tokens.Length.Should().Be(1);
            tokens[0].Contents.Length.Should().Be(" a b ".Length);
            tokens[0].Contents.StartingOffset.Should().Be(4);
            tokens[0].Contents.ToString().Should().Be(" a b ");
            tokens[0].StartsWithQuote.Should().BeTrue();
            tokens[0].EndsWithQuote.Should().BeTrue();
        }

        [TestMethod]
        public void TokenizingUnterminatedQuotesStrictly()
        {
            Action tokenizeAction = () => StringUtilities.Tokenize("\"a").ToArray();
            tokenizeAction.ShouldThrow<ArgumentException>();
        }

        [TestMethod]
        public void TokenizingUnterminatedQuotesLoosely()
        {
            var tokens = StringUtilities.Tokenize("\"a", true).ToArray();
            tokens.Length.Should().Be(1);
            tokens[0].Contents.Length.Should().Be("a".Length);
            tokens[0].Contents.StartingOffset.Should().Be(1);
            tokens[0].Contents.ToString().Should().Be("a");
            tokens[0].StartsWithQuote.Should().BeTrue();
            tokens[0].EndsWithQuote.Should().BeFalse();
        }

        [TestMethod]
        public void TerminatingQuotesInMiddleOfTokenWithNoPartialInput()
        {
            Action tokenizeAction = () => StringUtilities.Tokenize("\"ab\"cd\"").ToArray();
            tokenizeAction.ShouldThrow<ArgumentException>();

            Action tokenizeAction2 = () => StringUtilities.Tokenize("\"ab\"cd").ToArray();
            tokenizeAction2.ShouldThrow<ArgumentException>();
        }

        [TestMethod]
        public void TerminatingQuotesInMiddleOfTokenWithPartialInput()
        {
            var tokens = StringUtilities.Tokenize("\"ab\"cd\"", true).ToArray();
            tokens.Length.Should().Be(1);
            tokens[0].Contents.Length.Should().Be("ab\"cd".Length);
            tokens[0].Contents.StartingOffset.Should().Be(1);
            tokens[0].Contents.ToString().Should().Be("ab\"cd");
            tokens[0].StartsWithQuote.Should().BeTrue();
            tokens[0].EndsWithQuote.Should().BeTrue();
        }

        [TestMethod]
        public void CharsAfterTerminatingQuotesWithPartialInput()
        {
            var tokens = StringUtilities.Tokenize("\"a\"b", true).ToArray();
            tokens.Length.Should().Be(1);
            tokens[0].Contents.ToString().Should().Be("a\"b");
            tokens[0].Contents.Length.Should().Be("a\"b".Length);
            tokens[0].Contents.StartingOffset.Should().Be(1);
            tokens[0].StartsWithQuote.Should().BeTrue();
            tokens[0].EndsWithQuote.Should().BeFalse();
            tokens[0].InnerLength.Should().Be("a\"b".Length);
        }

        [TestMethod]
        public void NoNeedToQuoteStringWithNoSpaces()
        {
            StringUtilities.QuoteIfNeeded("HelloWorld").Should().Be("HelloWorld");
            StringUtilities.QuoteIfNeeded("Hello\0World").Should().Be("Hello\0World");
            StringUtilities.QuoteIfNeeded("\"Hello\"").Should().Be("\"Hello\"");
            StringUtilities.QuoteIfNeeded("Hello\nWorld").Should().Be("Hello\nWorld");
            StringUtilities.QuoteIfNeeded("Hello\rWorld").Should().Be("Hello\rWorld");
            StringUtilities.QuoteIfNeeded("Hello\vWorld").Should().Be("Hello\vWorld");
        }

        [TestMethod]
        public void NeedToQuoteEmptyString()
        {
            StringUtilities.QuoteIfNeeded(string.Empty).Should().Be("\"\"");
        }

        [TestMethod]
        public void NeedToQuoteStringsWithSpaces()
        {
            StringUtilities.QuoteIfNeeded("Hello World").Should().Be("\"Hello World\"");
            StringUtilities.QuoteIfNeeded(" HelloWorld").Should().Be("\" HelloWorld\"");
            StringUtilities.QuoteIfNeeded("HelloWorld ").Should().Be("\"HelloWorld \"");
            StringUtilities.QuoteIfNeeded(" HelloWorld ").Should().Be("\" HelloWorld \"");
            StringUtilities.QuoteIfNeeded("  ").Should().Be("\"  \"");
        }

        [TestMethod]
        public void NeedToQuoteStringsWithTabs()
        {
            StringUtilities.QuoteIfNeeded("Hello\tWorld").Should().Be("\"Hello\tWorld\"");
            StringUtilities.QuoteIfNeeded("\tHelloWorld").Should().Be("\"\tHelloWorld\"");
            StringUtilities.QuoteIfNeeded("HelloWorld\t").Should().Be("\"HelloWorld\t\"");
            StringUtilities.QuoteIfNeeded("\tHelloWorld\t").Should().Be("\"\tHelloWorld\t\"");
            StringUtilities.QuoteIfNeeded("\t\t").Should().Be("\"\t\t\"");
        }

        [TestMethod]
        public void ToHyphenatedLowerCaseIsCorrect()
        {
            string.Empty.ToHyphenatedLowerCase().Should().Be(string.Empty);
            "HelloWorld".ToHyphenatedLowerCase().Should().Be("hello-world");
            "helloWorld".ToHyphenatedLowerCase().Should().Be("hello-world");
            "hello-world".ToHyphenatedLowerCase().Should().Be("hello-world");
            "hello_world".ToHyphenatedLowerCase().Should().Be("hello-world");
            "HELLO_WORLD".ToHyphenatedLowerCase().Should().Be("hello-world");
            "HElLO_WORLD".ToHyphenatedLowerCase().Should().Be("hel-lo-world");
            "Hello_World".ToHyphenatedLowerCase().Should().Be("hello-world");
            "Hello_world".ToHyphenatedLowerCase().Should().Be("hello-world");
            "hello_world".ToHyphenatedLowerCase().Should().Be("hello-world");
        }

        [TestMethod]
        public void ToSnakeCaseIsCorrect()
        {
            string.Empty.ToSnakeCase().Should().Be(string.Empty);
            "HelloWorld".ToSnakeCase().Should().Be("hello_world");
            "helloWorld".ToSnakeCase().Should().Be("hello_world");
            "hello-world".ToSnakeCase().Should().Be("hello_world");
            "hello_world".ToSnakeCase().Should().Be("hello_world");
            "HELLO_WORLD".ToSnakeCase().Should().Be("hello_world");
            "HElLO_WORLD".ToSnakeCase().Should().Be("hel_lo_world");
            "Hello_World".ToSnakeCase().Should().Be("hello_world");
            "Hello_world".ToSnakeCase().Should().Be("hello_world");
            "hello_world".ToSnakeCase().Should().Be("hello_world");
        }
    }
}
