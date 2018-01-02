using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Types;

namespace NClap.Tests.Types
{
    [TestClass]
    public class ArgumentTypeTests
    {
        [TestMethod]
        public void Aliases()
        {
            ArgumentType.Int64.Should().BeSameAs(ArgumentType.Long);
            ArgumentType.UInt64.Should().BeSameAs(ArgumentType.ULong);
            ArgumentType.Int32.Should().BeSameAs(ArgumentType.Int);
            ArgumentType.UInt32.Should().BeSameAs(ArgumentType.UInt);
            ArgumentType.Int16.Should().BeSameAs(ArgumentType.Short);
            ArgumentType.UInt16.Should().BeSameAs(ArgumentType.UShort);
            ArgumentType.Int8.Should().BeSameAs(ArgumentType.SByte);
            ArgumentType.UInt8.Should().BeSameAs(ArgumentType.Byte);
            ArgumentType.Boolean.Should().BeSameAs(ArgumentType.Bool);
            ArgumentType.Single.Should().BeSameAs(ArgumentType.Float);
        }

        [TestMethod]
        public void InvalidIntegerArgumentTypeUsage()
        {
            var actions = new Action[]
            {
                () => new IntegerArgumentType(typeof(object), (str, styles) => 0, isSigned: true)
            };

            foreach (var action in actions)
            {
                action.Should().Throw<ArgumentException>();
            }
        }

        [TestMethod]
        public void InvalidEnumArgumentTypeUsage()
        {
            var actions = new Action[]
            {
                () => EnumArgumentType.Create(typeof(object))
            };

            foreach (var action in actions)
            {
                action.Should().Throw<ArgumentException>();
            }
        }

        [TestMethod]
        public void InvalidCollectionOfTArgumentTypeUsage()
        {
            var actions = new Action[]
            {
                () => new CollectionOfTArgumentType(typeof(int)),
                () => new CollectionOfTArgumentType(typeof(int[])),
                () => new CollectionOfTArgumentType(typeof(List<object>))
            };

            foreach (var action in actions)
            {
                action.Should().Throw<Exception>();
            }
        }

        [TestMethod]
        public void InvalidArrayArgumentTypeUsage()
        {
            var actions = new Action[]
            {
                () => new ArrayArgumentType(typeof(int))
            };

            foreach (var action in actions)
            {
                action.Should().Throw<ArgumentOutOfRangeException>();
            }
        }

        [TestMethod]
        public void InvalidKeyValuePairArgumentTypeUsage()
        {
            var actions = new Action[]
            {
                () => new KeyValuePairArgumentType(typeof(object)),
                () => new KeyValuePairArgumentType(typeof(List<object>)),
                () => new KeyValuePairArgumentType(typeof(KeyValuePair<object, object>))
            };

            foreach (var action in actions)
            {
                action.Should().Throw<Exception>();
            }
        }

        [TestMethod]
        public void InvalidTupleArgumentTypeUsage()
        {
            var actions = new Action[]
            {
                () => new TupleArgumentType(typeof(object)),
                () => new TupleArgumentType(typeof(List<object>)),
                () => new TupleArgumentType(typeof(Tuple<object>)),
                () => new TupleArgumentType(typeof(List<string>))
            };

            foreach (var action in actions)
            {
                action.Should().Throw<Exception>();
            }
        }

        [TestMethod]
        public void FormatSimpleType()
        {
            Action formatNull = () => ArgumentType.Int.Format(null);
            formatNull.Should().Throw<ArgumentNullException>();

            Action formatNonInt = () => ArgumentType.Int.Format("a");
            formatNonInt.Should().Throw<ArgumentOutOfRangeException>();

            ArgumentType.Int.Format(-1).Should().Be("-1");
            ArgumentType.Int.Format(7).Should().Be("7");
        }

        [TestMethod]
        public void FormatCollection()
        {
            var listOfIntArgType = new CollectionOfTArgumentType(typeof(List<int>));

            var list = new[] { 0, 10, 3, -1 }.ToList();
            listOfIntArgType.Format(list).Should().Be("0, 10, 3, -1");
        }

        [TestMethod]
        public void TryParseCollection()
        {
            var listOfIntArgType = new CollectionOfTArgumentType(typeof(List<int>));

            listOfIntArgType.TryParse(ArgumentParseContext.Default, "1, 2", out object value).Should().BeTrue();
            value.Should().BeOfType<List<int>>();

            var list = (List<int>)value;
            list.Should().HaveCount(2);
            list.Should().Equal(1, 2);
        }

