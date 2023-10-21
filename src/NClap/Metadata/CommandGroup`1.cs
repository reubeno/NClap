using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NClap.Exceptions;
using NClap.Parser;
using NClap.Types;
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
        private readonly CommandGroupOptions _options;
        private readonly object _parentObject;
        private object _selectedCommand;

        /// <summary>
        /// Parameterless constructor. Deprecated.
        /// </summary>
        [Obsolete("Deprecated in favor of a constructor that takes options.")]
        public CommandGroup()
        {
            if (!typeof(TCommandType).GetTypeInfo().IsEnum)
            {
                throw new NotSupportedException();
            }

            _options = new CommandGroupOptions();
        }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="options">Options.</param>
        public CommandGroup(CommandGroupOptions options)
        {
            if (!typeof(TCommandType).GetTypeInfo().IsEnum)
            {
                throw new NotSupportedException();
            }

            _options = options?.DeepClone() ?? new CommandGroupOptions();
        }

        /// <summary>
        /// Basic constructor. No longer implemented.
        /// </summary>
        /// <param name="selection">The selected command type.</param>
        /// <param name="parentObject">Optionally provides a reference to the
        /// object containing this command group.</param>
        [Obsolete("Please use new overload with options.")]
        public CommandGroup(TCommandType selection, object parentObject)
        {
            throw new NotImplementedException("This method is no longer implemented.");
        }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="options">Command group options.</param>
        /// <param name="selection">The selected command type.</param>
        /// <param name="parentObject">Optionally provides a reference to the
        /// object containing this command group.</param>
        public CommandGroup(CommandGroupOptions options, object selection, object parentObject) : this(options)
        {
            _parentObject = parentObject;
            SelectedCommand = selection;
        }

        /// <summary>
        /// True if the group has a selection, false if no selection was yet
        /// made.
        /// </summary>
        public bool HasSelection => SelectedCommand != null;

        /// <summary>
        /// The enum value corresponding with the selected command, or null if no
        /// selection has yet been made.
        /// </summary>
        [PositionalArgument(ArgumentFlags.Required, Position = 0, LongName = nameof(Command))]
        public TCommandType? Selection
        {
            get => (TCommandType?)SelectedCommand;
            set => SelectedCommand = value.HasValue ? (object)value.Value : null;
        }

        /// <summary>
        /// The enum value corresponding with the selected command, or null if no
        /// selection has yet been made.
        /// </summary>
        object ICommandGroup.Selection => SelectedCommand;

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

        private object SelectedCommand
        {
            get => _selectedCommand;
            set
            {
                _selectedCommand = value;
                InstantiatedCommand = value != null ? InstantiateCommand(value) : null;
            }
        }

        private ICommand InstantiateCommand(object selection)
        {
            var commandTypeType = EnumArgumentType.Create(typeof(TCommandType));

            return InstantiateCommand(commandTypeType, selection);
        }

        private ICommand InstantiateCommand(IEnumArgumentType commandTypeType, object selection)
        {
            if (!commandTypeType.TryGetValue(selection, out IArgumentValue selectionValue))
            {
                throw new InternalInvariantBrokenException();
            }

            var enumValue = (EnumArgumentValue)selectionValue;
            var selectionField = enumValue.ValueInfo;

            var commandAttrib = selectionField.GetSingleAttribute<CommandAttribute>();
            if (commandAttrib == null)
            {
                throw new InvalidCommandException($"No CommandAttribute was found on field '{selectionField.Name}' of type '{commandTypeType.DisplayName}'");
            }

            var implementingType = commandAttrib.GetImplementingType(typeof(TCommandType));
            if (implementingType == null)
            {
                throw new InvalidCommandException($"No implementing type found for command '{commandAttrib.LongName}' in type '{commandTypeType.DisplayName}'");
            }

            var commandDef = new CommandDefinition(selection, implementingType);

            return commandDef.Instantiate(services =>
            {
                _options.ServiceConfigurer?.Invoke(services);

                if (_parentObject != null)
                {
                    services.AddSingleton(_parentObject.GetType(), _parentObject);
                }
            });
        }
    }
}
