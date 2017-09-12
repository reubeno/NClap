﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Parser;

namespace NClap.Tests.Parser
{
    [TestClass]
    public class VerbParsingTests
    {
        class SomethingVerb : SynchronousVerb
        {
            public bool ThatSomething { get; set; }

            public override VerbResult Execute() => VerbResult.Success;
        }

        class OtherThingVerb : SynchronousVerb
        {
            public bool ThatOtherThing { get; set; }

            public override VerbResult Execute() => VerbResult.Success;
        }

        enum SimpleVerbType
        {
            [Verb(typeof(SomethingVerb))] Something,
            [Verb(typeof(OtherThingVerb))] OtherThing
        }

        class VerbArgument<TVerbType>
        {
            public TVerbType VerbType { get; set; }

            public T Get<T>(TVerbType verbType)
            {
                return (T)VerbArguments[verbType];
            }

            public IReadOnlyDictionary<TVerbType, object> VerbArguments { get; set; }
        }

        class SimpleArguments
        {
            [NamedArgument]
            public bool GlobalOption { get; set; }

            [PositionalArgument]
            public SimpleVerbType Verb { get; set; }
        }

        [TestMethod]
        public void SimpleVerbUsage()
        {
            var args = new SimpleArguments();
            CommandLineParser.Parse(new[] { "/GlobalOption", "OtherThing" }, args);
        }
    }
}