using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    internal static class ColoredMultistringBuilderTestExtensionMethods
    {
        public static void ShouldBeEmpty(this ColoredMultistringBuilder builder) =>
            builder.Length.Should().Be(0);

        public static void ShouldProduce(this ColoredMultistringBuilder builder, IEnumerable<ColoredString> pieces) =>
            builder.ToMultistring().Content.Should().Equal(pieces);

        public static void ShouldProduce(this ColoredMultistringBuilder builder, params ColoredString[] pieces) =>
            builder.ToMultistring().Content.Should().Equal(pieces);
    }

    [TestClass]
    public class ColoredMultistringBuilderTests
    {
        private static readonly Random random = new Random();

        private const string anyString = "TesT";
        private const char anyChar = 'x';

        private static readonly string[] anyArrayOfMultipleStrings = new[]
        {
            "Hello", ", world", "!"
        };

        private ColoredString[] anyArrayOfMultipleColoredStrings;

        [TestInitialize]
        public void Initialize()
        {
            anyArrayOfMultipleColoredStrings = anyArrayOfMultipleStrings.Select(
                s => CreateColoredString(s)).ToArray();
        }

        [TestMethod]
        public void TestNewlyConstructedBuilderIsEmpty()
        {
            var builder = new ColoredMultistringBuilder();
            builder.ShouldBeEmpty();
        }

        [TestMethod]
        public void TestEmptyBuilderYieldsEmptyString()
        {
            var builder = new ColoredMultistringBuilder();
            builder.ToString().Should().BeEmpty();
        }

        [TestMethod]
        public void TestEmptyBuilderYieldsEmptyMultistring()
        {
            var builder = new ColoredMultistringBuilder();
            var ms = builder.ToMultistring();
            ms.Should().NotBeNull();
            ms.IsEmpty().Should().BeTrue();
        }

        [TestMethod]
        public void TestAppendingOnePieceToEmptyBuilderYieldsBackCopyOfPiece()
        {
            var anyCs = CreateColoredString();

            var builder = new ColoredMultistringBuilder();
            builder.Append(anyCs);

            builder.Length.Should().Be(anyCs.Length);
            builder.ToString().Should().Be(anyCs.ToString());

            var ms = builder.ToMultistring();
            ms.Should().NotBeNull();
            ms.Length.Should().Be(anyCs.Length);
            ms.Content.Should().ContainSingle(anyCs);
        }

        [TestMethod]
        public void TestIterativelyAppendingMultiplePiecesWithSameColorMergesStrings()
        {
            var anyStrings = anyArrayOfMultipleStrings;
            var mergedString = string.Join(string.Empty, anyStrings);
            var anyColoredStrings = anyArrayOfMultipleColoredStrings;

            var builder = new ColoredMultistringBuilder();
            
            foreach (var cs in anyColoredStrings)
            {
                builder.Append(cs);
            }

            builder.Length.Should().Be(mergedString.Length);
            builder.ToString().Should().Be(mergedString);
            builder.ShouldProduce(anyColoredStrings);
        }

        [TestMethod]
        public void TestAppendingMultiplePiecesInOneOperationYieldsSameAsMultipleAppends()
        {
            var anyStrings = anyArrayOfMultipleStrings;
            var mergedString = string.Join(string.Empty, anyStrings);
            var anyColoredStrings = anyArrayOfMultipleColoredStrings;

            var builder = new ColoredMultistringBuilder();
            builder.Append(anyColoredStrings);

            builder.ShouldProduce(anyColoredStrings);
        }

        [TestMethod]
        public void TestAppendingMultistringAddsItsPieces()
        {
            var anyColoredStrings = anyArrayOfMultipleColoredStrings;
            var anyMultistring = new ColoredMultistring(anyColoredStrings);

            var builder = new ColoredMultistringBuilder();
            builder.Append(anyMultistring);

            builder.ShouldProduce(anyMultistring.Content);
        }

        [TestMethod]
        public void TestAppendingMultipleMultistrings()
        {
            var anyColoredStrings = anyArrayOfMultipleColoredStrings;
            var anyMultistring = new ColoredMultistring(anyColoredStrings);

            var anyOtherArrayOfMultipleColoredStrings = new[]
            {
                new ColoredString("Another", ConsoleColor.Cyan),
                new ColoredString("string", ConsoleColor.Gray)
            };

            var anyOtherMultistring = new ColoredMultistring(anyOtherArrayOfMultipleColoredStrings);

            var builder = new ColoredMultistringBuilder();
            builder.Append(new[] { anyMultistring, anyOtherMultistring });

            builder.ShouldProduce(anyColoredStrings.Concat(anyOtherMultistring.Content));
        }

        [TestMethod]
        public void TestAppendingBareStringYieldsColorlessMultistring()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyString);

            builder.ShouldProduce(new ColoredString(anyString, null, null));
        }

        [TestMethod]
        public void TestAppendingNegativeNumberOfCharsThrows()
        {
            const int anyNegativeCount = -1;

            var builder = new ColoredMultistringBuilder();

            builder.Invoking(b => b.Append(anyChar, anyNegativeCount))
                   .Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestAppendingZeroCharsHasNoEffect()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyChar, 0);

            builder.ShouldBeEmpty();
        }

        [TestMethod]
        public void TestAppendingMultipleCharsAppendsSingleColorlessString()
        {
            const int anyCharCount = 3;

            var builder = new ColoredMultistringBuilder();
            builder.Append(anyChar, anyCharCount);

            builder.ShouldProduce(new ColoredString(new string(anyChar, anyCharCount)));
        }

        [TestMethod]
        public void TestThatAppendingLineInsertsNewLine()
        {
            var builder = new ColoredMultistringBuilder();
            builder.AppendLine(Enumerable.Empty<ColoredMultistring>());

            builder.ShouldProduce(new ColoredString(Environment.NewLine));
        }

        [TestMethod]
        public void TestClearingEmptyBuilderHasNoEffect()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Clear();

            builder.ShouldBeEmpty();
        }

        [TestMethod]
        public void TestClearingNonEmptyBuilderDropsAllContent()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            builder.Clear();

            builder.ShouldBeEmpty();
        }

        [TestMethod]
        public void TestTruncationThrowsOnNegativeLength()
        {
            const int anyNegativeLength = -1;

            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            Action truncate = () => builder.Truncate(anyNegativeLength);
            truncate.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestTruncationToZeroLengthDropsContent()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            builder.Truncate(0);

            builder.ShouldBeEmpty();
        }

        [TestMethod]
        public void TestTruncationToTooLongLengthThrows()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            Action truncate = () => builder.Truncate(builder.Length + 1);
            truncate.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestTruncationTrimsContentFromLastPiece()
        {
            var anyFirstCs = CreateColoredString("ab");
            var anySecondCs = CreateColoredString("cde");

            var builder = new ColoredMultistringBuilder();
            builder.Append(new[] { anyFirstCs, anySecondCs });

            builder.Truncate(3);
            builder.ShouldProduce(
                anyFirstCs,
                new ColoredString("c", anySecondCs.ForegroundColor, anySecondCs.BackgroundColor));
        }

        [TestMethod]
        public void TestIndexOperationRetrievesCorrectCharacter()
        {
            var anyFirstString = CreateColoredString("abc");
            var anySecondString = CreateColoredString("def");

            var builder = new ColoredMultistringBuilder();
            builder.Append(new ColoredString[] { anyFirstString, anySecondString });

            for (var i = 0; i < anyFirstString.Length; ++i)
            {
                builder[i].Should().Be(anyFirstString[i]);
            }

            for (var i = 0; i < anySecondString.Length; ++i)
            {
                builder[anyFirstString.Length + i].Should().Be(anySecondString[i]);
            }
        }

        [TestMethod]
        public void TestIndexSetOperationUpdatesBuilder()
        {
            const char anyUpdatedChar = 'Y';

            var anyFirstString = CreateColoredString("abc");
            var anySecondString = CreateColoredString("defg");

            var builder = new ColoredMultistringBuilder();
            builder.Append(new ColoredString[] { anyFirstString, anySecondString });

            builder[0] = anyUpdatedChar;
            builder[6] = anyUpdatedChar;

            builder.ShouldProduce(
                new ColoredString("Ybc", anyFirstString.ForegroundColor, anyFirstString.BackgroundColor),
                new ColoredString("defY", anySecondString.ForegroundColor, anySecondString.BackgroundColor));
        }

        [TestMethod]
        public void TestIndexGetOperationThrowsOnNegativeIndex()
        {
            const int anyNegativeIndex = -1;

            var builder = new ColoredMultistringBuilder();

            Action indexOp = () => { var x = builder[anyNegativeIndex]; };
            indexOp.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestIndexSetOperationThrowsOnNegativeIndex()
        {
            const int anyNegativeIndex = -1;

            var builder = new ColoredMultistringBuilder();

            Action indexOp = () => { builder[anyNegativeIndex] = anyChar; };
            indexOp.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestIndexGetOperationThrowsOnTooLargeIndex()
        {
            var anyColoredString = CreateColoredString();

            var builder = new ColoredMultistringBuilder();
            builder.Append(anyColoredString);

            Action readOp = () => { var x = builder[anyColoredString.Length]; };
            readOp.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestIndexSetOperationThrowsOnTooLargeIndex()
        {
            var anyColoredString = CreateColoredString();

            var builder = new ColoredMultistringBuilder();
            builder.Append(anyColoredString);

            Action readOp = () => { builder[anyColoredString.Length] = anyChar; };
            readOp.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestCopyToExtractsCorrectCharacters()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            char[] buffer = new char[8];
            for (var i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = ' ';
            }

            builder.CopyTo(1, buffer, 3, 5);
            buffer.Should().Equal(new[] { ' ', ' ', ' ', 'e', 'l', 'l', 'o', ',', });
        }

        [TestMethod]
        public void TestCopyToThrowsOnNegativeIndicesOrCounts()
        {
            const int anyNegativeIndex = -1;

            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            char[] buffer = new char[builder.Length];

            builder.Invoking(b => b.CopyTo(anyNegativeIndex, buffer, 0, builder.Length))
                   .Should().Throw<ArgumentOutOfRangeException>();

            builder.Invoking(b => b.CopyTo(0, buffer, anyNegativeIndex, builder.Length))
                   .Should().Throw<ArgumentOutOfRangeException>();

            builder.Invoking(b => b.CopyTo(0, buffer, 0, anyNegativeIndex))
                   .Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestCopyToThrowsWhenOutputBufferTooSmall()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            char[] buffer = new char[1];

            builder.Invoking(b => b.CopyTo(0, buffer, 0, builder.Length))
                   .Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestInsertingAtANegativeIndexThrows()
        {
            var builder = new ColoredMultistringBuilder();

            builder.Invoking(b => b.Insert(Any.NegativeInt(), 'x'))
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestInsertingAfterEndOfStringThrows()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            builder.Invoking(b => b.Insert(builder.Length + 1, anyChar))
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestCharInsertionAtStartOfBuilder()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            builder.Insert(0, anyChar);

            builder.ShouldProduce(
                new ColoredString[] { new string(anyChar, 1) }.Concat(anyArrayOfMultipleColoredStrings));
        }


        [TestMethod]
        public void TestCharInsertionInMiddleOfPieceWithDifferentColor()
        {
            var cs = CreateColoredString("abc");

            var builder = new ColoredMultistringBuilder();
            builder.Append(cs);

            builder.Insert(1, 'x');

            builder.ShouldProduce(
                new ColoredString("a", cs.ForegroundColor, cs.BackgroundColor),
                new ColoredString("x"),
                new ColoredString("bc", cs.ForegroundColor, cs.BackgroundColor));
        }

        [TestMethod]
        public void TestCharInsertionInMiddleOfPieceWithSameColor()
        {
            var cs = new ColoredString("abc");

            var builder = new ColoredMultistringBuilder();
            builder.Append(cs);

            builder.Insert(1, 'x');

            builder.ShouldProduce(new ColoredString("axbc"));
        }

        [TestMethod]
        public void TestCharInsertionAtEndOfBuilder()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            builder.Insert(builder.Length, anyChar);

            builder.ShouldProduce(
                anyArrayOfMultipleColoredStrings.Concat(new ColoredString[] { new string(anyChar, 1) }));
        }

        [TestMethod]
        public void TestStringInsertionAtStartOfBuilder()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            builder.Insert(0, anyString);

            builder.ShouldProduce(
                new ColoredString[] { anyString }.Concat(anyArrayOfMultipleColoredStrings));
        }


        [TestMethod]
        public void TestStringInsertionInMiddleOfPieceWithDifferentColor()
        {
            var cs = CreateColoredString("abc");

            var builder = new ColoredMultistringBuilder();
            builder.Append(cs);

            builder.Insert(1, "de");

            builder.ShouldProduce(
                cs.Transform(_ => "a"),
                "de",
                cs.Transform(_ => "bc"));
        }

        [TestMethod]
        public void TestStringInsertionInMiddleOfPieceWithSameColor()
        {
            var cs = new ColoredString("abc");

            var builder = new ColoredMultistringBuilder();
            builder.Append(cs);

            builder.Insert(1, "de");

            builder.ShouldProduce("adebc");
        }

        [TestMethod]
        public void TestStringInsertionAtEndOfBuilder()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            builder.Insert(builder.Length, anyString);

            builder.ShouldProduce(
                anyArrayOfMultipleColoredStrings.Concat(new ColoredString[] { anyString }));
        }

        [TestMethod]
        public void TestRemoveThrowsOnNegativeIndexOrCount()
        {
            const int anyNegativeIndex = -1;
            const int anyCount = 1;
            const int anyValidStartIndex = 0;

            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            builder.Invoking(b => b.Remove(anyNegativeIndex, anyCount))
                   .Should().Throw<ArgumentOutOfRangeException>();

            builder.Invoking(b => b.Remove(anyValidStartIndex, anyNegativeIndex))
                   .Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestRemoveThrowsOnTooLargeIndex()
        {
            const int anyValidCount = 1;

            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            builder.Invoking(b => b.Remove(builder.Length, anyValidCount))
                   .Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestRemoveThrowsOnTooLargeCount()
        {
            const int anyValidStartIndex = 0;

            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            builder.Invoking(b => b.Remove(anyValidStartIndex, builder.Length + 1))
                   .Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TestRemovingAllCharsLeavesEmptyBuilder()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            builder.Remove(0, builder.Length);
            builder.ShouldBeEmpty();
        }

        [TestMethod]
        public void TestRemovingAcrossPieces()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            builder.Remove(1, builder.Length - 2);
            builder.ShouldProduce(
                anyArrayOfMultipleColoredStrings[0].Substring(0, 1),
                anyArrayOfMultipleColoredStrings.Last().Substring(anyArrayOfMultipleColoredStrings.Last().Length - 1));
        }

        [TestMethod]
        public void TestRemovingZeroLengthPreservesBuilderContent()
        {
            var builder = new ColoredMultistringBuilder();
            builder.Append(anyArrayOfMultipleColoredStrings);

            builder.Remove(0, 0);

            builder.ShouldProduce(anyArrayOfMultipleColoredStrings);
        }

        [TestMethod]
        public void TestRemovingMiddleOfPiecePreservesSurroundingText()
        {
            var builder = new ColoredMultistringBuilder();

            const string anyString = "SomeStringWithLength";
            var piece = new ColoredString(anyString, Any.Enum<ConsoleColor>());

            const int removeIndex = 2;
            const int removeLength = 3;

            builder.Append(piece);
            builder.Remove(removeIndex, removeLength);

            builder.ShouldProduce(new ColoredString(
                anyString.Remove(removeIndex, removeLength),
                piece.ForegroundColor, piece.BackgroundColor));
        }

        private static ColoredString CreateColoredString(string strContent = null)
        {
            ConsoleColor? anyFgColor = AnyConsoleColor();
            ConsoleColor? anyBgColor = AnyConsoleColor();

            return new ColoredString(
                strContent ?? anyString,
                anyFgColor,
                anyBgColor);
        }

        private static ConsoleColor AnyConsoleColor() => AnyOfEnum<ConsoleColor>();

        private static T AnyOfEnum<T>() where T : struct
        {
            var values = typeof(T).GetTypeInfo().GetEnumValues();
            var index = AnyNonNegativeIntegerLessThan(values.Length);
            return (T)values.GetValue(index);
        }

        private static int AnyNonNegativeIntegerLessThan(int exclusiveUpperBound) =>
            random.Next(exclusiveUpperBound);
    }
}
