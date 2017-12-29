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
        private readonly IReadOnlyDictionary<string, EnumArgumentValue> _valuesByCaseSensitiveName;
        private readonly IReadOnlyDictionary<string, EnumArgumentValue> _valuesByCaseInsensitiveName;
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
            _valuesByCaseSensitiveName = ConstructValueNameMap(_values, true);
            _valuesByCaseInsensitiveName = ConstructValueNameMap(_values, false);
            _valuesByValue = ConstructValueMap(_values);
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
        /// Converts a value into a readable string form.  The value must be of
        /// the type described by this interface.
        /// </summary>
        /// <param name="value">The value to format into a string.</param>
        /// <returns>The formatted string.</returns>
        public override string Format(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (_valuesByValue.TryGetValue(value, out EnumArgumentValue enumValue))
            {
                return enumValue.DisplayName;
            }

            return value.ToString();
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

            // Select the appropriate map, based on context.
            var map = context.CaseSensitive ? _valuesByCaseSensitiveName : _valuesByCaseInsensitiveName;

            // First try looking up the string in our name map.
            if (!map.TryGetValue(stringToParse, out EnumArgumentValue value))
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

            return value.Value;
        }

        /// <summary>
        /// Enumerate the values allowed for this enum.
        /// </summary>
        /// <returns>The values.</returns>
        public IEnumerable<IArgumentValue> GetValues() => _values;

        /// <summary>
        /// Tries to look up the <see cref="IArgumentValue"/> corresponding with
        /// the given object.
        /// </summary>
        /// <param name="value">Object to look up.</param>
        /// <param name="argValue">On success, receives the object's value.</param>
        /// <returns>true on success; false otherwise.</returns>
        public bool TryGetValue(object value, out IArgumentValue argValue)
        {
            if (!_valuesByValue.TryGetValue(value, out EnumArgumentValue enumArgValue))
            {
                argValue = null;
                return false;
            }

            argValue = enumArgValue;
            return true;
        }

        private static IEnumerable<EnumArgumentValue> GetAllValues(Type type) =>
            type.GetTypeInfo().GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new EnumArgumentValue(f));

        private static IReadOnlyDictionary<object, EnumArgumentValue> ConstructValueMap(IEnumerable<EnumArgumentValue> values)
        {
            var map = new Dictionary<object, EnumArgumentValue>();

            // Walk through all known values, trying to add them to the map.
            foreach (var v in values)
            {
                // We do our best to add each value to the map; but if there
                // are multiple members that share a value, then the first
                // one will "win".  We don't bother trying to maintain a
                // multimap.
                if (!map.ContainsKey(v.Value))
                {
                    map.Add(v.Value, v);
                }
            }

            return map;
        }

        /// <summary>
        /// Constructs a map from the provided enum values, useful for parsing.
        /// </summary>
        /// <param name="values">The values in question.</param>
        /// <param name="caseSensitive">True for the map to be built with case
        /// sensitivity; false for case insensitivity.</param>
        /// <returns>The constructed map.</returns>
        private static IReadOnlyDictionary<string, EnumArgumentValue> ConstructValueNameMap(IEnumerable<EnumArgumentValue> values, bool caseSensitive)
        {
            var valueNameMap = new Dictionary<string, EnumArgumentValue>(
                caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

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
