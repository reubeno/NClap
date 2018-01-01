using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using System.Linq;

namespace NClap.Tests.Parser
{
    [TestClass]
    public class CommandParsingTests
    {
        class SomethingCommand : SynchronousCommand
        {
            [NamedArgument]
            public bool ThatSomething { get; set; }

            public override CommandResult Execute() => CommandResult.Success;
        }

        class OtherThingCommand : SynchronousCommand
        {
            [NamedArgument]
            public bool ThatOtherThing { get; set; }

            public override CommandResult Execute() => CommandResult.Success;
        }

        enum SimpleCommandType
        {
            [Command(typeof(SomethingCommand))] Something,
            [Command(typeof(OtherThingCommand))] OtherThing
        }

        [ArgumentSet(Style = ArgumentSetStyle.WindowsCommandLine)]
        class SimpleArguments
        {
            [NamedArgument]
            public bool GlobalOption { get; set; }

            [PositionalArgument]
            public CommandGroup<SimpleCommandType> Command { get; set; }
        }

        [TestMethod]
        public void CommandSpecificArgsParseOkay()
        {
            CommandLineParser.TryParse(new[] { "/GlobalOption", "OtherThing", "/ThatOtherThing" }, out SimpleArguments args)
                .Should().BeTrue();

            args.Command.HasSelection.Should().BeTrue();
            args.Command.Selection.HasValue.Should().BeTrue();
            args.Command.Selection.Value.Should().Be(SimpleCommandType.OtherThing);

            var cmd = args.Command.InstantiatedCommand;
            cmd.Should().NotBeNull();

            var otherCmd = cmd.Should().BeOfType<OtherThingCommand>().Which;
            otherCmd.ThatOtherThing.Should().BeTrue();
        }

        [TestMethod]
        public void CommandSpecificArgsFormatOkay()
        {
            var stringArgs = new[] { "/GlobalOption", "OtherThing", "/ThatOtherThing" };

            CommandLineParser.TryParse(stringArgs, out SimpleArguments args)
                .Should().BeTrue();

            var formattedArgs = CommandLineParser.Format(args).ToList();

            formattedArgs.Should().Equal(
                "/GlobalOption=True", "OtherThing", "/ThatOtherThing=True");
        }
    }
}
