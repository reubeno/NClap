using System;
using System.Collections.Generic;
using NClap.Types;

namespace NClap.Parser
{
    /// <summary>
    /// Describes a command group.
    /// </summary>
    internal class CommandGroupDefinition
    {
        private readonly Dictionary<object, CommandDefinition> _commandsByKey = new Dictionary<object, CommandDefinition>();

        /// <summary>
        /// Constructs a new command group definition.
        /// </summary>
        /// <param name="argType">Argument type associated with this command group definition.</param>
        internal CommandGroupDefinition(IArgumentType argType)
        {
            ArgumentType = argType;
        }

        /// <summary>
        /// Argument type associated with this command group definition.
        /// </summary>
        internal IArgumentType ArgumentType { get; }

        /// <summary>
        /// Enumerates all commands defined in this group.
        /// </summary>
        internal IEnumerable<CommandDefinition> Commands => _commandsByKey.Values;

        /// <summary>
        /// Add a new command to this group.
        /// </summary>
        /// <param name="command">Command to add.</param>
        internal void Add(CommandDefinition command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (_commandsByKey.ContainsKey(command.Key))
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }

            _commandsByKey.Add(command.Key, command);
        }

        /// <summary>
        /// Tries to retrieve the definition of the command associated with the
        /// given key.
        /// </summary>
        /// <param name="key">Key to look up.</param>
        /// <param name="command">On success, receives the matching command definition.</param>
        /// <returns>true on success; false otherwise.</returns>
        internal bool TryGetCommand(object key, out CommandDefinition command) =>
            _commandsByKey.TryGetValue(key, out command);
    }
}
