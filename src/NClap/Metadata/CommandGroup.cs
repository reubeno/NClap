using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NClap.Exceptions;
using NClap.Utilities;

namespace NClap.Metadata
{
    /// <summary>
    /// Represents a group of commands, i.e. a command with sub-commands.
    /// </summary>
    public class CommandGroup<TCommandType> : Command, IArgumentProvider, ICommandGroup where TCommandType : struct
    {
        private TCommandType? _selectedCommandType;

        /// <summary>
        /// Default, parameterless constructor.
        /// </summary>
        public CommandGroup()
        {
            if (!typeof(TCommandType).GetTypeInfo().IsEnum)
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="selection">The selected command type.</param>
        public CommandGroup(TCommandType selection) : this()
        {
            Selection = selection;
        }

        /// <summary>
        /// True if the group has a selection, false if no selection was yet
        /// made.
        /// </summary>
        public bool HasSelection => Selection.HasValue;

        /// <summary>
        /// The enum value corresponding with the selected command, or null if no
        /// selection has yet been made.
        /// </summary>
        [PositionalArgument(ArgumentFlags.Required, Position = 0, LongName = nameof(Command))]
        public TCommandType? Selection
        {
            get => _selectedCommandType;
            set
            {
                _selectedCommandType = value;
                InstantiatedCommand = value.HasValue ? InstantiateCommand(value.Value) : null;
            }
        }

        /// <summary>
        /// The enum value corresponding with the selected command, or null if no
        /// selection has yet been made.
        /// </summary>
        object ICommandGroup.Selection => Selection;

        /// <summary>
        /// The command presently selected from this group, or null if no
        /// selection has yet been made.
        /// </summary>
        public ICommand InstantiatedCommand { get; private set; }

        /// <summary>
        /// Retrieve info for the object type that defines the arguments to be
        /// parsed.
        /// </summary>
        /// <returns>The defining type.</returns>
        public Type GetTypeDefiningArguments() => InstantiatedCommand?.GetType();

        /// <summary>
        /// Retrieve a reference to the object into which parsed arguments
        /// should be stored.
        /// </summary>
        /// <returns>The object in question.</returns>
        public object GetDestinationObject() => InstantiatedCommand;
        
        /// <summary>
        /// Executes the command synchronously.
        /// </summary>
        /// <returns>Result of execution.</returns>
        public CommandResult Execute() => ExecuteAsync(CancellationToken.None).Result;

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Result of execution.</returns>
        public override Task<CommandResult> ExecuteAsync(CancellationToken cancel)
        {
            if (InstantiatedCommand == null)
            {
                return Task.FromResult(CommandResult.UsageError);
            }

            return InstantiatedCommand.ExecuteAsync(cancel);
        }

        private ICommand InstantiateCommand(TCommandType selection)
        {
            var commandTypeType = typeof(TCommandType);

            var selectionName = commandTypeType.GetTypeInfo().GetEnumName(selection);
            var selectionField = commandTypeType.GetTypeInfo().GetField(selectionName);

            var commandAttrib = selectionField.GetSingleAttribute<CommandAttribute>();
            if (commandAttrib == null)
            {
                throw new InvalidCommandException(commandTypeType, selectionField, $"No CommandAttribute was found on field '{selectionField.Name}' of type '{commandTypeType.FullName}'");
            }

            var implementingType = commandAttrib.GetImplementingType(commandTypeType);
            if (implementingType == null)
            {
                throw new InvalidCommandException(commandTypeType, selectionField, $"No implementing type found for command '{commandAttrib.LongName ?? selectionName}' in type '{commandTypeType.FullName}'");
            }

            // Look for a constructor that takes the selected command. If we don't find that, then
            // instead look for a parameterless constructor.
            ConstructorInfo constructor;
            object[] constructorArgs;
            if ((constructor = implementingType.GetTypeInfo().GetConstructor(new[] { commandTypeType })) != null)
            {
                constructorArgs = new object[] { selection };
            }
            else if ((constructor = implementingType.GetTypeInfo().GetConstructor(Array.Empty<Type>())) != null)
            {
                constructorArgs = Array.Empty<object>();
            }
            else
            {
                throw new InvalidCommandException(commandTypeType, selectionField, $"Command implementation type '{implementingType.FullName}' does not contain compatible constructor");
            }

            var command = constructor.Invoke(constructorArgs) as ICommand;
            if (command == null)
            {
                throw new InvalidCommandException(commandTypeType, selectionField, $"Failed to instantiate command");
            }

            command.Parent = this;

            return command;
        }
    }
}
