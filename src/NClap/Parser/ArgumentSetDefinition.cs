using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NClap.Exceptions;
using NClap.Metadata;
using NClap.Utilities;

namespace NClap.Parser
{
    /// <summary>
    /// Encapsulates an argument set.
    /// </summary>
    internal class ArgumentSetDefinition
    {
        // Argument metadata and parse state.
        private readonly Dictionary<IMutableMemberInfo, ArgumentDefinition> _argumentsByMember = new Dictionary<IMutableMemberInfo, ArgumentDefinition>();
        private readonly List<ArgumentDefinition> _namedArguments = new List<ArgumentDefinition>();
        private readonly Dictionary<string, ArgumentDefinition> _namedArgumentsByName;
        private readonly SortedList<int, ArgumentDefinition> _positionalArguments = new SortedList<int, ArgumentDefinition>();

        // Options.
        private readonly CommandLineParserOptions _options;

        // State.
        private int _nextPositionalArgIndexToDefine;

        /// <summary>
        /// Constructs an argument set from a type with reflection-based argument attributes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="defaultValues">Optionally provides an object with default values.</param>
        /// <param name="options">Optionally provides additional options
        /// controlling how parsing proceeds.</param>
        public ArgumentSetDefinition(Type type, object defaultValues = null, CommandLineParserOptions options = null) :
            // Try to grab attributes from type.
            this(options, type.GetTypeInfo().GetSingleAttribute<ArgumentSetAttribute>())
        {
            // Scan the provided type for argument definitions.
            AddArgumentsFromTypeWithAttributes(type, defaultValues);
        }

        /// <summary>
        /// Constructs an empty argument set.
        /// </summary>
        /// <param name="options">Optionally provides additional options
        /// controlling how parsing proceeds.</param>
        /// <param name="setAttribute">Optionally provides attribute information
        /// describing the attribute set.</param>
        public ArgumentSetDefinition(CommandLineParserOptions options = null, ArgumentSetAttribute setAttribute = null)
        {
            // Save off the options provided; if none were provided, construct some defaults.
            _options = options?.Clone() ?? new CommandLineParserOptions();

            // Save off set attributes; if none were provided, construct some defaults.
            Attribute = setAttribute ?? new ArgumentSetAttribute();

            // Set up private fields dependent on the ArgumentSetAttribute.
            _namedArgumentsByName = new Dictionary<string, ArgumentDefinition>(StringComparerToUse);
        }

        /// <summary>
        /// Options for the set.
        /// </summary>
        public ArgumentSetAttribute Attribute { get; }

        /// <summary>
        /// The named arguments in this set.
        /// </summary>
        public IEnumerable<ArgumentDefinition> NamedArguments => _namedArguments;

        /// <summary>
        /// The positional arguments in this set, in index order.
        /// </summary>
        public IEnumerable<ArgumentDefinition> PositionalArguments => _positionalArguments.Values;

        /// <summary>
        /// Enumerates all valid long and short names of named arguments.
        /// </summary>
        public IEnumerable<string> ArgumentNames => _namedArgumentsByName.Keys;

        /// <summary>
        /// Try to look up a named argument by name (short name or long name).
        /// </summary>
        /// <param name="name">Name to look up.</param>
        /// <param name="arg">On success, receives the named argument.</param>
        /// <returns>True on success; false otherwise.</returns>
        public bool TryGetNamedArgument(string name, out ArgumentDefinition arg) => _namedArgumentsByName.TryGetValue(name, out arg);

        /// <summary>
        /// Try to look up a positional argument by position.
        /// </summary>
        /// <param name="position">0-based position index to look up.</param>
        /// <param name="arg">On success, receives the argument.</param>
        /// <returns>True on success; false otherwise.</returns>
        public bool TryGetPositionalArgument(int position, out ArgumentDefinition arg) => _positionalArguments.TryGetValue(position, out arg);

        /// <summary>
        /// Defines additional argument definitions from the reflection-based
        /// attributes stored on the provided type.
        /// </summary>
        /// <param name="defininingType">The type to inspect.</param>
        /// <param name="defaultValues">Optionally provides an object indicating
        /// default values.</param>
        /// <param name="fixedDestination">Optionally provides a fixed object
        /// to store values to.</param>
        public void AddArgumentsFromTypeWithAttributes(Type defininingType, object defaultValues = null, object fixedDestination = null)
        {
            // Extract argument descriptors from the defining type.
            var args = GetArgumentDescriptors(defininingType, Attribute, defaultValues, _options, fixedDestination).ToList();

            // Define the arguments.
            Add(args);
        }

