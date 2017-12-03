using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.ConsoleInput;
using NClap.Repl;
using NClap.Utilities;
using NSubstitute;

namespace NClap.Tests.Repl
{
    /// <summary>
    /// Tests for <see cref="ConsoleLoopClient" />.
    /// </summary>
    [TestClass]
    public class ConsoleLoopClientTests
    {
        [TestMethod]
        public void ConstructorThrowsOnNull()
        {
            Action constructAction = () => new ConsoleLoopClient(null);
            constructAction.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor()
        {
            var reader = Substitute.For<IConsoleReader>();
            var client = new ConsoleLoopClient(reader);

            client.Reader.Should().BeSameAs(reader);
        }

        [TestMethod]
        public void ErrorsAreIgnoredWithNoErrorWriter()
        {
            var reader = Substitute.For<IConsoleReader>();
            var client = new ConsoleLoopClient(reader);

            client.OnError("Hello");
        }

        [TestMethod]
        public void ErrorWriterReceivesError()
        {
            const string errorText = "Hello, errors!";

            var reader = Substitute.For<IConsoleReader>();
            var output = Substitute.For<IConsoleOutput>();

            reader.ConsoleOutput.Returns(output);

            var client = new ConsoleLoopClient(reader);
            client.OnError(errorText);

            output.Received().Write(
                Arg.Is<ColoredString>(cs => cs.Content.Equals(errorText + Environment.NewLine)));
        }

        [TestMethod]
        public void Prompts()
        {
            const string prompt = "[Prompt!] ";

            var reader = Substitute.For<IConsoleReader>();
            var lineInput = Substitute.For<IConsoleLineInput>();

            lineInput.Prompt = prompt;
            reader.LineInput.Returns(lineInput);

            var client = new ConsoleLoopClient(reader);
            client.Prompt.Should().Be(prompt);

            const string newPrompt = "NewPrompt";
            client.Prompt = newPrompt;
            client.Prompt.Should().Be(newPrompt);
            lineInput.Prompt.ToString().Should().Be(newPrompt);

            client.DisplayPrompt();
            lineInput.Received(1).DisplayPrompt();
        }

        [TestMethod]
        public void ReadLine()
        {
            const string lineText = "The line that was read.";

            var reader = Substitute.For<IConsoleReader>();
            var lineInput = Substitute.For<IConsoleLineInput>();

            reader.LineInput.Returns(lineInput);
            reader.ReadLine().Returns(lineText);

            var client = new ConsoleLoopClient(reader);
            client.ReadLine().Should().Be(lineText);
            reader.Received(1).ReadLine();
        }
    }
}
