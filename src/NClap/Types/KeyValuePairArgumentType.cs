using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace NClap.Types
{
    /// <summary>
    /// Implementation to describe KeyValuePair&lt;,&gt; types.
    /// </summary>
    class KeyValuePairArgumentType : ArgumentTypeBase
    {
        private const char KeyValueSeparatorChar = '=';

        private readonly IArgumentType _keyType;
        private readonly IArgumentType _valueType;
        private readonly ConstructorInfo _constructor;

        /// <summary>
        /// Constructs an object to describe the provided KeyValuePair type.
        /// </summary>
        /// <param name="type">The KeyValuePair type to describe.</param>
        public KeyValuePairArgumentType(Type type)
            : base(type)
        {
            if (!type.GetTypeInfo().IsGenericType)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            if (type.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            var typeParams = type.GetTypeInfo().GetGenericArguments();
            if (!ArgumentType.TryGetType(typeParams[0], out _keyType) ||
                !ArgumentType.TryGetType(typeParams[1], out _valueType))
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Strings.ConstituentTypeNotSupported, type.Name));
            }

            _constructor = type.GetTypeInfo().GetConstructor(typeParams);
            Debug.Assert(_constructor != null);
        }

        /// <summary>
        /// A summary of the concrete syntax required to indicate a value of
        /// the type described by this interface (e.g. "&gt;Int32&lt;").
        /// </summary>
        public override string SyntaxSummary =>
            string.Format(CultureInfo.CurrentCulture, "{0}={1}", _keyType.SyntaxSummary, _valueType.SyntaxSummary);

        /// <summary>
        /// Converts a value into a readable string form.  The value must be of
        /// the type described by this interface.
        /// </summary>
        /// <param name="value">The value to format into a string.</param>
        /// <returns>The formatted string.</returns>
        public override string Format(object value)
        {
            var formatted = base.Format(value);
            return formatted.Substring(1, formatted.Length - 2).Replace(", ", "=");
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
        public override IEnumerable<string> GetCompletions(ArgumentCompletionContext context, string valueToComplete)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (valueToComplete == null)
            {
                throw new ArgumentNullException(nameof(valueToComplete));
            }

            var separatorIndex = valueToComplete.IndexOf(KeyValueSeparatorChar);
            if (separatorIndex < 0)
            {
                return _keyType.GetCompletions(context, valueToComplete);
            }

            var valueSoFar = valueToComplete.Substring(separatorIndex + 1);
            return _valueType.GetCompletions(context, valueSoFar)
                       .Select(completion => valueToComplete.Substring(0, separatorIndex + 1) + completion);
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
            var separatorIndex = stringToParse.IndexOf(KeyValueSeparatorChar);
            if (separatorIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            var keyString = stringToParse.Substring(0, separatorIndex);
            var valueString = stringToParse.Substring(separatorIndex + 1);

            if (!_keyType.TryParse(context, keyString, out object key))
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            if (!_valueType.TryParse(context, valueString, out object value))
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse));
            }

            return _constructor.Invoke(new[] { key, value });
        }
    }
}
