using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class CommandTests
    {
        private class TestCommand : Command
        {
        }

        [TestMethod]
        public void TestThatBaseCommandImplementationReturnsUsageError()
        {
            var command = new TestCommand();
            command.ExecuteAsync(CancellationToken.None).Result.Should().Be(CommandResult.UsageError);
        }
    }
}
