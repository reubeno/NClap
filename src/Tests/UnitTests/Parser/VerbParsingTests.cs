using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;

namespace NClap.Tests.Parser
{
    [TestClass]
    public class CommandParsingTests
    {
        class SomethingCommand : SynchronousCommand
        {
            public bool ThatSomething { get; set; }

            public override CommandResult Execute() => CommandResult.Success;
        }

        class OtherThingCommand : SynchronousCommand
        {
            public bool ThatOtherThing { get; set; }

            public override CommandResult Execute() => CommandResult.Success;
        }

        enum SimpleCommandType
        {
            [Command(typeof(SomethingCommand))] Something,
            [Command(typeof(OtherThingCommand))] OtherThing
        }

        class CommandArgument<TCommandType>
        {
            public TCommandType CommandType { get; set; }

            public T Get<T>(TCommandType commandType)
            {
                return (T)CommandArguments[commandType];
            }

            public IReadOnlyDictionary<TCommandType, object> CommandArguments { get; set; }
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class SimpleArguments
        {
            [NamedArgument]
            public bool GlobalOption { get; set; }

            [PositionalArgument]
            public SimpleCommandType Command { get; set; }
        }

        [TestMethod]
        public void SimpleCommandUsage()
        {
            CommandLineParser.TryParse(new[] { "/GlobalOption", "OtherThing" }, out SimpleArguments args)
                .Should().BeTrue();
        }
    }
}
