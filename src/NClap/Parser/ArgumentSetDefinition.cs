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
    public sealed class ArgumentSetDefinition : IDeepCloneable<ArgumentSetDefinition>
    {
        // Argument metadata and parse state.
        private readonly Dictionary<IMutableMemberInfo, ArgumentDefinition> _argumentsByMember = new Dictionary<IMutableMemberInfo, ArgumentDefinition>();
        private readonly List<ArgumentDefinition> _namedArguments = new List<ArgumentDefinition>();
        private readonly Dictionary<string, ArgumentDefinition> _namedArgumentsByName;
        private readonly Dictionary<string, ArgumentDefinition> _namedArgumentsByShortName;
        private readonly Dictionary<string, ArgumentDefinition> _namedArgumentsByLongName;
        private readonly SortedList<int, ArgumentDefinition> _positionalArguments = new SortedList<int, ArgumentDefinition>();
        private readonly List<ArgumentSetAttribute> _auxiliaryAttributes = new List<ArgumentSetAttribute>();

        // State.
        private int _nextPositionalArgIndexToDefine;

        /// <summary>
        /// Constructs an empty argument set.
        /// </summary>
        /// <param name="setAttribute">Optionally provides attribute information describing the
        /// argument set.</param>
        public ArgumentSetDefinition(ArgumentSetAttribute setAttribute = null)
        {
            // Save off set attributes; if none were provided, construct some defaults.
            Attribute = setAttribute ?? new ArgumentSetAttribute();

            // Set up private fields dependent on the ArgumentSetAttribute.
            _namedArgumentsByName = new Dictionary<string, ArgumentDefinition>(StringComparerToUse);
            _namedArgumentsByShortName = new Dictionary<string, ArgumentDefinition>(StringComparerToUse);
            _namedArgumentsByLongName = new Dictionary<string, ArgumentDefinition>(StringComparerToUse);
        }

        private ArgumentSetDefinition(ArgumentSetDefinition template) : this(template.Attribute)
        {
            _argumentsByMember = template._argumentsByMember.ToDictionary(p => p.Key, p => p.Value);
            _namedArguments.AddRange(template._namedArguments);
            _namedArgumentsByName = template._namedArgumentsByName.ToDictionary(p => p.Key, p => p.Value, StringComparerToUse);
            _namedArgumentsByShortName = template._namedArgumentsByShortName.ToDictionary(p => p.Key, p => p.Value, StringComparerToUse);
            _namedArgumentsByLongName = template._namedArgumentsByLongName.ToDictionary(p => p.Key, p => p.Value, StringComparerToUse);

            foreach (var positionalArg in template._positionalArguments)
            {
                _positionalArguments.Add(positionalArg.Key, positionalArg.Value);
            }

            _nextPositionalArgIndexToDefine = template._nextPositionalArgIndexToDefine;
            _auxiliaryAttributes.AddRange(template._auxiliaryAttributes);
        }

        /// <summary>
        /// Options for the set.
        /// </summary>
        public ArgumentSetAttribute Attribute { get; }

        /// <summary>
        /// All arguments in this set.
        /// </summary>
        public IEnumerable<ArgumentDefinition> AllArguments => NamedArguments.Concat(PositionalArguments);

        /// <summary>
        /// The named arguments in this set.
        /// </summary>
        public IEnumerable<ArgumentDefinition> NamedArguments => _namedArguments;

        /// <summary>
        /// The positional arguments in this set, in index order.
        /// </summary>
        public IEnumerable<ArgumentDefinition> PositionalArguments => _positionalArguments.Values;

        /// <summary>
        /// Optionally indicates the default assembly associated with this definition.
        /// </summary>
        internal Assembly DefaultAssembly { get; set; }

        /// <summary>
        /// Enumerates all names of named arguments (of all name types).
        /// </summary>
        /// <returns>Enumeration.</returns>
        public IEnumerable<string> GetAllArgumentNames() => _namedArgumentsByName.Keys;

        /// <summary>
        /// Enumerates named arguments of the given type.
        /// </summary>
        /// <param name="nameType">Type of name to look up.</param>
        /// <returns>Enumeration.</returns>
        public IEnumerable<string> GetArgumentNames(ArgumentNameType nameType)
        {
            var dict = GetNamedArgumentDictionary(nameType);
            return dict.Keys;
        }

        /// <summary>
        /// Try to look up a named argument by short or long name.
        /// </summary>
        /// <param name="nameType">Type of name to look up.</param>
        /// <param name="name">Name to look up.</param>
        /// <param name="arg">On success, receives the named argument.</param>
        /// <returns>True on success; false otherwise.</returns>
        public bool TryGetNamedArgument(ArgumentNameType nameType, string name, out ArgumentDefinition arg)
        {
            var dict = GetNamedArgumentDictionary(nameType);
            return dict.TryGetValue(name, out arg);
        }

        /// <summary>
        /// Try to look up a positional argument by position.
        /// </summary>
        /// <param name="position">0-based position index to look up.</param>
        /// <param name="arg">On success, receives the argument.</param>
        /// <returns>True on success; false otherwise.</returns>
        public bool TryGetPositionalArgument(int position, out ArgumentDefinition arg) => _positionalArguments.TryGetValue(position, out arg);

        /// <summary>
        /// Adds an argument.
        /// </summary>
        /// <param name="arg">Argument to define.</param>
        public void Add(ArgumentDefinition arg) => Add(new[] { arg });

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

        /// <summary>
        /// Adds an auxiliary argument set attribute.
        /// </summary>
        /// <param name="attrib">Attribute to add.</param>
        public void AddAuxiliaryAttribute(ArgumentSetAttribute attrib) =>
            _auxiliaryAttributes.Add(attrib);

        /// <summary>
        /// Shallow clones this definition.
        /// </summary>
        /// <returns>The cloned object.</returns>
        public ArgumentSetDefinition DeepClone() => new ArgumentSetDefinition(this);

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
            _namedArgumentsByLongName.Add(argument.LongName, argument);

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
                            _namedArgumentsByShortName.Remove(conflictingArg.ShortName);
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
                if (Attribute.ShortNamesAreOneCharacterLong && argument.ShortName.Length > 1)
                {
                    throw new InvalidArgumentSetException(argument, string.Format(CultureInfo.CurrentCulture,
                        Strings.ArgumentShortNameTooLong,
                        argument.ShortName));
                }

                _namedArgumentsByName.Add(argument.ShortName, argument);
                _namedArgumentsByShortName.Add(argument.ShortName, argument);
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

        /// <summary>
        /// String comparer to use for names in this argument set.
        /// </summary>
        private StringComparer StringComparerToUse =>
            Attribute.CaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

        private IReadOnlyDictionary<string, ArgumentDefinition> GetNamedArgumentDictionary(ArgumentNameType nameType)
        {
            switch (nameType)
            {
                case ArgumentNameType.ShortName:
                    return _namedArgumentsByShortName;
                case ArgumentNameType.LongName:
                    return _namedArgumentsByLongName;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nameType));
            }
        }
    }
}
