using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Repl;
using NClap.Tests.ConsoleInput;
using NSubstitute;

namespace NClap.Tests.Repl
{
    [TestClass]
    public class LoopTests
    {
        enum EmptyVerb
        {
        }

        enum OnlyExitableVerb
        {
            [Verb(typeof(ExitVerb))] Exit
        }

        enum TestVerb
        {
            [Verb(typeof(DoSomethingVerb))] DoSomething,
            [HelpVerb] Help,
            NoAttribute,
            [Verb(typeof(NoConstructorVerb))] NoConstructor,
            [Verb(typeof(NonVerb))] NonVerb,
            [Verb(typeof(ExitVerb))] Exit
        }

        class NonVerb
        {
        }

        class NoConstructorVerb : SynchronousVerb
        {
            public NoConstructorVerb(int notUsed)
            {
            }

            public override VerbResult Execute() => VerbResult.Success;
        }

        class DoSomethingVerb : SynchronousVerb
        {
            public override VerbResult Execute() => VerbResult.Success;
        }

        [TestMethod]
        public void ConstructorThrowsOnNullClient()
        {
            Action constructAction = () => new Loop<EmptyVerb>((ILoopClient)null, null);
            constructAction.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void LoopWithNoVerbs()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns((string)null);

            Action execute = () => Loop<EmptyVerb>.Execute(client);
            execute.ShouldNotThrow();
        }

        [TestMethod]
        public void LoopWithOnlyExitVerb()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("exit");

            Action execute = () => Loop<OnlyExitableVerb>.Execute(client);
            execute.ShouldNotThrow();
        }

        [TestMethod]
        public void LoopWithSimpleVerb()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("DoSomething", string.Empty, "&DoSomething", (string)null);

            var options = new LoopOptions { EndOfLineCommentCharacter = '&' };

            Loop<TestVerb>.Execute(client, options);
        }

        [TestMethod]
        public void NonVerbEnum()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("ThisNotAVerb", new string[] { null });

            Loop<TestVerb>.Execute(client, null);

            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void NonParseable()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("\"NotAVerb", new string[] { null });

            Loop<TestVerb>.Execute(client, null);

            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void NoConstructorInVerb()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("NoConstructor", new string[] { null });

            Loop<TestVerb>.Execute(client, null);
            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void ImplementingTypeIsNotAVerb()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("NonVerb", new string[] { null });

            Loop<TestVerb>.Execute(client, null);

            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void FailsParsingArgsToVerb()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("DoSomething DoesNotBelongHere", new string[] { null });

            Loop<TestVerb>.Execute(client, null);

            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void NotAnEnumValue()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("255", new string[] { null });

            Action executor = () => Loop<TestVerb>.Execute(client, null);
            executor.ShouldThrow<NotSupportedException>();
        }

        [TestMethod]
        public void GetHelp()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("Help", "Help help", "Help exit", (string)null);

            Loop<TestVerb>.Execute(client, null);
        }

        [TestMethod]
        public void Completions()
        {
            var client = Substitute.For<ILoopClient>();
            var loop = new Loop<TestVerb>(client, null);

            IReadOnlyList<string> completions = loop.GenerateCompletions(new string[] { }, 1).ToList();
            completions.Should().BeEmpty();

            completions = loop.GenerateCompletions(new string[] { }, 0).ToList();
            completions.Should().ContainInOrder("DoSomething", "Exit", "NoConstructor", "NonVerb");

            completions = loop.GenerateCompletions(new[] { "N" }, 0).ToList();
            completions.Should().ContainInOrder("NoConstructor", "NonVerb");

            completions = loop.GenerateCompletions(new[] { "DoSomething" }, 1).ToList();
            completions.Should().BeEmpty();

            completions = loop.GenerateCompletions(new[] { "BogusVerb" }, 1).ToList();
            completions.Should().BeEmpty();

            completions = loop.GenerateCompletions(new[] { "NoAttribute" }, 1).ToList();
            completions.Should().BeEmpty();

            completions = loop.GenerateCompletions(new[] { "Exit" }, 1).ToList();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void LoopCreatedWithIoParameters()
        {
            var keys = OnlyExitableVerb.Exit.ToString().AsKeys().Concat(
                ConsoleKey.Enter.ToKeyInfo());

            var input = new SimulatedConsoleInput(keys);
            var output = new SimulatedConsoleOutput();

            var parameters = new LoopInputOutputParameters
            {
                ConsoleInput = input,
                ConsoleOutput = output
            };

            Loop<OnlyExitableVerb>.Execute(parameters, null);
        }
    }
}
