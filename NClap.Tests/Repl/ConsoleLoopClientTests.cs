using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.ConsoleInput;
using NClap.Repl;
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
            constructAction.ShouldThrow<ArgumentNullException>();
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

            var sb = new StringBuilder();
            using (var errorWriter = new StringWriter(sb))
            {
                var client = new ConsoleLoopClient(reader, errorWriter);
                client.OnError(errorText);
            }

            sb.ToString().Should().Be(errorText + Environment.NewLine);
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
