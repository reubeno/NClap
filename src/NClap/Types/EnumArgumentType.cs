using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NClap.Metadata;
using NClap.Utilities;

namespace NClap.Types
{
    /// <summary>
    /// Implementation to describe enumeration types.
    /// </summary>
    internal class EnumArgumentType : ArgumentTypeBase, IEnumArgumentType
    {
        private readonly Dictionary<string, IArgumentValue> _valuesByCaseSensitiveName =
            new Dictionary<string, IArgumentValue>(StringComparer.Ordinal);

        private readonly Dictionary<string, IArgumentValue> _valuesByCaseInsensitiveName =
            new Dictionary<string, IArgumentValue>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<object, IArgumentValue> _valuesByValue = new Dictionary<object, IArgumentValue>();

        private readonly List<IArgumentValue> _values = new List<IArgumentValue>();

        /// <summary>
        /// Constructs an object to describe an empty enumeration type.  Values must be
        /// separately defined.
        /// </summary>
        protected EnumArgumentType() : base(typeof(object))
        {
        }

        /// <summary>
        /// Constructs an object to describe the provided enumeration type.
        /// </summary>
        /// <param name="type">The enumeration type to describe.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="type" />
        /// is not an enum type with a valid backing integer representation type.</exception>
        protected EnumArgumentType(Type type) : base(type)
        {
            AddValuesFromType(type);
        }

        /// <summary>
        /// Constructs a new object to describe the provided enumeration type.
        /// </summary>
        /// <param name="type">The enumeration type to describe.</param>
        /// <returns>The constructed type object.</returns>
        public static EnumArgumentType Create(Type type)
        {
            var flagsAttrib = type.GetTypeInfo().GetSingleAttribute<FlagsAttribute>();
            var argType = (flagsAttrib != null) ? new FlagsEnumArgumentType(type) : new EnumArgumentType(type);

            var extensibleAttribs = type.GetTypeInfo().GetAttributes<ExtensibleEnumAttribute>().ToList();
            if (extensibleAttribs.Count > 0)
            {
                var types = extensibleAttribs.Select(a => a.Provider)
                    .Where(p => p != null)
                    .Select(ConstructEnumArgumentTypeProviderFromType)
                    .SelectMany(p => p.GetTypes());

                argType = new MergedEnumArgumentType(new[] { argType }.Concat(types));
            }

            return argType;
        }

        /// <summary>
        /// Converts a value into a readable string form.  The value must be of
        /// the type described by this interface.
        /// </summary>
        /// <param name="value">The value to format into a string.</param>
        /// <returns>The formatted string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/>
        /// is null.</exception>
        public override string Format(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (_valuesByValue.TryGetValue(value, out IArgumentValue enumValue))
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
            if (!map.TryGetValue(stringToParse, out IArgumentValue value))
            {
                // We might have more options if it's an enum type, but if it's not--there's
                // nothing else we can do.
                if (!Type.GetTypeInfo().IsEnum)
                {
                    throw new ArgumentOutOfRangeException(nameof(stringToParse));
                }

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
            if (!_valuesByValue.TryGetValue(value, out IArgumentValue enumArgValue))
            {
                argValue = null;
                return false;
            }

            argValue = enumArgValue;
            return true;
        }

        /// <summary>
        /// Defines a new value in this type.
        /// </summary>
        /// <param name="value">Value to define.</param>
        protected void AddValue(IArgumentValue value)
        {
            _values.Add(value);
            AddToValueNameMap(_valuesByCaseSensitiveName, value);
            AddToValueNameMap(_valuesByCaseInsensitiveName, value);
            AddToValueMap(_valuesByValue, value);
        }

        /// <summary>
        /// Inspects the given type and defines all values in it.
        /// </summary>
        /// <param name="type">Type to inspect.</param>
        protected void AddValuesFromType(Type type)
        {
            if (!type.GetTypeInfo().IsEnum)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            foreach (var value in GetAllValues(type))
            {
                AddValue(value);
            }
        }

        /// <summary>
        /// Defines all values in the given type.
        /// </summary>
        /// <param name="type">Type to inspect.</param>
        protected void AddValuesFromType(IEnumArgumentType type)
        {
            foreach (var value in type.GetValues())
            {
                AddValue(value);
            }
        }

        private static IEnumerable<IArgumentValue> GetAllValues(Type type) =>
            type.GetTypeInfo().GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new EnumArgumentValue(f));

        private static void AddToValueMap(Dictionary<object, IArgumentValue> map, IArgumentValue value)
        {
            // We do our best to add each value to the map; but if there
            // are multiple members that share a value, then the first
            // one will "win".  We don't bother trying to maintain a
            // multimap.
            if (!map.ContainsKey(value.Value))
            {
                map.Add(value.Value, value);
            }
        }

        /// <summary>
        /// Adds the given value to the provided name map.
        /// </summary>
        /// <param name="map">Map to add to.</param>
        /// <param name="value">The value to add.</param>
        private static void AddToValueNameMap(Dictionary<string, IArgumentValue> map, IArgumentValue value)
        {
            // We skip disallowed values.
            if (value.Disallowed) return;

            // Make sure the long name for the value isn't a duplicate.
            if (map.ContainsKey(value.LongName))
            {
                throw new ArgumentOutOfRangeException(nameof(value), Strings.EnumValueLongNameIsInvalid);
            }

            // If explicitly provided, make sure the short name for the
            // value isn't a duplicate.
            if ((value.ShortName != null) && map.ContainsKey(value.ShortName))
            {
                throw new ArgumentOutOfRangeException(nameof(value), Strings.EnumValueShortNameIsInvalid);
            }

            // Add the long and short name.
            map[value.LongName] = value;
            if (value.ShortName != null)
            {
                map[value.ShortName] = value;
            }
        }

        private static IEnumArgumentTypeProvider ConstructEnumArgumentTypeProviderFromType(Type type)
        {
            return (IEnumArgumentTypeProvider)type.GetParameterlessConstructor().Invoke(Array.Empty<object>());
        }
    }
}
