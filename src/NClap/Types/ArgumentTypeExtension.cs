using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NClap.Types
{
    /// <summary>
    /// Base class for extending the argument semantics of an existing
    /// implementation of the IArgumentType interface.
    /// </summary>
    public class ArgumentTypeExtension : IArgumentType
    {
        // CA1051: Do not declare visible instance fields
#pragma warning disable CA1051

        /// <summary>
        /// Optional override of the string parser implementation of the base
        /// argument type.
        /// </summary>
        protected readonly IStringParser Parser;

        /// <summary>
        /// Optional override of the object formatter implementation of the base
        /// argument type.
        /// </summary>
        protected readonly IObjectFormatter Formatter;

        /// <summary>
        /// Optional override of the string completer implementation of the
        /// base argument type.
        /// </summary>
        protected readonly IStringCompleter Completer;

#pragma warning restore CA1051

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The primitive object type to find a corresponding
        /// IArgumentType implementation for (e.g. System.String).</param>
        /// <param name="parser">Optionally provides an override implementation
        /// of the base argument type's string parser implementation.</param>
        /// <param name="formatter">Optionally provides an override
        /// implementation of the base argument type's object formatter
        /// implementation.</param>
        /// <param name="completer">Optionally provides an override
        /// implementation of the base argument type's string completer
        /// implementation.</param>
        public ArgumentTypeExtension(Type type, IStringParser parser = null, IObjectFormatter formatter = null, IStringCompleter completer = null)
            : this(ArgumentType.GetType(type), parser, formatter, completer)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="innerType">The base IArgumentType instance to
        /// wrap and extend.</param>
        /// <param name="parser">Optionally provides an override implementation
        /// of the base argument type's string parser implementation.</param>
        /// <param name="formatter">Optionally provides an override
        /// implementation of the base argument type's object formatter
        /// implementation.</param>
        /// <param name="completer">Optionally provides an override
        /// implementation of the base argument type's string completer
        /// implementation.</param>
        public ArgumentTypeExtension(IArgumentType innerType, IStringParser parser = null, IObjectFormatter formatter = null, IStringCompleter completer = null)
        {
            InnerType = innerType;
            Parser = parser;
            Formatter = formatter;
            Completer = completer;
        }

        /// <summary>
        /// The base IArgumentType object to wrap and extend.
        /// </summary>
        public IArgumentType InnerType { get; }

        /// <summary>
        /// The type's human-readable (display) name.
        /// </summary>
        public virtual string DisplayName => InnerType.DisplayName;

        /// <summary>
        /// The Type object associated with values described by this interface.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "[Legacy]")]
        public virtual Type Type => InnerType.Type;

        /// <summary>
        /// A summary of the concrete syntax required to indicate a value of
        /// the type described by this interface (e.g. "&gt;Int32&lt;").
        /// </summary>
        public virtual string SyntaxSummary => InnerType.SyntaxSummary;

        /// <summary>
        /// Tries to parse the provided string, extracting a value of the type
        /// described by this interface.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="stringToParse">The string to parse.</param>
        /// <param name="value">On success, receives the parsed value; null
        /// otherwise.</param>
        /// <returns>True on success; false otherwise.</returns>
        public virtual bool TryParse(ArgumentParseContext context, string stringToParse, out object value) =>
            (Parser ?? InnerType).TryParse(context, stringToParse, out value);

        /// <summary>
        /// Converts a value into a readable string form.  The value must be of
        /// the type described by this interface.
        /// </summary>
        /// <param name="value">The value to format into a string.</param>
        /// <returns>The formatted string.</returns>
        public virtual string Format(object value) =>
            (Formatter ?? InnerType).Format(value);

        /// <summary>
        /// Generates a set of valid strings--parseable to this type--that
        /// contain the provided string as a strict prefix.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="valueToComplete">The string to complete.</param>
        /// <returns>An enumeration of a set of completion strings; if no such
        /// strings could be generated, or if the type doesn't support
        /// completion, then an empty enumeration is returned.</returns>
        public virtual IEnumerable<string> GetCompletions(ArgumentCompletionContext context, string valueToComplete) =>
            (Completer ?? InnerType).GetCompletions(context, valueToComplete);

        /// <summary>
        /// Enumeration of all types that this type depends on / includes.
        /// </summary>
        public virtual IEnumerable<IArgumentType> DependentTypes => InnerType.DependentTypes;
    }
}
