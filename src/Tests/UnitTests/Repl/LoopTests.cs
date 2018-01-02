using System;
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
        enum EmptyCommand
        {
        }

        enum OnlyExitableCommand
        {
            [Command(typeof(ExitCommand))] Exit
        }

        enum TestCommand
        {
            [Command(typeof(DoSomethingCommand))] DoSomething,
            [HelpCommand] Help,
            NoAttribute,
            [Command(typeof(NoConstructorCommand))] NoConstructor,
            [Command(typeof(NonCommand))] NonCommand,
            [Command(typeof(ExitCommand))] Exit
        }

        class NonCommand
        {
        }

        class NoConstructorCommand : SynchronousCommand
        {
            public NoConstructorCommand(int notUsed)
            {
            }

            public override CommandResult Execute() => CommandResult.Success;
        }

        class DoSomethingCommand : SynchronousCommand
        {
            public override CommandResult Execute() => CommandResult.Success;
        }

        [TestMethod]
        public void ConstructorThrowsOnNullClient()
        {
            Action constructAction = () => { var x = new Loop(typeof(EmptyCommand), (ILoopClient)null); };
            constructAction.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void LoopWithNoCommands()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns((string)null);

            Action execute = () => new Loop(typeof(EmptyCommand), client).Execute();
            execute.Should().NotThrow();
        }

        [TestMethod]
        public void LoopWithOnlyExitCommand()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("exit");

            Action execute = () => new Loop(typeof(OnlyExitableCommand), client).Execute();
            execute.Should().NotThrow();
        }

        [TestMethod]
        public void LoopWithSimpleCommand()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("DoSomething", string.Empty, "&DoSomething", (string)null);
            client.EndOfLineCommentCharacter.Returns('&');

            var loop = new Loop(typeof(TestCommand), client);
            loop.Execute();
        }

        [TestMethod]
        public void NonCommandEnum()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("ThisNotACommand", new string[] { null });

            new Loop(typeof(TestCommand), client).Execute();
            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void NonParseable()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("\"NotACommand", new string[] { null });

            new Loop(typeof(TestCommand), client).Execute();
            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void NoConstructorInCommand()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("NoConstructor", new string[] { null });

            new Loop(typeof(TestCommand), client).Execute();
            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void ImplementingTypeIsNotACommand()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("NonCommand", new string[] { null });

            new Loop(typeof(TestCommand), client).Execute();
            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void FailsParsingArgsToCommand()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("DoSomething DoesNotBelongHere", new string[] { null });

            new Loop(typeof(TestCommand), client).Execute();
            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void NotAnEnumValue()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("255", new string[] { null });

            new Loop(typeof(TestCommand), client).ExecuteOnce()
                .Should().Be(CommandResult.UsageError);
        }

        [TestMethod]
        public void GetHelp()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("Help", "Help help", "Help exit", (string)null);

            new Loop(typeof(TestCommand), client).Execute();
        }

        [TestMethod]
        public void InvalidTokenCannotBeCompleted()
        {
            Action action = () => new Loop(typeof(TestCommand)).GetCompletions(new string[] { }, 1).ToList();
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void CommandsCanBeCompleted()
        {
            // TODO: We're not thrilled that 'NoAttribute' shows up below; needs revisiting.
            var completions = new Loop(typeof(TestCommand)).GetCompletions(new string[] { }, 0).ToList();
            completions.Should().Equal("DoSomething", "Exit", "Help", "NoAttribute", "NoConstructor", "NonCommand");
        }

        [TestMethod]
        public void CommandPrefixCanBeCompleted()
        {
            // TODO: We're not thrilled that 'NoAttribute' shows up below; needs revisiting.
            var completions = new Loop(typeof(TestCommand)).GetCompletions(new[] { "N" }, 0).ToList();
            completions.Should().Equal("NoAttribute", "NoConstructor", "NonCommand");
        }

        [TestMethod]
        public void ParameterlessCommandHasNoArgCompletions()
        {
            var completions = new Loop(typeof(TestCommand)).GetCompletions(new[] { "DoSomething" }, 1).ToList();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void BogusCommandGetsNoCompletions()
        {
            var completions = new Loop(typeof(TestCommand)).GetCompletions(new[] { "BogusCommand" }, 1).ToList();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void AttributeLessEnumValueCannotBeCompleted()
        {
            var completions = new Loop(typeof(TestCommand)).GetCompletions(new[] { "NoAttribute" }, 1).ToList();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void ExitCommandGetsNoArgCompletion()
        {
            var completions = new Loop(typeof(TestCommand)).GetCompletions(new[] { "Exit" }, 1).ToList();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void LoopCreatedWithIoParameters()
        {
            var keys = OnlyExitableCommand.Exit.ToString().AsKeys().Concat(
                ConsoleKey.Enter.ToKeyInfo());

            var input = new SimulatedConsoleInput(keys);
            var output = new SimulatedConsoleOutput();

            var parameters = new LoopInputOutputParameters
            {
                ConsoleInput = input,
                ConsoleOutput = output
            };

            new Loop(typeof(OnlyExitableCommand), parameters).Execute();
        }
    }
}
