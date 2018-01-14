using System;
using System.Collections.Generic;
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
    /// <typeparam name="TCommandType">Type defining the command.</typeparam>
    public class CommandGroup<TCommandType> : Command, IArgumentProvider, ICommandGroup, IArgumentSetWithHelp
        where TCommandType : struct
    {
        private object _parentObject;
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
        /// <param name="parentObject">Optionally provides a reference to the
        /// object containing this command group.</param>
        public CommandGroup(TCommandType selection, object parentObject) : this()
        {
            _parentObject = parentObject;
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
        /// Indicates if help information is desired.
        /// </summary>
        // TODO: Figure out how to uncomment the following line.
        // [NamedArgument(ArgumentFlags.Optional, Description = "Display usage information for command")]
        public bool Help { get; set; }

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

            var constructorArgs = new List<object> { selection };
            if (_parentObject != null)
            {
                constructorArgs.Add(_parentObject);
            }

            Func<ICommand> constructorFunc;

            try
            {
                constructorFunc = implementingType.GetConstructor<ICommand>(constructorArgs,
                    /*considerParameterlessConstructor=*/true);
            }
            catch (NotSupportedException ex)
            {
                throw new InvalidCommandException("No valid command constructor could be found.", ex);
            }

            return constructorFunc();
        }
    }
}