        [TestMethod]
        public void TryParseCollectionWithBadSyntax()
        {
            var listOfIntArgType = new CollectionOfTArgumentType(typeof(List<int>));

            listOfIntArgType.TryParse(ArgumentParseContext.Default, "1, z", out object value).Should().BeFalse();
            value.Should().BeNull();
        }

        [TestMethod]
        public void ArrayToCollection()
        {
            var intArrayArgType = new ArrayArgumentType(typeof(int[]));

            Action invocation = () => intArrayArgType.ToCollection(null);
            invocation.Should().Throw<ArgumentNullException>();

            var outCollection = intArrayArgType.ToCollection(new ArrayList(new[] { 10, -1 }));
            outCollection.Should().BeOfType<int[]>();

            var outList = (int[])outCollection;
            outList.Length.Should().Be(2);
            outList[0].Should().Be(10);
            outList[1].Should().Be(-1);
        }

        [TestMethod]
        public void CollectionToCollection()
        {
            var listOfIntArgType = new CollectionOfTArgumentType(typeof(List<int>));

            Action invocation = () => listOfIntArgType.ToCollection(null);
            invocation.Should().Throw<ArgumentNullException>();

            var outCollection = listOfIntArgType.ToCollection(new ArrayList(new[] { 10, -1 }));
            outCollection.Should().BeOfType<List<int>>();

            var outList = (List<int>)outCollection;
            outList.Should().HaveCount(2);
            outList.Should().Equal(10, -1);
        }

        [TestMethod]
        public void ParsingNonBinaryUnits()
        {
            var defaultContext = ArgumentParseContext.Default;
            var context = new ArgumentParseContext { NumberOptions = NumberOptions.AllowMetricUnitSuffix };


            ArgumentType.Int.TryParse(defaultContext, "3k", out object value).Should().BeFalse();
            ArgumentType.Int.TryParse(context, "3kB", out value).Should().BeFalse();
            ArgumentType.Int.TryParse(context, "3MB", out value).Should().BeFalse();
            ArgumentType.Int.TryParse(context, "3T", out value).Should().BeFalse();
            ArgumentType.Int.TryParse(context, "3DA", out value).Should().BeFalse();
            ArgumentType.Int.TryParse(context, "3H", out value).Should().BeFalse();

            ArgumentType.Int.TryParse(context, "3", out value).Should().BeTrue();
            value.Should().Be(3);

            ArgumentType.Int.TryParse(context, "3da", out value).Should().BeTrue();
            value.Should().Be(30);

            ArgumentType.Int.TryParse(context, "3h", out value).Should().BeTrue();
            value.Should().Be(300);

            ArgumentType.Int.TryParse(context, "3k", out value).Should().BeTrue();
            value.Should().Be(3000);

            ArgumentType.Int.TryParse(context, "3M", out value).Should().BeTrue();
            value.Should().Be(3000000);
        }

        [TestMethod]
        public void ParsingBinaryUnits()
        {
            var defaultContext = ArgumentParseContext.Default;
            var context = new ArgumentParseContext { NumberOptions = NumberOptions.AllowBinaryMetricUnitSuffix };


            ArgumentType.Int.TryParse(defaultContext, "3k", out object value).Should().BeFalse();
            ArgumentType.Int.TryParse(context, "3b", out value).Should().BeFalse();
            ArgumentType.Int.TryParse(context, "3ib", out value).Should().BeFalse();
            ArgumentType.Int.TryParse(context, "3B", out value).Should().BeFalse();
            ArgumentType.Int.TryParse(context, "k", out value).Should().BeFalse();
            ArgumentType.Int.TryParse(context, "M", out value).Should().BeFalse();

            ArgumentType.Int.TryParse(context, "3", out value).Should().BeTrue();
            value.Should().Be(3);

            foreach (var s in new[] { "3k", "3KB", "3KiB", "3kB" })
            {
                ArgumentType.Int.TryParse(context, s, out value).Should().BeTrue();
                value.Should().Be(3 * 1024);
            }

            foreach (var s in new[] { "3M", "3MB", "3MiB" })
            {
                ArgumentType.Int.TryParse(context, s, out value).Should().BeTrue();
                value.Should().Be(3 * 1024 * 1024);
            }

            ArgumentType.Int.TryParse(context, "1G", out value).Should().BeTrue();
            value.Should().Be(1024 * 1024 * 1024);

            ArgumentType.Long.TryParse(context, "2T", out value).Should().BeTrue();
            value.Should().Be(2L * 1024 * 1024 * 1024 * 1024);

            ArgumentType.Long.TryParse(context, "2P", out value).Should().BeTrue();
            value.Should().Be(2L * 1024 * 1024 * 1024 * 1024 * 1024);

            ArgumentType.Long.TryParse(context, "2E", out value).Should().BeTrue();
            value.Should().Be(2L * 1024 * 1024 * 1024 * 1024 * 1024 * 1024);

            ArgumentType.Long.TryParse(context, "2Z", out value).Should().BeFalse();
            ArgumentType.Long.TryParse(context, "2Y", out value).Should().BeFalse();
        }

