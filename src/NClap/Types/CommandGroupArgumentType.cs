using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NClap.Exceptions;
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

        /// <summary>
        /// Constructs a new object to describe the provided command group type.
        /// </summary>
        /// <param name="type">Type to describe.</param>
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

            if (!type.GetGenericTypeDefinition().IsEffectivelySameAs(typeof(CommandGroup<>)) &&
                type.Name != "CommandGroup`1")
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            _commandTypeType = typeParams[0];
            if (!_commandTypeType.GetTypeInfo().IsEnum)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            _commandArgType = EnumArgumentType.Create(_commandTypeType);
        }

        /// <summary>
        /// Generates a set of valid strings--parseable to this type--that
        /// contain the provided string as a strict prefix.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="valueToComplete">The string to complete.</param>
        /// <returns>An enumeration of a set of completion strings; if no such
        /// strings could be generated, or if the type doesn't support
        /// completion, then an empty enumeration is returned.</returns>
        public override IEnumerable<string> GetCompletions(ArgumentCompletionContext context, string valueToComplete) =>
            _commandArgType.GetCompletions(context, valueToComplete);

        /// <summary>
        /// Converts a value into a readable string form.
        /// </summary>
        /// <param name="value">The value to format into a string.</param>
        /// <returns>The formatted string.</returns>
        public override string Format(object value)
        {
            var group = (ICommandGroup)value;
            if (!group.HasSelection)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return _commandArgType.Format(group.Selection);
        }

        /// <summary>
        /// Tries to parse the provided string, extracting a value of the type
        /// described by this interface.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="stringToParse">The string to parse.</param>
        /// <returns>True on success; false otherwise.</returns>
        protected override object Parse(ArgumentParseContext context, string stringToParse)
        {
            if (!_commandArgType.TryParse(context, stringToParse, out object selection))
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            var groupOptions = new CommandGroupOptions
            {
                ServiceConfigurer = context.ServiceConfigurer
            };

            var constructorArgTypes = new[] { typeof(CommandGroupOptions), typeof(object), typeof(object) };
            var commandGroupConstructor = Type.GetTypeInfo().GetConstructor(constructorArgTypes);

            if (commandGroupConstructor == null)
            {
                throw new InternalInvariantBrokenException($"Constructor not found in {Type.FullName}: ({string.Join(", ", constructorArgTypes.Select(ty => ty.FullName))})");
            }

            try
            {
                return commandGroupConstructor.Invoke(new[] { groupOptions, selection, context.ContainingObject });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// The type's human-readable (display) name.
        /// </summary>
        public override string DisplayName => _commandArgType.DisplayName;

        /// <summary>
        /// Enumeration of all types that this type depends on / includes.
        /// </summary>
        public override IEnumerable<IArgumentType> DependentTypes => new[] { _commandArgType };
    }
}
