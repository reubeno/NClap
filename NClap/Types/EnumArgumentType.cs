using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using NClap.Metadata;

namespace NClap.Types
{
    /// <summary>
    /// Implementation to describe enumeration types.
    /// </summary>
    class EnumArgumentType : ArgumentTypeBase
    {
        private readonly IReadOnlyDictionary<string, FieldInfo> _valueNameMap;

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
            if (!type.IsEnum)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            UnderlyingType = Enum.GetUnderlyingType(type);
            if (UnderlyingType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            IArgumentType underlyingArgType;
            if (!ArgumentType.TryGetType(UnderlyingType, out underlyingArgType))
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            UnderlyingIntegerType = underlyingArgType as IntegerArgumentType;
            if (UnderlyingIntegerType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            _valueNameMap = ConstructValueNameMap(type);
        }

        /// <summary>
        /// Constructs a new object to describe the provided enumeration type.
        /// </summary>
        /// <param name="type">The enumeration type to describe.</param>
        /// <returns>The constructed type object.</returns>
        public static EnumArgumentType Create(Type type)
        {
            var flagsAttrib = type.GetCustomAttribute<FlagsAttribute>();
            return (flagsAttrib != null) ? new FlagsEnumArgumentType(type) : new EnumArgumentType(type);
        }

        /// <summary>
        /// A summary of the concrete syntax required to indicate a value of
        /// the type described by this interface (e.g. "&gt;Int32&lt;").
        /// </summary>
        public override string SyntaxSummary => string.Format(
            CultureInfo.CurrentCulture,
            "{{{0}}}",
            string.Join(" | ", Type.GetEnumNames()));

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
            SelectCompletions(context, valueToComplete, Type.GetEnumNames());

        /// <summary>
        /// Parses the provided string.  Throws an exception if the string
        /// cannot be parsed.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="stringToParse">String to parse.</param>
        /// <returns>The parsed object.</returns>
        protected override object Parse(ArgumentParseContext context, string stringToParse)
        {
            FieldInfo field;

            // First try looking up the string in our name map.
            if (_valueNameMap.TryGetValue(stringToParse, out field))
            {
                stringToParse = field.Name;
            }

            // Now use base facilities for parsing.
            var parsedObject = Enum.Parse(Type, stringToParse, true /* ignore case */);

            // Try to look find any <see cref="ArgumentValueAttribute" />
            // associated with this value.
            var name = Enum.GetName(Type, parsedObject);
            if ((name != null) &&
                (_valueNameMap.TryGetValue(name, out field)))
            {
                var attrib = TryGetArgumentValueAttribute(field);
                if ((attrib != null) &&
                    (attrib.Flags.HasFlag(ArgumentValueFlags.Disallowed)))
                {
                    throw new ArgumentOutOfRangeException(nameof(stringToParse));
                }
            }

            return parsedObject;
        }

        /// <summary>
        /// Constructs a map associating the set of strings that may be used
        /// to indicate a value of the given enum type, along with the
        /// values they map to.
        /// </summary>
        /// <param name="type">The enum type in question.</param>
        /// <returns>The constructed map.</returns>
        private static IReadOnlyDictionary<string, FieldInfo> ConstructValueNameMap(Type type)
        {
            var valueNameMap = new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);

            // Process each value allowed on the given type, adding all synonyms
            // that indicate them.
            foreach (var field in type.GetFields())
            {
                var attrib = TryGetArgumentValueAttribute(field);
                var longName = attrib?.LongName ?? field.Name;
                var shortName = attrib?.ShortName;

                // Make sure the long name for the value isn't a duplicate.
                if (valueNameMap.ContainsKey(longName))
                {
                    throw new ArgumentOutOfRangeException(nameof(type), Strings.EnumValueLongNameIsInvalid);
                }

                // If explicitly provided, make sure the short name for the
                // value isn't a duplicate.
                if ((shortName != null) && valueNameMap.ContainsKey(shortName))
                {
                    throw new ArgumentOutOfRangeException(nameof(type), Strings.EnumValueShortNameIsInvalid);
                }

                // Add the long and short name.
                valueNameMap[longName] = field;
                if (shortName != null)
                {
                    valueNameMap[shortName] = field;
                }
            }

            return valueNameMap;
        }

        private static ArgumentValueAttribute TryGetArgumentValueAttribute(FieldInfo field) =>
            // Look for an <see cref="ArgumentValueAttribute" /> attribute,
            // which might further customize how we can parse strings into
            // this value.
            field.GetCustomAttributes(typeof(ArgumentValueAttribute), false)
                    .Cast<ArgumentValueAttribute>()
                    .SingleOrDefault();
    }
}