        [TestMethod]
        public void ParsingDecimalUnits()
        {
            var context = new ArgumentParseContext { NumberOptions = NumberOptions.AllowMetricUnitSuffix };

            ArgumentType.Int.TryParse(context, "1.1234k", out object value).Should().BeFalse();

            ArgumentType.Int.TryParse(context, "1.5k", out value).Should().BeTrue();
            value.Should().Be(1500);

            ArgumentType.Int.TryParse(context, "1.333k", out value).Should().BeTrue();
            value.Should().Be(1333);

            ArgumentType.Int.TryParse(context, "-1.5k", out value).Should().BeTrue();
            value.Should().Be(-1500);

            ArgumentType.Int.TryParse(context, "1.5k", out value).Should().BeTrue();
            value.Should().Be(1500);

            ArgumentType.UInt.TryParse(context, "-1.5k", out value).Should().BeFalse();
        }

        [TestMethod]
        public void ParsingUnitsWithOverflow()
        {
            var context = new ArgumentParseContext { NumberOptions = NumberOptions.AllowBinaryMetricUnitSuffix };

            ArgumentType.Int.TryParse(context, "10GB", out object value).Should().BeFalse();
            ArgumentType.Int.TryParse(context, "10TB", out value).Should().BeFalse();
        }

        [TestMethod]
        public void CompletingString()
        {
            var completions = ArgumentType.String.GetCompletions(CreateContext(), "xyzzy").ToArray();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void CompletingChar()
        {
            var completions = ArgumentType.Char.GetCompletions(CreateContext(), "x").ToArray();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void CompletingGuid()
        {
            var completions = ArgumentType.Guid.GetCompletions(CreateContext(), "{0").ToArray();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void CompletingUri()
        {
            var completions = ArgumentType.Uri.GetCompletions(CreateContext(), "htt").ToArray();
            completions.Should().BeEmpty();
        }

        public enum TestEnum
        {
            Default,
            SomeValue,
            SomeOtherValue
        }

        [TestMethod]
        public void CompletingEnums()
        {
            var type = EnumArgumentType.Create(typeof(TestEnum));
            var c = CreateContext();

            type.GetCompletions(c, string.Empty).ToArray()
                .Should().Equal("Default", "SomeOtherValue", "SomeValue");

            type.GetCompletions(c, "a").ToArray().Should().BeEmpty();

            type.GetCompletions(c, "s").ToArray()
                .Should().Equal("SomeOtherValue", "SomeValue");

            type.GetCompletions(c, "S").ToArray()
                .Should().Equal("SomeOtherValue", "SomeValue");

            type.GetCompletions(c, "Defaulte").ToArray().Should().BeEmpty();

            type.GetCompletions(c, "Default").ToArray()
                .Should().Equal("Default");
        }

        [TestMethod]
        public void CompletingBools()
        {
            var type = ArgumentType.Bool;
            var c = CreateContext();

            type.GetCompletions(c, string.Empty).ToArray().Should().Equal("False", "True");

            type.GetCompletions(c, "a").ToArray().Should().BeEmpty();
            type.GetCompletions(c, "+").ToArray().Should().BeEmpty();
            type.GetCompletions(c, "-").ToArray().Should().BeEmpty();

            type.GetCompletions(c, "f").ToArray().Should().Equal("False");
            type.GetCompletions(c, "false").ToArray().Should().Equal("False");
            type.GetCompletions(c, "FA").ToArray().Should().Equal("False");
        }

        [TestMethod]
        public void GetTypeThrowsOnNull()
        {
            Action getAction = () => ArgumentType.GetType(null);
            getAction.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TryGetTypeThrowsOnNull()
        {
            Action getAction = () => ArgumentType.TryGetType(null, out IArgumentType ty);
            getAction.Should().Throw<ArgumentNullException>();
        }

        private static ArgumentCompletionContext CreateContext()
        {
            return new ArgumentCompletionContext
            {
                ParseContext = ArgumentParseContext.Default,
                Tokens = new List<string> { string.Empty },
                TokenIndex = 0
            };
        }
    }
}
