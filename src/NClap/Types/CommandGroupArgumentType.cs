using System;
using System.Collections.Generic;
using System.Reflection;
using NClap.Metadata;
using NClap.Utilities;

namespace NClap.Types
{
    /// <summary>
    /// Implementation to describe command groups.
    /// </summary>
    internal class CommandGroupArgumentType : ArgumentTypeBase
    {
        private readonly Type _commandTypeType;
        private readonly IEnumArgumentType _commandArgType;

        public CommandGroupArgumentType(Type type) : base(type)
        {
            if (!type.GetTypeInfo().IsGenericType)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            var typeParams = type.GetTypeInfo().GetGenericArguments();
            if (typeParams.Length != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            if (!type.GetGenericTypeDefinition().IsEffectivelySameAs(typeof(CommandGroup<>)))
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            _commandTypeType = typeParams[0];
            if (!_commandTypeType.GetTypeInfo().IsEnum)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            if (!ArgumentType.TryGetType(_commandTypeType, out IArgumentType commandArgType))
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            _commandArgType = (IEnumArgumentType)commandArgType;
        }

        public override IEnumerable<string> GetCompletions(ArgumentCompletionContext context, string valueToComplete) =>
            _commandArgType.GetCompletions(context, valueToComplete);

        public override string Format(object value)
        {
            var group = (ICommandGroup)value;
            if (!group.HasSelection)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return _commandArgType.Format(group.Selection);
        }

        protected override object Parse(ArgumentParseContext context, string stringToParse)
        {
            if (!_commandArgType.TryParse(context, stringToParse, out object selection))
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            var commandGroupConstructor = Type.GetTypeInfo().GetConstructor(new[] { _commandTypeType });
            if (commandGroupConstructor == null)
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            var group = commandGroupConstructor.Invoke(new[] { selection });
            if (group == null)
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            return group;
        }

        public override string DisplayName => _commandArgType.DisplayName;

        public override IEnumerable<IArgumentType> DependentTypes => new[] { _commandArgType };
    }
}
