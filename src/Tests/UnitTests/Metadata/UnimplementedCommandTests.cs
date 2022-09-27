using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using System;
using System.Threading;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class UnimplementedCommandTests
    {
        [TestMethod]
        public void TestThatUnimplementedCommandAlwaysReturnsCorrectCode()
        {
            var command = new UnimplementedCommand();
            command.Awaiting(c => c.ExecuteAsync(CancellationToken.None)).Should().ThrowAsync<NotImplementedException>();
            command.Invoking(c => c.Execute()).Should().Throw<NotImplementedException>();
        }
    }
}
