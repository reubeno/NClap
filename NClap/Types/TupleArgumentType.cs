using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NClap.Types
{
    /// <summary>
    /// Implementation to describe Tuple types.
    /// </summary>
    class TupleArgumentType : ArgumentTypeBase
    {
        private const char ItemSeparatorChar = ',';

        private readonly Type[] _typeParameters;
        private readonly IReadOnlyList<IArgumentType> _argTypeParameters;
        private readonly ConstructorInfo _creatorMethod;

        /// <summary>
        /// Constructs an object to describe the provided Tuple type.
        /// </summary>
        /// <param name="type">The Tuple type to describe.</param>
        public TupleArgumentType(Type type)
            : base(type)
        {
            if (type.GetInterface("ITuple") == null)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            _typeParameters = type.GetGenericArguments();
            Debug.Assert(_typeParameters != null);
            Debug.Assert(_typeParameters.Length > 0);

            _argTypeParameters = _typeParameters.Select(ArgumentType.GetType).ToList();
            _creatorMethod = type.GetConstructor(_typeParameters);
        }

        /// <summary>
        /// A summary of the concrete syntax required to indicate a value of
        /// the type described by this interface (e.g. "&gt;Int32&lt;").
        /// </summary>
        public override string SyntaxSummary =>
            string.Join(",", _argTypeParameters.Select(param => param.SyntaxSummary));

        /// <summary>
        /// Converts a value into a readable string form.  The value must be of
        /// the type described by this interface.
        /// </summary>
        /// <param name="value">The value to format into a string.</param>
        /// <returns>The formatted string.</returns>
        public override string Format(object value)
        {
            var formatted = base.Format(value);
            return formatted.Substring(1, formatted.Length - 2).Replace(", ", ",");
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

            var tokens = valueToComplete.Split(ItemSeparatorChar);
            Debug.Assert(tokens.Length >= 1);

            var currentTokenIndex = tokens.Length - 1;
            var currentToken = tokens[currentTokenIndex];

            tokens[currentTokenIndex] = string.Empty;

            return _argTypeParameters[currentTokenIndex].GetCompletions(context, currentToken)
                       .Select(completion => string.Join(ItemSeparatorChar.ToString(), tokens) + completion);
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
            var tokens = stringToParse.Split(ItemSeparatorChar);
            if (tokens.Length != _typeParameters.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(stringToParse), Strings.TooFewElementsInTupleString);
            }

            var parsedObjects = new object[_typeParameters.Length];
            for (var i = 0; i < tokens.Length; ++i)
            {
                if (!_argTypeParameters[i].TryParse(context, tokens[i], out parsedObjects[i]))
                {
                    throw new ArgumentOutOfRangeException(nameof(stringToParse));
                }
            }

            return _creatorMethod.Invoke(parsedObjects);
        }
    }
}
