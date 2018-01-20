using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Types;

namespace NClap.Tests.Parser
{
    [TestClass]
    public class TypeParsingTests
    {
        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class ArgumentsWithType<T>
        {
            [NamedArgument(ArgumentFlags.AtMostOnce, AllowEmpty = true)]
            public T Value;
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        public class ArgumentsWithCollectionType<T>
        {
            [NamedArgument(ArgumentFlags.Multiple, LongName = "Value")]
            public T Values;
        }

        public class CustomObjectType : CustomArgumentTypeBase
        {
            public static readonly CustomObjectType One = new CustomObjectType();
            public static readonly CustomObjectType Two = new CustomObjectType();

            public override bool TryParse(ArgumentParseContext context, string stringToParse, out object value)
            {
                switch (stringToParse.ToUpperInvariant())
                {
                case "ONE":
                    value = One;
                    return true;
                case "TWO":
                    value = Two;
                    return true;
                default:
                    value = null;
                    return false;
                }
            }

            public override string ToString()
            {
                if (this == One)
                {
                    return nameof(One);
                }

                if (this == Two)
                {
                    return nameof(Two);
                }

                return "<OTHER>";
            }
        }

        public class InvalidCustomObjectType : CustomArgumentTypeBase
        {
            public InvalidCustomObjectType(Exception exception)
            {
                throw exception;
            }

            public override bool TryParse(ArgumentParseContext context, string stringToParse, out object value)
            {
                throw new NotImplementedException();
            }
        }

        public enum TestEnum
        {
            Default = 0,

            [ArgumentValue]
            SomeValue,

            SomeOtherValue,

            [ArgumentValue(Flags = ArgumentValueFlags.Disallowed)]
            SomeDisallowedValue
        }

        [TestMethod]
        public void UnsupportedTypesForParsing()
        {
            Action parseAsObject = () => Parse(Array.Empty<string>(), out ArgumentsWithType<object> objectArgs);
            parseAsObject.Should().Throw<NotSupportedException>();

            Action parseAsPairOfObjects = () => Parse(Array.Empty<string>(), out ArgumentsWithType<KeyValuePair<object, object>> pairOfObjectsArgs);
            parseAsPairOfObjects.Should().Throw<NotSupportedException>();

            Action parseAsQueueOfInts = () => Parse(Array.Empty<string>(), out ArgumentsWithType<Queue<int>> queueOfIntsArgs);
            parseAsQueueOfInts.Should().Throw<NotSupportedException>();

            Action parseAsIEnumerableOfInts = () => Parse(Array.Empty<string>(), out ArgumentsWithType<IEnumerable<int>> iEnumerableOfIntsArgs);
            parseAsIEnumerableOfInts.Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void ParsingString()
        {
            ShouldParseAs(Array.Empty<string>(), (string)null);
            ShouldParseAs(new[] { "/value" }, string.Empty);
            ShouldParseAs(new[] { "/value=" }, string.Empty);
            ShouldParseAs(new[] { "/value=a" }, "a");
            ShouldParseAs(new[] { "/value=a b" }, "a b");
            ShouldParseAs(new[] { "/value= a" }, " a");
            ShouldParseAs(new[] { "/value=Ab" }, "Ab");
            ShouldParseAs(new[] { "/value=01" }, "01");
        }

        [TestMethod]
        public void FormattingString()
        {
            ShouldFormatAs(string.Empty, new[] { "/Value=" });
            ShouldFormatAs("a", new[] { "/Value=a" });
            ShouldFormatAs("a b", new[] { "/Value=a b" });
        }

        [TestMethod]
        public void ParsingChar()
        {
            ShouldParseAs(Array.Empty<string>(), '\0');
            ShouldParseAs(new[] { "/value=a" }, 'a');
            ShouldParseAs(new[] { "/value=7" }, '7');
            ShouldParseAs(new[] { "/value=\n"}, '\n');

            ShouldFailToParse<char>(new[] { "/value" });
            ShouldFailToParse<char>(new[] { "/value=" });
            ShouldFailToParse<char>(new[] { "/value=abc" });
            ShouldFailToParse<char>(new[] { "/value= a " });
        }

        [TestMethod]
        public void FormattingChar()
        {
            ShouldFormatAs('a', new[] { "/Value=a" });
            ShouldFormatAs('A', new[] { "/Value=A" });
            ShouldFormatAs(' ', new[] { "/Value= " });
            ShouldFormatAs('\n', new[] { "/Value=\n" });
        }

        [TestMethod]
        public void ParsingGuid()
        {
            ShouldParseAs(Array.Empty<string>(), Guid.Empty);
            ShouldParseAs(new[] { "/value=00000000-0000-0000-0000-000000000000" }, Guid.Empty);
            ShouldParseAs(new[] { "/value=00000000000000000000000000000000" }, Guid.Empty);
            ShouldParseAs(new[] { "/value={00000000-0000-0000-0000-000000000000}" }, Guid.Empty);
            ShouldParseAs(new[] { "/value={9F4A81EB-4DEB-4492-93FC-0E4C0DF01B21}" }, Guid.Parse("{9F4A81EB-4DEB-4492-93FC-0E4C0DF01B21}"));
            ShouldParseAs(new[] { "/value={9f4a81eb-4deb-4492-93fc-0e4c0df01b21}" }, Guid.Parse("{9F4A81EB-4DEB-4492-93FC-0E4C0DF01B21}"));
            ShouldParseAs(new[] { "/value=9F4A81EB4DEB449293FC0E4C0DF01B21" }, Guid.Parse("{9F4A81EB-4DEB-4492-93FC-0E4C0DF01B21}"));
            ShouldParseAs(new[] { "/value=9f4a81eb4deb449293fc0e4c0df01b21" }, Guid.Parse("{9F4A81EB-4DEB-4492-93FC-0E4C0DF01B21}"));

            ShouldFailToParse<Guid>(new[] { "/value" });
            ShouldFailToParse<Guid>(new[] { "/value=" });
            ShouldFailToParse<Guid>(new[] { "/value=abc" });
            ShouldFailToParse<Guid>(new[] { "/value=0" });
            ShouldFailToParse<Guid>(new[] { "/value={00000000000000000000000000000000}" });
            ShouldFailToParse<Guid>(new[] { "/value={9F4A81EB4DEB449293FC0E4C0DF01B21}" });
        }

        [TestMethod]
        public void FormattingGuid()
        {
            ShouldFormatAs(Guid.Empty, Array.Empty<string>());
            ShouldFormatAs(Guid.Parse("9F4A81EB-4DEB-4492-93FC-0E4C0DF01B21"), new[] { "/Value=9f4a81eb-4deb-4492-93fc-0e4c0df01b21" });
        }

        [TestMethod]
        public void ParsingUri()
        {
            ShouldParseAs(Array.Empty<string>(), (Uri)null);
            ShouldParseAs(new[] { "/value=http://www.microsoft.com" }, new Uri("http://www.microsoft.com"));
            ShouldParseAs(new[] { "/value=mailto:noone" }, new Uri("mailto:noone"));

            ShouldFailToParse<Uri>(new[] { "/value=3invalid/21" });
            ShouldFailToParse<Uri>(new[] { "/value=" });
        }

        [TestMethod]
        public void FormattingUri()
        {
            ShouldFormatAs(new Uri("http://www.microsoft.com/"), new[] { "/Value=http://www.microsoft.com/" });
        }

        [TestMethod]
        public void ParsingPath()
        {
            CommandLineParser.TryParse(Array.Empty<string>(), out ArgumentsWithType<FileSystemPath> args).Should().BeTrue();
            args.Value.Should().BeNull();

            CommandLineParser.TryParse(new[] { @"/value=c:\temp" }, out ArgumentsWithType<FileSystemPath> args2).Should().BeTrue();
            args2.Value.Should().Be((FileSystemPath)@"c:\temp");
        }

        [TestMethod]
        public void FormattingPath()
        {
            ShouldFormatAs((FileSystemPath)@"c:\temp", new[] { @"/Value=c:\temp" });
        }

        [TestMethod]
        public void ParsingBool()
        {
            ShouldParseAs(Array.Empty<string>(), false);

            ShouldParseAs(new[] { "/value" }, true);
            ShouldParseAs(new[] { "/value=true" }, true);
            ShouldParseAs(new[] { "/value=True" }, true);
            ShouldParseAs(new[] { "/value=false" }, false);
            ShouldParseAs(new[] { "/value=False" }, false);
            ShouldParseAs(new[] { "/value+" }, true);
            ShouldParseAs(new[] { "/value-" }, false);
            ShouldParseAs(new[] { "/value=+" }, true);
            ShouldParseAs(new[] { "/value=-" }, false);

            ShouldFailToParse<bool>(new[] { "/value=1" });
            ShouldFailToParse<bool>(new[] { "/value=foo" });
            ShouldFailToParse<bool>(new[] { "/value=value" });
        }

        [TestMethod]
        public void FormattingBool()
        {
            ShouldFormatAs(false, Array.Empty<string>());
            ShouldFormatAs(true, new[] { "/Value=True" });
        }

        [TestMethod]
        public void ParsingInt()
        {
            ShouldParseAs(Array.Empty<string>(), 0);
            ShouldParseAs(new[] { "/value=0" }, 0);
            ShouldParseAs(new[] { "/value=1" }, 1);
            ShouldParseAs(new[] { "/value=-1" }, -1);
            ShouldParseAs(new[] { "/value=16" }, 16);
            ShouldParseAs(new[] { "/value=16 " }, 16);
            ShouldParseAs(new[] { "/value= 16" }, 16);
            ShouldParseAs(new[] { "/value=016" }, 16);
            ShouldParseAs(new[] { "/value=0n16" }, 16);
            ShouldParseAs(new[] { "/value=0x10" }, 16);
            ShouldParseAs(new[] { "/value=0x0" }, 0);

            ShouldFailToParse<int>(new[] { "/value" });
            ShouldFailToParse<int>(new[] { "/value=--5" });
            ShouldFailToParse<int>(new[] { "/value=16." });
            ShouldFailToParse<int>(new[] { "/value=_16" });
            ShouldFailToParse<int>(new[] { "/value=9999999999999" });
            ShouldFailToParse<int>(new[] { "/value=-9999999999999" });
            ShouldFailToParse<int>(new[] { "/value=0N16" });
            ShouldFailToParse<int>(new[] { "/value=0X16" });
        }

        [TestMethod]
        public void FormattingInt()
        {
            ShouldFormatAs(0, Array.Empty<string>());
            ShouldFormatAs(1, new[] { "/Value=1" });
            ShouldFormatAs(1000, new[] { "/Value=1000" });
            ShouldFormatAs(-10, new[] { "/Value=-10" });
        }

        [TestMethod]
        public void ParsingUInt()
        {
            ShouldParseAs<uint>(Array.Empty<string>(), 0);
            ShouldParseAs<uint>(new[] { "/value=0" }, 0);
            ShouldParseAs<uint>(new[] { "/value=1" }, 1);
            ShouldParseAs<uint>(new[] { "/value=16" }, 16);
            ShouldParseAs<uint>(new[] { "/value=16 " }, 16);
            ShouldParseAs<uint>(new[] { "/value= 16" }, 16);
            ShouldParseAs<uint>(new[] { "/value=016" }, 16);
            ShouldParseAs<uint>(new[] { "/value=0n16" }, 16);
            ShouldParseAs<uint>(new[] { "/value=0x10" }, 16);
            ShouldParseAs<uint>(new[] { "/value=0x0" }, 0);

            ShouldFailToParse<uint>(new[] { "/value" });
            ShouldFailToParse<uint>(new[] { "/value=-1" });
            ShouldFailToParse<uint>(new[] { "/value=16." });
            ShouldFailToParse<uint>(new[] { "/value=_16" });
            ShouldFailToParse<uint>(new[] { "/value=9999999999999" });
            ShouldFailToParse<uint>(new[] { "/value=0N16" });
            ShouldFailToParse<uint>(new[] { "/value=0X16" });
        }

        [TestMethod]
        public void FormattingUInt()
        {
            ShouldFormatAs<uint>(0, Array.Empty<string>());
            ShouldFormatAs<uint>(1, new[] { "/Value=1" });
            ShouldFormatAs<uint>(1000, new[] { "/Value=1000" });
        }

        [TestMethod]
        public void ParsingDecimal()
        {
            ShouldParseAs(Array.Empty<string>(), 0.0M);
            ShouldParseAs(new[] { "/value=0" }, 0.0M);
            ShouldParseAs(new[] { "/value=0." }, 0.0M);
            ShouldParseAs(new[] { "/value=0.0" }, 0.0M);
            ShouldParseAs(new[] { "/value=-0.0" }, 0.0M);
            ShouldParseAs(new[] { "/value=1.2" }, 1.2M);
            ShouldParseAs(new[] { "/value=-1.2" }, -1.2M);

            ShouldFailToParse<decimal>(new[] { "/value" });
        }

        [TestMethod]
        public void FormattingDecimal()
        {
            ShouldFormatAs(0.0M, Array.Empty<string>());
            ShouldFormatAs(1.2M, new[] { "/Value=1.2" });
            ShouldFormatAs(-2.3M, new[] { "/Value=-2.3" });
        }

        [TestMethod]
        public void ParsingFloat()
        {
            ShouldParseAs(Array.Empty<string>(), 0.0F);
            ShouldParseAs(new[] { "/value=0" }, 0.0F);
            ShouldParseAs(new[] { "/value=0." }, 0.0F);
            ShouldParseAs(new[] { "/value=0.0" }, 0.0F);
            ShouldParseAs(new[] { "/value=-0.0" }, 0.0F);
            ShouldParseAs(new[] { "/value=1.2" }, 1.2F);
            ShouldParseAs(new[] { "/value=-1.2" }, -1.2F);

            ShouldFailToParse<float>(new[] { "/value" });
        }

        [TestMethod]
        public void FormattingFloat()
        {
            ShouldFormatAs(0.0F, Array.Empty<string>());
            ShouldFormatAs(1.2F, new[] { "/Value=1.2" });
            ShouldFormatAs(-2.3F, new[] { "/Value=-2.3" });
        }

        [TestMethod]
        public void ParsingDouble()
        {
            ShouldParseAs(Array.Empty<string>(), 0.0D);
            ShouldParseAs(new[] { "/value=0" }, 0.0D);
            ShouldParseAs(new[] { "/value=0." }, 0.0D);
            ShouldParseAs(new[] { "/value=0.0" }, 0.0D);
            ShouldParseAs(new[] { "/value=-0.0" }, 0.0D);
            ShouldParseAs(new[] { "/value=1.2" }, 1.2D);
            ShouldParseAs(new[] { "/value=-1.2" }, -1.2D);

            ShouldFailToParse<double>(new[] { "/value" });
        }

        [TestMethod]
        public void FormattingDouble()
        {
            ShouldFormatAs(0.0D, Array.Empty<string>());
            ShouldFormatAs(1.2D, new[] { "/Value=1.2" });
            ShouldFormatAs(-2.3D, new[] { "/Value=-2.3" });
        }

        [TestMethod]
        public void ParsingEnum()
        {
            ShouldParseAs(Array.Empty<string>(), TestEnum.Default);
            ShouldParseAs(new[] { "/value=Default" }, TestEnum.Default);
            ShouldParseAs(new[] { "/value=0" }, TestEnum.Default);
            ShouldParseAs(new[] { "/value=SomeValue" }, TestEnum.SomeValue);
            ShouldParseAs(new[] { "/value=SOMEVALUE" }, TestEnum.SomeValue);

            ShouldFailToParse<TestEnum>(new[] { "/value" });
            ShouldFailToParse<TestEnum>(new[] { "/value=" });
            ShouldFailToParse<TestEnum>(new[] { "/value= " });
            ShouldFailToParse<TestEnum>(new[] { "/value=Some Value" });
            ShouldFailToParse<TestEnum>(new[] { "/value= SomeValue " });
            ShouldFailToParse<TestEnum>(new[] { "/value=0x0" });
            ShouldFailToParse<TestEnum>(new[] { "/value=99" });
            ShouldFailToParse<TestEnum>(new[] { "/value=-1" });
            ShouldFailToParse<TestEnum>(new[] { "/value=SomeDisallowedValue" });
        }

        [TestMethod]
        public void FormattingEnum()
        {
            ShouldFormatAs(TestEnum.Default, Array.Empty<string>());
            ShouldFormatAs(TestEnum.SomeOtherValue, new[] { "/Value=SomeOtherValue" });
            ShouldFormatAs(TestEnum.SomeValue, new[] { "/Value=SomeValue" });
            ShouldFormatAs(TestEnum.SomeDisallowedValue, new[] { "/Value=SomeDisallowedValue" });
            ShouldFormatAs((TestEnum)0xFFFF, new[] { "/Value=65535" });
        }

        [TestMethod]
        public void ParsingKeyValuePairOfStrings()
        {
            ShouldParseAs(Array.Empty<string>(), new KeyValuePair<string, string>(null, null));
            ShouldParseAs(new[] { "/value=a=b" }, new KeyValuePair<string, string>("a", "b"));
            ShouldParseAs(new[] { "/value=a b=c d" }, new KeyValuePair<string, string>("a b", "c d"));
            ShouldParseAs(new[] { "/value==" }, new KeyValuePair<string, string>(string.Empty, string.Empty));
            ShouldParseAs(new[] { "/value===" }, new KeyValuePair<string, string>(string.Empty, "="));

            ShouldFailToParse<KeyValuePair<string, string>>(new[] { "/value" });
            ShouldFailToParse<KeyValuePair<string, string>>(new[] { "/value=" });
            ShouldFailToParse<KeyValuePair<string, string>>(new[] { "/value=ab" });
        }

        [TestMethod]
        public void FormattingKeyValuePairOfStrings()
        {
            ShouldFormatAs(new KeyValuePair<string, string>("a", "b"), new[] { "/Value=a=b" });
            ShouldFormatAs(new KeyValuePair<string, string>("a b", "c d"), new[] { "/Value=a b=c d" });
            ShouldFormatAs(new KeyValuePair<string, string>(string.Empty, string.Empty), new[] { "/Value==" });
        }

        [TestMethod]
        public void ParsingKeyValuePairOfStringAndInt()
        {
            ShouldParseAs(Array.Empty<string>(), new KeyValuePair<string, int>(null, 0));
            ShouldParseAs(new[] { "/value=a=0" }, new KeyValuePair<string, int>("a", 0));
            ShouldParseAs(new[] { "/value=a b=0x20" }, new KeyValuePair<string, int>("a b", 32));
            ShouldParseAs(new[] { "/value==-10" }, new KeyValuePair<string, int>(string.Empty, -10));

            ShouldFailToParse<KeyValuePair<string, int>>(new[] { "/value" });
            ShouldFailToParse<KeyValuePair<string, int>>(new[] { "/value=" });
            ShouldFailToParse<KeyValuePair<string, int>>(new[] { "/value==" });
            ShouldFailToParse<KeyValuePair<string, int>>(new[] { "/value=a=" });
            ShouldFailToParse<KeyValuePair<string, int>>(new[] { "/value=ab" });
        }

        [TestMethod]
        public void ParsingKeyValuePairOfIntAndInt()
        {
            ShouldParseAs(Array.Empty<string>(), new KeyValuePair<int, int>(0, 0));
            ShouldParseAs(new[] { "/value=3=4" }, new KeyValuePair<int, int>(3, 4));
            ShouldParseAs(new[] { "/value= 3 =7" }, new KeyValuePair<int, int>(3, 7));

            ShouldFailToParse<KeyValuePair<int, int>>(new[] { "/value" });
            ShouldFailToParse<KeyValuePair<int, int>>(new[] { "/value=" });
            ShouldFailToParse<KeyValuePair<int, int>>(new[] { "/value==" });
            ShouldFailToParse<KeyValuePair<int, int>>(new[] { "/value=0=" });
            ShouldFailToParse<KeyValuePair<int, int>>(new[] { "/value=4" });
        }

        [TestMethod]
        public void ParsingTupleOfInt()
        {
            ShouldParseAs(Array.Empty<string>(), (Tuple<int>)null);

            ShouldParseAs(new[] { "/value=3" }, Tuple.Create(3));
            ShouldFailToParse<Tuple<int>>(new[] { "/value" });
            ShouldFailToParse<Tuple<int>>(new[] { "/value=" });
            ShouldFailToParse<Tuple<int>>(new[] { "/value=3,4" });

            ShouldParseAs(new[] { "/value=3,4" }, Tuple.Create(3, 4));
            ShouldFailToParse<Tuple<int, int>>(new[] { "/value" });
            ShouldFailToParse<Tuple<int, int>>(new[] { "/value=" });
            ShouldFailToParse<Tuple<int, int>>(new[] { "/value=3" });
            ShouldFailToParse<Tuple<int, int>>(new[] { "/value=3," });
            ShouldFailToParse<Tuple<int, int>>(new[] { "/value=,4" });
            ShouldFailToParse<Tuple<int, int>>(new[] { "/value=,," });

            ShouldParseAs(new[] { "/value=3,4,5" }, Tuple.Create(3, 4, 5));
            ShouldParseAs(new[] { "/value=3,hello,5" }, Tuple.Create(3, "hello", 5));
            ShouldParseAs(new[] { "/value=3,1=1,5" }, Tuple.Create(3, new KeyValuePair<int, int>(1, 1), 5));
        }

        [TestMethod]
        public void FormattingTuples()
        {
            ShouldFormatAs(Tuple.Create(3, 4, 5), new[] { "/Value=3,4,5" });
            ShouldFormatAs(Tuple.Create(string.Empty, string.Empty), new[] { "/Value=," });
        }

        [TestMethod]
        public void ParsingNullableTypes()
        {
            ShouldFailToParse<int?>(new[] { "/value" });
            ShouldFailToParse<int?>(new[] { "/value=" });
            ShouldParseAs(Array.Empty<string>(), (int?)null);
            ShouldParseAs(new[] { "/value=1" }, new int?(1));

            ShouldParseAs(Array.Empty<string>(), (Guid?)null);
            ShouldParseAs(new[] { "/value=00000000-0000-0000-0000-000000000000" }, new Guid?(Guid.Empty));

            ShouldParseAs(Array.Empty<string>(), (bool?)null);
            ShouldParseAs(new[] { "/value" }, new bool?(true));
            ShouldParseAs(new[] { "/value+" }, new bool?(true));
            ShouldParseAs(new[] { "/value-" }, new bool?(false));
        }

        [TestMethod]
        public void FormattingNullableTypes()
        {
            ShouldFormatAs(new int?(3), new[] { "/Value=3" });
            ShouldFormatAs((int?)null, Array.Empty<string>());
        }

        [TestMethod]
        public void ParsingCustomObjectType()
        {
            ShouldFailToParse<CustomObjectType>(new[] { "/value" });
            ShouldFailToParse<CustomObjectType>(new[] { "/value=" });
            ShouldFailToParse<CustomObjectType>(new[] { "/value= Two " });

            ShouldParseAs(new[] { "/value=one" }, CustomObjectType.One);
            ShouldParseAs(new[] { "/value=ONE" }, CustomObjectType.One);
            ShouldParseAs(new[] { "/value=two" }, CustomObjectType.Two);
            ShouldParseAs(new[] { "/value=Two" }, CustomObjectType.Two);
        }

        [TestMethod]
        public void UsingCustomObjectType()
        {
            var customObjectType = CustomObjectType.One;

            customObjectType.Type.Should().BeSameAs(customObjectType.GetType());
            customObjectType.DisplayName.Should().Be(nameof(CustomObjectType));
            customObjectType.SyntaxSummary.Should().Be("<CustomObjectType>");

            Action formattingNull = () => customObjectType.Format(null);
            formattingNull.Should().Throw<ArgumentNullException>();

            Action formattingObjectOfWrongType = () => customObjectType.Format(7);
            formattingObjectOfWrongType.Should().Throw<ArgumentOutOfRangeException>();

            customObjectType.Format(CustomObjectType.Two).Should().Be("Two");
        }

        [TestMethod]
        public void CompletingCustomObjectType()
        {
            var customObjectType = CustomObjectType.One;
            var completions = customObjectType.GetCompletions(CreateContext(), "x").ToArray();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void ParsingInvalidCustomObjectType()
        {
            Action tryParse = () => Parse(Array.Empty<string>(), out ArgumentsWithType<InvalidCustomObjectType> args);
            tryParse.Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void ParsingArrayOfStrings()
        {
            ShouldParseCollectionAs<int[]>(
                Array.Empty<string>(),
                values => values.Length == 0);

            ShouldParseCollectionAs<int[]>(
                new[] { "/value:10", "/value:5" },
                values => (values.Length == 2) && (values[0] == 10) && (values[1] == 5));

            ShouldFailToParseCollection<int[]>(new[] { "/value" });
        }

        [TestMethod]
        public void ListOfIntsShouldParse()
        {
            ShouldFailToParseCollection<List<int>>(new[] { "/value" });
            ShouldParseCollectionAs<List<int>>(Array.Empty<string>(), values => values.Count == 0);
            ShouldParseCollectionAs<List<int>>(new[] { "/value:10", "/value:5" },
                values => (values.Count == 2) && (values[0] == 10) && (values[1] == 5));
        }

        [TestMethod]
        public void ListOfIntsShouldFormat()
        {
            ShouldFormatCollectionAs(new List<int>(), Array.Empty<string>());
            ShouldFormatCollectionAs(new List<int>(new[] { 10, 5 }), new[] { "/Value=10", "/Value=5" });
        }

        [TestMethod]
        public void LinkedListOfIntsShouldParse()
        {
            ShouldFailToParseCollection<LinkedList<int>>(new[] { "/value" });
            ShouldParseCollectionAs<LinkedList<int>>(Array.Empty<string>(), values => values.Count == 0);
            ShouldParseCollectionAs<LinkedList<int>>(new[] { "/value:10", "/value:5" },
                values => (values.Count == 2) && (values.First() == 10) && (values.Last() == 5));
        }

        [TestMethod]
        public void HashSetOfIntsShouldParse()
        {
            ShouldFailToParseCollection<HashSet<int>>(new[] { "/value" });
            ShouldParseCollectionAs<HashSet<int>>(Array.Empty<string>(), values => values.Count == 0);
            ShouldParseCollectionAs<HashSet<int>>(new[] { "/value:10", "/value:5" },
                values => (values.Count == 2) && (values.First() == 10) && (values.Last() == 5));
            ShouldParseCollectionAs<HashSet<int>>(new[] { "/value:10", "/value:10" },
                values => (values.Count == 1) && (values.Single() == 10));
        }

        [TestMethod]
        public void SortedSetOfIntsShouldParse()
        {
            ShouldFailToParseCollection<SortedSet<int>>(new[] { "/value" });
            ShouldParseCollectionAs<SortedSet<int>>(Array.Empty<string>(), values => values.Count == 0);
            ShouldParseCollectionAs<SortedSet<int>>(new[] { "/value:10", "/value:5" },
                values => (values.Count == 2) && (values.First() == 5) && (values.Last() == 10));
            ShouldParseCollectionAs<SortedSet<int>>(new[] { "/value:10", "/value:10" },
                values => (values.Count == 1) && (values.Single() == 10));
        }

        [TestMethod]
        public void DictionaryOfIntsShouldParse()
        {
            ShouldFailToParseCollection<Dictionary<int, int>>(new[] { "/value" });
            ShouldParseCollectionAs<Dictionary<int, int>>(Array.Empty<string>(),
                values => values.Count == 0);
            ShouldParseCollectionAs<Dictionary<int, int>>(new[] { "/value:10=9", "/value:5=4" },
                values => (values.Count == 2) && (values[10] == 9) && (values[5] == 4));
            ShouldFailToParseCollection<Dictionary<int, int>>(new[] {"/value:10=9", "/value:10=4"});
        }

        [TestMethod]
        public void SortedDictionaryOfIntsShouldParse()
        {
            ShouldFailToParseCollection<SortedDictionary<int, int>>(new[] { "/value" });
            ShouldParseCollectionAs<SortedDictionary<int, int>>(Array.Empty<string>(),
                values => values.Count == 0);
            ShouldParseCollectionAs<SortedDictionary<int, int>>(new[] { "/value:10=9", "/value:5=4" },
                values => (values.Count == 2) && (values[10] == 9) && (values[5] == 4) &&
                          (values.Keys.First() == 5) && (values.Keys.Last() == 10));
        }

        [TestMethod]
        public void SortedListOfIntsShouldParse()
        {
            ShouldFailToParseCollection<SortedList<int, int>>(new[] { "/value" });
            ShouldParseCollectionAs<SortedList<int, int>>(Array.Empty<string>(),
                values => values.Count == 0);
            ShouldParseCollectionAs<SortedList<int, int>>(new[] { "/value:10=9", "/value:5=4" },
                values => (values.Count == 2) && (values[10] == 9) && (values[5] == 4) &&
                          (values.Keys.First() == 5) && (values.Keys.Last() == 10));
        }

        private static void ShouldFormatAs<T>(T value, IEnumerable<string> expectedArgs)
        {
            var expectedArgsList = expectedArgs.ToList();

            var formatted = CommandLineParser.Format(new ArgumentsWithType<T> { Value = value });
            if (expectedArgsList.Any())
            {
                formatted.Should().Equal(expectedArgsList);
            }
            else
            {
                formatted.Should().BeEmpty();
            }
        }

        private static void ShouldFormatCollectionAs<T>(T value, IEnumerable<string> expectedArgs)
        {
            var expectedArgsList = expectedArgs.ToList();

            var formatted = CommandLineParser.Format(new ArgumentsWithCollectionType<T> { Values = value });
            if (expectedArgsList.Any())
            {
                formatted.Should().Equal(expectedArgsList);
            }
            else
            {
                formatted.Should().BeEmpty();
            }
        }

        private static void ShouldParseAs<T>(IEnumerable<string> args, T expectedValue) =>
            ValidateParse(args, true, expectedValue);

        private static void ShouldFailToParse<T>(IEnumerable<string> args) =>
            ValidateParse<T>(args, false);

        private static void ShouldParseCollectionAs<T>(IEnumerable<string> args, Func<T, bool> valueValidator) =>
            ValidateCollectionParse(args, true, valueValidator);

        private static void ShouldFailToParseCollection<T>(IEnumerable<string> args) =>
            ValidateCollectionParse<T>(args, false);

        private static void ValidateCollectionParse<T>(IEnumerable<string> args, bool expectedParseResult, Func<T, bool> valueValidator = null)
        {
            Parse(args, out ArgumentsWithCollectionType<T> parsedArgs).Should().Be(expectedParseResult);
            if (expectedParseResult)
            {
                valueValidator?.Invoke(parsedArgs.Values).Should().BeTrue();
            }
        }

        private static void ValidateParse<T>(IEnumerable<string> args, bool expectedParseResult, T expectedValue = default(T))
        {
            var argsList = args.ToList();

            Parse(argsList, out ArgumentsWithType<T> parsedArgs).Should().Be(expectedParseResult, "because we're parsing: \"{0}\"", string.Join(" ", argsList));
            if (expectedParseResult)
            {
                parsedArgs.Value.Should().Be(expectedValue, "because we're parsing: \"{0}\"", string.Join(" ", argsList));
            }
        }

        private static bool Parse<T>(IEnumerable<string> args, out T parsedArgs) where T : class, new() =>
            CommandLineParser.TryParse<T>(args, new CommandLineParserOptions { DisplayUsageInfoOnError = false }, out parsedArgs);

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
