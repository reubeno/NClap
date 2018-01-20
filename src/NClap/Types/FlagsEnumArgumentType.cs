using System;
using System.Linq;
using System.Reflection;
using NClap.Exceptions;
using NClap.Utilities;

namespace NClap.Types
{
    /// <summary>
    /// Implementation to describe enumeration types with System.FlagsAttribute
    /// attributes.
    /// </summary>
    internal class FlagsEnumArgumentType : EnumArgumentType
    {
        /// <summary>
        /// The type underlying this enumeration type.
        /// </summary>
        private readonly Type _underlyingType;

        /// <summary>
        /// The IntegerArgumentType object for the type underlying this
        /// enumeration type.
        /// </summary>
        private readonly IntegerArgumentType _underlyingIntegerType;

        /// <summary>
        /// Primary constructor.
        /// </summary>
        /// <param name="type">The flag-based enumeration type to describe.
        /// </param>
        public FlagsEnumArgumentType(Type type) : base(type)
        {
            if (type.GetTypeInfo().GetSingleAttribute<FlagsAttribute>() == null)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            _underlyingType = Enum.GetUnderlyingType(type);
            if (!ArgumentType.TryGetType(_underlyingType, out IArgumentType underlyingArgType))
            {
                throw new NotSupportedException(nameof(type));
            }

            _underlyingIntegerType = underlyingArgType as IntegerArgumentType;
            if (_underlyingIntegerType == null)
            {
                throw new InternalInvariantBrokenException();
            }
        }

        /// <summary>
        /// Parses the provided string.  Throws an exception if the string
        /// cannot be parsed.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="stringToParse">String to parse.</param>
        /// <returns>The parsed object.</returns>
        protected override object Parse(ArgumentParseContext context, string stringToParse)
        {
            var pieces = stringToParse.Split('|').ToList();
            if ((pieces.Count == 0) || ((pieces.Count == 1) && (pieces[0].Length == 0)))
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            var underlyingResult =
                pieces.Select(piece => Convert.ChangeType(base.Parse(context, piece), _underlyingType))
                      .Aggregate(_underlyingIntegerType.Or);

            return Enum.ToObject(Type, underlyingResult);
        }

        /// <summary>
        /// Converts a value into a readable string form.  The value must be of
        /// the type described by this interface.
        /// </summary>
        /// <param name="value">The value to format into a string.</param>
        /// <returns>The formatted string.</returns>
        public override string Format(object value)
        {
            var formatted = base.Format(value);
            return formatted.Replace(", ", "|");
        }
    }
}