        /// <summary>
        /// Adds an argument.
        /// </summary>
        /// <param name="arg">Argument to define.</param>
        public void AddArgument(ArgumentDefinition arg) => Add(new[] { arg });

        /// <summary>
        /// Adds arguments.
        /// </summary>
        /// <param name="args">Arguments to define.</param>
        public void Add(IEnumerable<ArgumentDefinition> args)
        {
            // Index the descriptors.
            foreach (var arg in args)
            {
                _argumentsByMember.Add(arg.Member, arg);
            }

            // Symmetrically reflect any conflicts.
            foreach (var arg in args)
            {
                foreach (var conflictingMemberName in arg.Attribute.ConflictsWith)
                {
                    var conflictingArgs =
                        args.Where(a => a.Member.MemberInfo.Name.Equals(conflictingMemberName, StringComparison.Ordinal))
                            .ToList();

                    if (conflictingArgs.Count != 1)
                    {
                        throw new InvalidArgumentSetException(arg, string.Format(
                            CultureInfo.CurrentCulture,
                            Strings.ConflictingMemberNotFound,
                            conflictingMemberName,
                            arg.Member.MemberInfo.Name));
                    }

                    // Add the conflict both ways -- if only one says it
                    // conflicts with the other, there's still a conflict.
                    arg.AddConflictingArgument(conflictingArgs[0]);
                    conflictingArgs[0].AddConflictingArgument(arg);
                }
            }

            // Add arguments.
            var positionalIndexBias = _nextPositionalArgIndexToDefine;
            foreach (var arg in args)
            {
                if (arg.Attribute is NamedArgumentAttribute)
                {
                    AddNamedArgument(arg);
                }
                else if (arg.Attribute is PositionalArgumentAttribute)
                {
                    AddPositionalArgument(arg, positionalIndexBias);
                }
            }

            // Re-validate positional arguments.
            ValidateThatPositionalArgumentsDoNotOverlap();
        }

        private void AddNamedArgument(ArgumentDefinition argument)
        {
            //
            // Validate and register the long name.
            //

            if (_namedArgumentsByName.ContainsKey(argument.LongName))
            {
                throw new InvalidArgumentSetException(argument, string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.DuplicateArgumentLongName,
                    argument.LongName));
            }

            _namedArgumentsByName.Add(argument.LongName, argument);

            //
            // Validate and register the short name.
            //

            if (!string.IsNullOrEmpty(argument.ShortName))
            {
                if (_namedArgumentsByName.TryGetValue(argument.ShortName, out ArgumentDefinition conflictingArg))
                {
                    Debug.Assert(conflictingArg != null);
                    if (argument.ExplicitShortName)
                    {
                        if (conflictingArg.ExplicitShortName)
                        {
                            throw new InvalidArgumentSetException(argument, string.Format(CultureInfo.CurrentCulture,
                                Strings.DuplicateArgumentShortName,
                                argument.ShortName));
                        }
                        else
                        {
                            // TODO: Decide whether this works for dynamically imported args.
                            _namedArgumentsByName.Remove(conflictingArg.ShortName);
                            conflictingArg.ClearShortName();
                        }
                    }
                    else
                    {
                        argument.ClearShortName();
                    }
                }
            }

            if (!string.IsNullOrEmpty(argument.ShortName))
            {
                if (Attribute.AllowMultipleShortNamesInOneToken &&
                    argument.ShortName.Length > 1)
                {
                    throw new InvalidArgumentSetException(argument, string.Format(CultureInfo.CurrentCulture,
                        Strings.ArgumentShortNameTooLong,
                        argument.ShortName));
                }

                _namedArgumentsByName.Add(argument.ShortName, argument);
            }

            // Add to unique list.
            _namedArguments.Add(argument);
        }

        private void AddPositionalArgument(ArgumentDefinition arg, int positionalIndexBias)
        {
            var attrib = (PositionalArgumentAttribute)arg.Attribute;
            var position = positionalIndexBias + attrib.Position;

            if (_positionalArguments.ContainsKey(position))
            {
                throw new InvalidArgumentSetException(arg, string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.DuplicatePositionArguments,
                    _positionalArguments[position].Member.MemberInfo.Name,
                    arg.Member.MemberInfo.Name,
                    position));
            }

