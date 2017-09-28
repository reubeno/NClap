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
            Action constructAction = () => { var x = new Loop<EmptyCommand>((ILoopClient)null, null); };
            constructAction.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void LoopWithNoCommands()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns((string)null);

            Action execute = () => new Loop<EmptyCommand>(client).Execute();
            execute.ShouldNotThrow();
        }

        [TestMethod]
        public void LoopWithOnlyExitCommand()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("exit");

            Action execute = () => new Loop<OnlyExitableCommand>(client).Execute();
            execute.ShouldNotThrow();
        }

        [TestMethod]
        public void LoopWithSimpleCommand()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("DoSomething", string.Empty, "&DoSomething", (string)null);

            var options = new LoopOptions { EndOfLineCommentCharacter = '&' };

            new Loop<TestCommand>(client, options).Execute();
        }

        [TestMethod]
        public void NonCommandEnum()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("ThisNotACommand", new string[] { null });

            new Loop<TestCommand>(client, null).Execute();

            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void NonParseable()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("\"NotACommand", new string[] { null });

            new Loop<TestCommand>(client, null).Execute();

            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void NoConstructorInCommand()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("NoConstructor", new string[] { null });

            new Loop<TestCommand>(client, null).Execute();
            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void ImplementingTypeIsNotACommand()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("NonCommand", new string[] { null });

            new Loop<TestCommand>(client, null).Execute();

            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void FailsParsingArgsToCommand()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("DoSomething DoesNotBelongHere", new string[] { null });

            new Loop<TestCommand>(client, null).Execute();

            client.Received().OnError(Arg.Any<string>());
        }

        [TestMethod]
        public void NotAnEnumValue()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("255", new string[] { null });

            new Loop<TestCommand>(client, null).ExecuteOnce().Should().Be(CommandResult.UsageError);
        }

        [TestMethod]
        public void GetHelp()
        {
            var client = Substitute.For<ILoopClient>();
            client.ReadLine().Returns("Help", "Help help", "Help exit", (string)null);

            new Loop<TestCommand>(client, null).Execute();
        }

        [TestMethod]
        public void InvalidTokenCannotBeCompleted()
        {
            Action action = () => Loop<TestCommand>.GenerateCompletions(new string[] { }, 1).ToList();
            action.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void CommandsCanBeCompleted()
        {
            var completions = Loop<TestCommand>.GenerateCompletions(new string[] { }, 0).ToList();
            completions.Should().ContainInOrder("DoSomething", "Exit", "NoConstructor", "NonCommand");
        }

        [TestMethod]
        public void CommandPrefixCanBeCompleted()
        {
            var completions = Loop<TestCommand>.GenerateCompletions(new[] { "N" }, 0).ToList();
            completions.Should().ContainInOrder("NoConstructor", "NonCommand");
        }

        [TestMethod]
        public void ParameterlessCommandHasNoArgCompletions()
        {
            var completions = Loop<TestCommand>.GenerateCompletions(new[] { "DoSomething" }, 1).ToList();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void BogusCommandGetsNoCompletions()
        {
            var completions = Loop<TestCommand>.GenerateCompletions(new[] { "BogusCommand" }, 1).ToList();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void AttributeLessEnumValueCannotBeCompleted()
        {
            var completions = Loop<TestCommand>.GenerateCompletions(new[] { "NoAttribute" }, 1).ToList();
            completions.Should().BeEmpty();
        }

        [TestMethod]
        public void ExitCommandGetsNoArgCompletion()
        {
            var completions = Loop<TestCommand>.GenerateCompletions(new[] { "Exit" }, 1).ToList();
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

            new Loop<OnlyExitableCommand>(parameters, null).Execute();
        }
    }
}
