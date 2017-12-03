using System;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Types;

namespace NClap.Tests.Types
{
    [TestClass]
    public class ArgumentParseContextTests
    {
        [TestMethod]
        public void DefaultedReader()
        {
            var context = new ArgumentParseContext();

            Action setNull = () => context.FileSystemReader = null;
            setNull.Should().NotThrow<ArgumentNullException>();

            context.FileSystemReader.Should().NotBeNull();
        }
    }
}
