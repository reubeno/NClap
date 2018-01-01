using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Metadata;
using NClap.Types;

namespace NClap.Tests.Types
{
    [TestClass]
    public class CommandGroupArgumentTypeTests
    {
        class FooCommand : SynchronousCommand
        {
            public override CommandResult Execute()
            {
                ++CallCount;
                return CommandResult.Success;
            }
            public int CallCount { get; private set; }
        }

        enum SimpleCommandType
        {
            [Command(typeof(FooCommand))]
            Foo,

            [Command(typeof(CommandGroup<NestedCommandType>))]
            Nested,
        }

        enum NestedCommandType
        {
            [Command(typeof(NestedCommand))]
            Bar,

            [Command(typeof(NestedCommand))]
            Baz
        }

        class NestedCommand : SynchronousCommand
        {
            [NamedArgument]
            int Bar { get; set; }

            public override CommandResult Execute() => CommandResult.Success;
        }

        class SimpleCommandArgs
        {
            [NamedArgument]
            int SharedValue { get; set; }

            [PositionalArgument(ArgumentFlags.Required)]
            CommandGroup<SimpleCommandType> Command { get; set; }
        }

        [TestMethod]
        public void ConstructorThrowsOnNonGenericType()
        {
            Action a = () => { var x = new CommandGroupArgumentType(typeof(int)); };
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void ConstructorThrowsOnDifferentArityGenericType()
        {
            Action a = () => { var x = new CommandGroupArgumentType(typeof(Tuple<int, int>)); };
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void ConstructorThrowsOnNonCommandGroupType()
        {
            Action a = () => { var x = new CommandGroupArgumentType(typeof(List<int>)); };
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void ConstructorThrowsWhenInnerTypeIsNotEnum()
        {
            Action a = () => { var x = new CommandGroupArgumentType(typeof(CommandGroup<Guid>)); };
            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void SimpleCompletionWorks()
        {
            var argType = new CommandGroupArgumentType(typeof(CommandGroup<SimpleCommandType>));
            argType.GetCompletions(new ArgumentCompletionContext(), "N")
                .Should().ContainSingle()
                .And.Equal(nameof(SimpleCommandType.Nested));
        }

        [TestMethod]
        public void FormattingThrowsOnNonCommandGroup()
        {
            var argType = new CommandGroupArgumentType(typeof(CommandGroup<SimpleCommandType>));
            Action a = () => argType.Format(3);
            a.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void FormattingThrowsOnNoSelection()
        {
            var argType = new CommandGroupArgumentType(typeof(CommandGroup<SimpleCommandType>));

            var group = new CommandGroup<SimpleCommandType>();
            Action a = () => argType.Format(group);

            a.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void FormattingWorksOnSelection()
        {
            var argType = new CommandGroupArgumentType(typeof(CommandGroup<SimpleCommandType>));
            var group = new CommandGroup<SimpleCommandType>
            {
                Selection = SimpleCommandType.Nested
            };

            argType.Format(group).Should().Be(nameof(SimpleCommandType.Nested));
        }

        [TestMethod]
        public void DisplayNameIsCorrect()
        {
            var argType = new CommandGroupArgumentType(typeof(CommandGroup<SimpleCommandType>));
            argType.DisplayName.Should().Be(nameof(SimpleCommandType));
        }

        [TestMethod]
        public void ParsingThrowsOnInvalidCommand()
        {
            var argType = new CommandGroupArgumentType(typeof(CommandGroup<SimpleCommandType>));
            argType.TryParse(ArgumentParseContext.Default, "NotACommand", out object result).Should().BeFalse();
        }

        [TestMethod]
        public void ParsingTopLevelLeafCommandSucceeds()
        {
            var argType = new CommandGroupArgumentType(typeof(CommandGroup<SimpleCommandType>));
            argType.TryParse(ArgumentParseContext.Default, "Foo", out object result).Should().BeTrue();
            var group = result.Should()
                .BeOfType<CommandGroup<SimpleCommandType>>()
                .Which;

            group.HasSelection.Should().BeTrue();
            group.Selection.Should().Be(SimpleCommandType.Foo);
            group.Execute().Should().Be(CommandResult.Success);
            group.InstantiatedCommand
                .Should().NotBeNull()
                .And.BeOfType<FooCommand>()
                .Which.CallCount.Should().Be(1);
        }

        [TestMethod]
        public void ParsingNestedGroupCommandSucceeds()
        {
            var argType = new CommandGroupArgumentType(typeof(CommandGroup<SimpleCommandType>));
            argType.TryParse(ArgumentParseContext.Default, "Nested", out object result).Should().BeTrue();
            var group = result.Should()
                .BeOfType<CommandGroup<SimpleCommandType>>()
                .Which;

            group.HasSelection.Should().BeTrue();
            group.Selection.Should().Be(SimpleCommandType.Nested);

            var nestedGroup = group.InstantiatedCommand
                .Should().NotBeNull()
                .And.BeAssignableTo<ICommandGroup>()
                .Which;

           nestedGroup.HasSelection.Should().BeFalse();
            nestedGroup.ExecuteAsync(CancellationToken.None).Result.Should().Be(CommandResult.UsageError);
        }
    }
}
