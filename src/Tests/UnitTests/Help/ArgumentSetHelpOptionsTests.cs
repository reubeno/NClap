using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Help;
using NClap.Tests.Utilities;

namespace NClap.Tests.Help
{
    [TestClass]
    public class ArgumentSetHelpOptionsTests
    {
        [TestMethod]
        public void TestThatDeepCloningDefaultOptionsWorksAsExpected()
        {
            var options = new ArgumentSetHelpOptions();
            DeepCloneTests.CloneShouldYieldADistinctButEquivalentObject(options);
        }
    }
}
