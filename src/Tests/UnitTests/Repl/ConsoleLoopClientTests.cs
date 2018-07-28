﻿using System;
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
        public void TestThatConstructorThrowsOnNull()
        {
            Action constructAction = () => new ConsoleLoopClient(null);
            constructAction.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TestThatConstructorGeneratesPlausibleClient()
        {
            var reader = Substitute.For<IConsoleReader>();
            var client = new ConsoleLoopClient(reader);

            client.Reader.Should().BeSameAs(reader);
        }

        [TestMethod]
        public void TestThatStoredTokenCompleterIsRetrievable()
        {
            var reader = Substitute.For<IConsoleReader>();
            var completer = Substitute.For<ITokenCompleter>();

            var client = new ConsoleLoopClient(reader);
            client.TokenCompleter = completer;

            client.TokenCompleter.Should().BeSameAs(completer);
        }

        [TestMethod]
        public void TestThatErrorsAreIgnoredWithNoErrorWriter()
        {
            var reader = Substitute.For<IConsoleReader>();
            var client = new ConsoleLoopClient(reader);

            client.OnError("Hello");
        }

        [TestMethod]
        public void TestThatErrorWriterReceivesError()
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
        public void TestThatPromptsAreObserved()
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
        public void TestThatColoredPromptsAreObserved()
        {
            var prompt = new ColoredString("[Prompt!] ", ConsoleColor.Cyan);

            var reader = Substitute.For<IConsoleReader>();
            var lineInput = Substitute.For<IConsoleLineInput>();

            lineInput.Prompt = prompt;
            reader.LineInput.Returns(lineInput);

            var client = new ConsoleLoopClient(reader);
            client.Prompt.Should().Be(prompt);

            var newPrompt = new ColoredString("NewPrompt", ConsoleColor.Green);
            client.PromptWithColor = newPrompt;
            client.PromptWithColor.Should().Be(newPrompt);
            client.Prompt.Should().Be(newPrompt.ToString());
            lineInput.Prompt.Should().Be(newPrompt);

            client.DisplayPrompt();
            lineInput.Received(1).DisplayPrompt();
        }

        [TestMethod]
        public void TestThatReadLineWorksAsExpected()
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
