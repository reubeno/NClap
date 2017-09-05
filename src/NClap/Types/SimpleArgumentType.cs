using System;
using System.Collections.Generic;

namespace NClap.Types
{
    /// <summary>
    /// Method that parses a string.
    /// </summary>
    /// <typeparam name="T">Argument type.</typeparam>
    /// <param name="stringToParse">String to parse.</param>
    /// <returns>The parsed object.</returns>
    delegate T SimpleArgumentTypeParseHandler<out T>(string stringToParse);

    /// <summary>
    /// Method that generates completions for a string.
    /// </summary>
    /// <param name="context">Context for completion.</param>
    /// <param name="valueToComplete">String to complete.</param>
    /// <returns>The possible completions.</returns>
    delegate IEnumerable<string> SimpleArgumentTypeCompletionHandler(ArgumentCompletionContext context, string valueToComplete);

    /// <summary>
    /// Basic implementation of IArgumentType, useful for describing built-in
    /// .NET types with simple semantics.
    /// </summary>
    class SimpleArgumentType : ArgumentTypeBase
    {
        private readonly SimpleArgumentTypeParseHandler<object> _parseHandler;
        private readonly SimpleArgumentTypeCompletionHandler _completionHandler;
        private readonly string _displayName;

        /// <summary>
        /// Constructs a new object to describe the provided type.
        /// </summary>
        /// <param name="type">Type to describe.</param>
        /// <param name="parseHandler">Delegate used to parse strings with the
        /// given type.</param>
        /// <param name="completionHandler">Delegate used to generate
        /// completions with the given type.</param>
        /// <param name="displayName">The type's human-readable name.</param>
        private SimpleArgumentType(Type type, SimpleArgumentTypeParseHandler<object> parseHandler, SimpleArgumentTypeCompletionHandler completionHandler, string displayName = null)
            : base(type)
        {
            _parseHandler = parseHandler;
            _completionHandler = completionHandler;
            _displayName = displayName;
        }

        /// <summary>
        /// Primary construction method.
        /// </summary>
        /// <typeparam name="T">Type to describe.</typeparam>
        /// <param name="parseHandler">Delegate used to parse strings with the
        /// given type.</param>
        /// <param name="completionHandler">Delegate used to complete strings with
        /// the given type.</param>
        /// <param name="displayName">The type's human-readable name.</param>
        /// <returns>The constructed object.</returns>
        public static SimpleArgumentType Create<T>(SimpleArgumentTypeParseHandler<T> parseHandler, SimpleArgumentTypeCompletionHandler completionHandler = null, string displayName = null) =>
            new SimpleArgumentType(typeof(T), s => parseHandler(s), completionHandler, displayName);

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
            return _completionHandler != null 
                ? _completionHandler(context, valueToComplete)
                : base.GetCompletions(context, valueToComplete);
        }

        /// <summary>
        /// Parses the provided string.  Throws an exception if the string
        /// cannot be parsed.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="stringToParse">String to parse.</param>
        /// <returns>The parsed object.</returns>
        protected override object Parse(ArgumentParseContext context, string stringToParse) =>
            _parseHandler(stringToParse);

        /// <summary>
        /// The type's human-readable (display) name.
        /// </summary>
        public override string DisplayName => _displayName ?? base.DisplayName;
    }
}
