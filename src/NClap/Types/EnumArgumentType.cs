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
        private readonly IReadOnlyList<FieldInfo> _values;

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

            _valueNameMap = ConstructValueNameMap(type);
            _values = _valueNameMap.Values.Distinct().ToList();
        }

        /// <summary>
        /// Constructs a new object to describe the provided enumeration type.
        /// </summary>
        /// <param name="type">The enumeration type to describe.</param>
        /// <returns>The constructed type object.</returns>
        public static EnumArgumentType Create(Type type)
        {
            var flagsAttrib = type.GetTypeInfo().GetCustomAttribute<FlagsAttribute>();
            return (flagsAttrib != null) ? new FlagsEnumArgumentType(type) : new EnumArgumentType(type);
        }

        /// <summary>
        /// A summary of the concrete syntax required to indicate a value of
        /// the type described by this interface (e.g. "&lt;Int32&gt;").
        /// </summary>
        public override string SyntaxSummary => string.Format(
            CultureInfo.CurrentCulture,
            "{{{0}}}",
            string.Join(" | ", _values.Where(value => !IsValueDisallowed(value) && !IsValueHidden(value))
                                      .Select(GetDisplayNameForHelp)));

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
            SelectCompletions(context, valueToComplete, Type.GetTypeInfo().GetEnumNames());

        /// <summary>
        /// Parses the provided string.  Throws an exception if the string
        /// cannot be parsed.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="stringToParse">String to parse.</param>
        /// <returns>The parsed object.</returns>
        protected override object Parse(ArgumentParseContext context, string stringToParse)
        {
            // First try looking up the string in our name map.
            if (_valueNameMap.TryGetValue(stringToParse, out FieldInfo field))
            {
                stringToParse = field.Name;
            }

            // Otherwise, only let through literal integers.
            else if (!int.TryParse(stringToParse, NumberStyles.AllowLeadingSign, null, out int parsedInt))
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            // Now use base facilities for parsing.
            var parsedObject = Enum.Parse(Type, stringToParse, true /* ignore case */);

            // Try to look find any <see cref="ArgumentValueAttribute" />
            // associated with this value.
            var name = Enum.GetName(Type, parsedObject);
            if (IsValueDisallowed(name))
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            return parsedObject;
        }

        /// <summary>
        /// Checks if the indicated (named) value has been disallowed by
        /// metadata.
        /// </summary>
        /// <param name="valueName">The name of the value to check.</param>
        /// <returns>True if the value has been disallowed; false otherwise.
        /// </returns>
        private bool IsValueDisallowed(string valueName) =>
            DoesValueHaveFlags(valueName, ArgumentValueFlags.Disallowed);

        /// <summary>
        /// Checks if the indicated (named) value has been disallowed by
        /// metadata.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value has been disallowed; false otherwise.
        /// </returns>
        private bool IsValueDisallowed(FieldInfo value) =>
            DoesValueHaveFlags(value, ArgumentValueFlags.Disallowed);

        /// <summary>
        /// Checks if the indicated (named) value should be hidden from help
        /// content.
        /// </summary>
        /// <param name="valueName">The name of the value to check.</param>
        /// <returns>True if the value should be hidden; false otherwise.
        /// </returns>
        private bool IsValueHidden(string valueName) =>
            DoesValueHaveFlags(valueName, ArgumentValueFlags.Hidden);

        /// <summary>
        /// Checks if the indicated (named) value should be hidden from help
        /// content.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value should be hidden; false otherwise.
        /// </returns>
        private bool IsValueHidden(FieldInfo value) =>
            DoesValueHaveFlags(value, ArgumentValueFlags.Hidden);

        /// <summary>
        /// Checks if the indicated (named) value is associated with
        /// ArgumentValueFlags metadata and has the indicated flags.
        /// </summary>
        /// <param name="valueName">The name of the value to check.</param>
        /// <param name="flags">The flags to check the presence of.</param>
        /// <returns>True if the value has the indicated flag; false otherwise.
        /// </returns>
        private bool DoesValueHaveFlags(string valueName, ArgumentValueFlags flags)
        {
            if (valueName == null)
            {
                return false;
            }

            if (!_valueNameMap.TryGetValue(valueName, out FieldInfo field))
            {
                return false;
            }

            return DoesValueHaveFlags(field, flags);
        }

        /// <summary>
        /// Checks if the indicated (named) value is associated with
        /// ArgumentValueFlags metadata and has the indicated flags.
        /// </summary>
        /// <param name="value">Info for the value to check.</param>
        /// <param name="flags">The flags to check the presence of.</param>
        /// <returns>True if the value has the indicated flag; false otherwise.
        /// </returns>
        private bool DoesValueHaveFlags(FieldInfo value, ArgumentValueFlags flags)
        {
            var attrib = TryGetArgumentValueAttribute(value);
            if (attrib == null)
            {
                return false;
            }

            return attrib.Flags.HasFlag(flags);
        }

        private string GetDisplayNameForHelp(FieldInfo value)
        {
            string displayName = value.Name.ToString();

            var attrib = TryGetArgumentValueAttribute(value);
            if (attrib?.LongName != null)
            {
                displayName = attrib.LongName;
            }
            else if (attrib?.ShortName != null)
            {
                displayName = attrib.ShortName;
            }

            return displayName;
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
            foreach (var field in type.GetTypeInfo().GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attrib = TryGetArgumentValueAttribute(field);

                // Skip values marked disallowed.
                if (attrib?.Flags.HasFlag(ArgumentValueFlags.Disallowed) ?? false)
                {
                    continue;
                }

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
