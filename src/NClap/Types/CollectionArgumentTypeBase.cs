using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace NClap.Types
{
    /// <summary>
    /// Abstract base class for implementing the ICollectionArgumentType
    /// interface.
    /// </summary>
    internal abstract class CollectionArgumentTypeBase : ArgumentTypeBase, ICollectionArgumentType
    {
        private readonly IArgumentType _elementArgumentType;

        /// <summary>
        /// Constructor for use by derived classes.
        /// </summary>
        /// <param name="type">Type described by this object.</param>
        /// <param name="elementType">Type of elements stored within the
        /// collection type described by this object.</param>
        protected CollectionArgumentTypeBase(Type type, Type elementType)
            : base(type)
        {
            if (!ArgumentType.TryGetType(elementType, out _elementArgumentType))
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Strings.ElementTypeNotSupported, type));
            }
        }

        /// <summary>
        /// Type of elements in the collection described by this object.
        /// </summary>
        public IArgumentType ElementType => _elementArgumentType;

        /// <summary>
        /// A summary of the concrete syntax required to indicate a value of
        /// the type described by this interface (e.g. "&gt;Int32&lt;").
        /// </summary>
        public override string SyntaxSummary => _elementArgumentType.SyntaxSummary;

        /// <summary>
        /// Converts a value into a readable string form.  The value must be of
        /// the type described by this interface.
        /// </summary>
        /// <param name="value">The value to format into a string.</param>
        /// <returns>The formatted string.</returns>
        public override string Format(object value) =>
            string.Join(", ", GetElements(value).Select(ElementType.Format));

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

            string[] tokens;
            if (context.ParseContext.ElementSeparators.Any())
            {
                tokens = valueToComplete.Split(
                    context.ParseContext.ElementSeparators.ToArray(),
                    StringSplitOptions.None);
            }
            else
            {
                tokens = new[] { valueToComplete };
            }

            Debug.Assert(tokens.Length >= 1);

            var currentTokenIndex = tokens.Length - 1;
            var currentToken = tokens[currentTokenIndex];

            tokens[currentTokenIndex] = string.Empty;

            var preferredSeparator = GetPreferredElementSeparatorOrDefault(context.ParseContext) ?? string.Empty;

            return _elementArgumentType.GetCompletions(context, currentToken)
                       .Select(completion => string.Join(preferredSeparator, tokens) + completion);
        }

        /// <summary>
        /// Constructs a collection of the type described by this object,
        /// populated with objects from the provided input collection.
        /// </summary>
        /// <param name="objects">Objects to add to the collection.</param>
        /// <returns>Constructed collection.</returns>
        public abstract object ToCollection(IEnumerable objects);

        /// <summary>
        /// Enumerates the items in the collection.  The input collection
        /// should be of the type described by this object.
        /// </summary>
        /// <param name="collection">Collection to enumerate.</param>
        /// <returns>The enumeration.</returns>
        public abstract IEnumerable ToEnumerable(object collection);

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="stringToParse">String to parse.</param>
        /// <returns>The parsed object.</returns>
        protected override object Parse(ArgumentParseContext context, string stringToParse)
        {
            string[] elementStrings;
            if (context.ElementSeparators.Any())
            {
                elementStrings = stringToParse.Split(
                    context.ElementSeparators.ToArray(),
                    StringSplitOptions.None);
            }
            else
            {
                elementStrings = new[] { stringToParse };
            }

            var parsedElements = elementStrings.Select(elementString =>
            {
                if (!_elementArgumentType.TryParse(context, elementString, out object parsedElement))
                {
                    throw new ArgumentOutOfRangeException(nameof(stringToParse));
                }

                return parsedElement;
            }).ToArray();

            return ToCollection(parsedElements);
        }

        /// <summary>
        /// Enumeration of all types that this type depends on / includes.
        /// </summary>
        public override IEnumerable<IArgumentType> DependentTypes => new[] { _elementArgumentType };

        /// <summary>
        /// Enumerates the objects contained within the provided collection;
        /// the collection must be of the type described by this object.
        /// </summary>
        /// <param name="collection">Collection to enumerate.</param>
        /// <returns>The enumerated objects.</returns>
        protected abstract IEnumerable<object> GetElements(object collection);

        /// <summary>
        /// Given a parser context, returns the preferred element separator. Returns null
        /// if no such separator exists.
        /// </summary>
        /// <param name="context">Parser context.</param>
        /// <returns>Preferred element separator, if one exists; null otherwise.</returns>
        private static string GetPreferredElementSeparatorOrDefault(ArgumentParseContext context)
        {
            var seps = context.ElementSeparators;
            return (seps.Count == 0) ? null : seps[0];
        }
    }
}
