using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NClap.Utilities;

namespace NClap.Types
{
    /// <summary>
    /// Implementation to describe enumeration types.
    /// </summary>
    internal class EnumArgumentType : ArgumentTypeBase, IEnumArgumentType
    {
        private readonly IReadOnlyDictionary<string, EnumArgumentValue> _valuesByName;
        private readonly IReadOnlyDictionary<object, EnumArgumentValue> _valuesByValue;
        private readonly IReadOnlyList<EnumArgumentValue> _values;

        /// <summary>
        /// The type underlying this enumeration type.
        /// </summary>
        protected readonly Type UnderlyingType;

        /// <summary>
        /// The IntegerArgumentType object for the type underlying this
        /// enumeration type.
        /// </summary>
        protected readonly IntegerArgumentType UnderlyingIntegerType;

        /// <summary>
        /// Constructs an object to describe the provided enumeration type.
        /// </summary>
        /// <param name="type">The enumeration type to describe.</param>
        protected EnumArgumentType(Type type) : base(type)
        {
            if (!type.GetTypeInfo().IsEnum)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            UnderlyingType = Enum.GetUnderlyingType(type);
            if (UnderlyingType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            if (!ArgumentType.TryGetType(UnderlyingType, out IArgumentType underlyingArgType))
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            UnderlyingIntegerType = underlyingArgType as IntegerArgumentType;
            if (UnderlyingIntegerType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            _values = GetAllValues(type).ToList();
            _valuesByName = ConstructValueNameMap(_values);
            _valuesByValue = _values.ToDictionary(v => v.Value, v => v);
        }

        /// <summary>
        /// Constructs a new object to describe the provided enumeration type.
        /// </summary>
        /// <param name="type">The enumeration type to describe.</param>
        /// <returns>The constructed type object.</returns>
        public static EnumArgumentType Create(Type type)
        {
            var flagsAttrib = type.GetTypeInfo().GetSingleAttribute<FlagsAttribute>();
            return (flagsAttrib != null) ? new FlagsEnumArgumentType(type) : new EnumArgumentType(type);
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
            // Only complete to long names.
            SelectCompletions(context, valueToComplete,
                _values.Where(v => !v.Hidden && !v.Disallowed)
                       .Select(v => v.LongName));

        /// <summary>
        /// Parses the provided string.  Throws an exception if the string
        /// cannot be parsed.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="stringToParse">String to parse.</param>
        /// <returns>The parsed object.</returns>
        protected override object Parse(ArgumentParseContext context, string stringToParse)
        {
            object parsedObject;

            // First try looking up the string in our name map.
            if (!_valuesByName.TryGetValue(stringToParse, out EnumArgumentValue value))
            {
                // Otherwise, only let through literal integers.
                if (!int.TryParse(stringToParse, NumberStyles.AllowLeadingSign, null, out int parsedInt))
                {
                    throw new ArgumentOutOfRangeException(nameof(stringToParse));
                }

                // Now use base facilities for parsing.
                parsedObject = Enum.Parse(Type, stringToParse, true /* ignore case */);

                // Try looking it up again, this time by value.
                if (!_valuesByValue.TryGetValue(parsedObject, out value))
                {
                    throw new ArgumentOutOfRangeException(nameof(stringToParse));
                }
            }

            if (value.Disallowed)
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            return value.Value;
        }

        /// <summary>
        /// Enumerate the values allowed for this enum.
        /// </summary>
        /// <returns>The values.</returns>
        public IEnumerable<IArgumentValue> GetValues() => _values;

        private static IEnumerable<EnumArgumentValue> GetAllValues(Type type) =>
            type.GetTypeInfo().GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new EnumArgumentValue(f));

        /// <summary>
        /// Constructs a map from the provided enum values, useful for parsing.
        /// </summary>
        /// <param name="values">The values in question.</param>
        /// <returns>The constructed map.</returns>
        private static IReadOnlyDictionary<string, EnumArgumentValue> ConstructValueNameMap(IEnumerable<EnumArgumentValue> values)
        {
            // TODO CASE: need to dynamically choose case sensitivity or not.
            var valueNameMap = new Dictionary<string, EnumArgumentValue>(StringComparer.OrdinalIgnoreCase);

            // Process each value allowed on the given type, adding all synonyms
            // that indicate them.
            foreach (var value in values.Where(v => !v.Disallowed))
            {
                // Make sure the long name for the value isn't a duplicate.
                if (valueNameMap.ContainsKey(value.LongName))
                {
                    throw new ArgumentOutOfRangeException(nameof(values), Strings.EnumValueLongNameIsInvalid);
                }

                // If explicitly provided, make sure the short name for the
                // value isn't a duplicate.
                if ((value.ShortName != null) && valueNameMap.ContainsKey(value.ShortName))
                {
                    throw new ArgumentOutOfRangeException(nameof(values), Strings.EnumValueShortNameIsInvalid);
                }

                // Add the long and short name.
                valueNameMap[value.LongName] = value;
                if (value.ShortName != null)
                {
                    valueNameMap[value.ShortName] = value;
                }
            }

            return valueNameMap;
        }
    }
}
