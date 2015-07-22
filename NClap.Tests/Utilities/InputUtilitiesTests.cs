using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Utilities;

namespace NClap.Tests.Utilities
{
    [TestClass]
    public class InputUtilitiesTests
    {
        [TestMethod]
        public void SimpleTest()
        {
            InputUtilities.TryGetSingleChar(ConsoleKey.A, (ConsoleModifiers)0).Should().Be('a');
            InputUtilities.TryGetSingleChar(ConsoleKey.A, ConsoleModifiers.Shift).Should().Be('A');
        }
    }
}
