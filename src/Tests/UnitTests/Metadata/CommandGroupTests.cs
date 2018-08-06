using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Exceptions;
using NClap.Metadata;

namespace NClap.Tests.Metadata
{
    [TestClass]
    public class CommandGroupTests
    {
        struct NonEnumValueType
        {
        }

        enum CommandType
        {
            NotACommandBecauseNoAttribute,

            [Command(null)]
            NotACommandBecauseNoImplementingType,

            [Command(typeof(NoParameterlessConstructor))]
            InvalidCommandBecauseTypeHasNoParameterlessConstructor,

            [Command(typeof(NotACommandType))]
            InvalidCommandBecauseTypeIsNotICommand,

            [Command(typeof(LegitCommand))]
            ValidCommand
        }

        class NotACommandType
        {
        }

        class NoParameterlessConstructor
        {
            public NoParameterlessConstructor(int foo)
            {
            }
        }

        class LegitCommand : SynchronousCommand
        {
            public override CommandResult Execute() => CommandResult.Success;
        }

        [TestMethod]
        public void CannotConstructWithNonEnumCommandType()
        {
            Action a = () => { var x = new CommandGroup<NonEnumValueType>(null); };
            a.Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void CanConstructWithNonEnumCommandType()
        {
            Action a = () => { var group = new CommandGroup<CommandType>(null); };
            a.Should().NotThrow<Exception>();
        }

        [TestMethod]
        public void CannotSelectEnumValueWithoutCommandAttribute()
        {
            var group = new CommandGroup<CommandType>(null);
            Action a = () => { group.Selection = CommandType.NotACommandBecauseNoAttribute; };
            a.Should().Throw<InvalidCommandException>();
        }

        [TestMethod]
        public void CannotSelectCommandWithoutImplementingType()
        {
            var group = new CommandGroup<CommandType>(null);
            Action a = () => { group.Selection = CommandType.NotACommandBecauseNoImplementingType; };
            a.Should().Throw<InvalidCommandException>();
        }

        [TestMethod]
        public void CannotSelectCommandWithoutParameterlessConstructor()
        {
            var group = new CommandGroup<CommandType>(null);
            Action a = () => { group.Selection = CommandType.InvalidCommandBecauseTypeHasNoParameterlessConstructor; };
            a.Should().Throw<InvalidCommandException>();
        }

        [TestMethod]
        public void CannotSelectCommandWithoutICommandImplementingType()
        {
            var group = new CommandGroup<CommandType>(null);
            Action a = () => { group.Selection = CommandType.InvalidCommandBecauseTypeIsNotICommand; };
            a.Should().Throw<InvalidCommandException>();
        }
    }
}