            _positionalArguments.Add(position, arg);
            _nextPositionalArgIndexToDefine = position + 1;
        }

        private static IEnumerable<IMutableMemberInfo> GetAllFieldsAndProperties(Type type, bool includeNonPublicMembers)
        {
            // Generate a list of the fields and properties declared on
            // 'argumentSpecification', and on all types in its inheritance
            // hierarchy.
            var members = new List<IMutableMemberInfo>();
            for (var currentType = type; currentType != null; currentType = currentType.GetTypeInfo().BaseType)
            {
                var bindingFlags =
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.DeclaredOnly;

                if (includeNonPublicMembers)
                {
                    bindingFlags |= BindingFlags.NonPublic;
                }

                members.AddRange(currentType.GetFieldsAndProperties(bindingFlags));
            }

            return members;
        }

        private static IEnumerable<ArgumentDefinition> GetArgumentDescriptors(Type type, ArgumentSetAttribute setAttribute, object defaultValues, CommandLineParserOptions options, object fixedDestination)
        {
            // Find all fields and properties that have argument attributes on
            // them. For each that we find, capture information about them.
            var argList = GetAllFieldsAndProperties(type, includeNonPublicMembers: true)
                .SelectMany(member => CreateArgumentDescriptorsIfApplicable(member, defaultValues, setAttribute, options, fixedDestination));

            // If the argument set attribute indicates that we should also
            // include un-attributed, public, writable members as named
            // arguments, then look for them now.
            if (setAttribute.PublicMembersAreNamedArguments)
            {
                argList = argList.Concat(GetAllFieldsAndProperties(type, includeNonPublicMembers: false)
                    .Where(member => member.IsWritable)
                    .Where(member => member.MemberInfo.GetSingleAttribute<ArgumentBaseAttribute>() == null)
                    .Where(member => member.MemberInfo.GetSingleAttribute<ArgumentGroupAttribute>() == null)
                    .Select(member => CreateArgumentDescriptor(member, new NamedArgumentAttribute(), defaultValues, setAttribute, options, fixedDestination)));
            }

            return argList;
        }

        private static IEnumerable<ArgumentDefinition> CreateArgumentDescriptorsIfApplicable(IMutableMemberInfo member, object defaultValues,
            ArgumentSetAttribute setAttribute, CommandLineParserOptions options, object fixedDestination)
        {
            var descriptors = Enumerable.Empty<ArgumentDefinition>();

            var argAttrib = member.MemberInfo.GetSingleAttribute<ArgumentBaseAttribute>();
            if (argAttrib != null)
            {
                descriptors = descriptors.Concat(new[] { CreateArgumentDescriptor(member, argAttrib, defaultValues, setAttribute, options, fixedDestination) });
            }

            var groupAttrib = member.MemberInfo.GetSingleAttribute<ArgumentGroupAttribute>();
            if (groupAttrib != null)
            {
                // TODO: investigate defaultValues
                descriptors = descriptors.Concat(GetArgumentDescriptors(member.MemberType,
                    setAttribute,
                    /*defaultValues=*/null,
                    options,
                    fixedDestination));
            }

            return descriptors;
        }

        private static ArgumentDefinition CreateArgumentDescriptor(
            IMutableMemberInfo member,
            ArgumentBaseAttribute attribute,
            object defaultValues,
            ArgumentSetAttribute setAttribute,
            CommandLineParserOptions options,
            object fixedDestination)
        {
            if (!member.IsReadable || !member.IsWritable)
            {
                var declaringType = member.MemberInfo.DeclaringType;

                throw new InvalidArgumentSetException(member, string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.MemberNotSupported,
                    member.MemberInfo.Name,
                    declaringType?.Name));
            }

            var defaultFieldValue = (defaultValues != null) ? member.GetValue(defaultValues) : null;
            return new ArgumentDefinition(member,
                attribute,
                setAttribute,
                options,
                defaultFieldValue,
                fixedDestination: fixedDestination);
        }

        private void ValidateThatPositionalArgumentsDoNotOverlap()
        {
            var namedArguments = _namedArguments;
            var positionalArguments = _positionalArguments;

            // Validate positional arguments.
            var lastIndex = -1;
            var allArgsConsumed = namedArguments.Any(a => a.TakesRestOfLine);
            foreach (var argument in positionalArguments)
            {
                if (allArgsConsumed || (argument.Key != lastIndex + 1))
                {
                    throw new InvalidArgumentSetException(
                        argument.Value,
                        Strings.NonConsecutivePositionalParameters);
                }

                lastIndex = argument.Key;
                allArgsConsumed = argument.Value.TakesRestOfLine || argument.Value.AllowMultiple;
            }
        }

        private StringComparer StringComparerToUse =>
            Attribute.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
    }
}
