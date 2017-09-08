using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using NClap.Utilities;

namespace NClap.Types
{
    /// <summary>
    /// Abstract base class for internal implementations of the IArgumentType
    /// interface.
    /// </summary>
    internal abstract class ArgumentTypeBase : IArgumentType
    {
        /// <summary>
        /// Constructor for use by derived classes.
        /// </summary>
        /// <param name="type">Type described by this object.</param>
        protected ArgumentTypeBase(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// The type's human-readable (display) name.
        /// </summary>
        public virtual string DisplayName => Type.Name.ToSnakeCase();

        /// <summary>
        /// The Type object associated with values described by this interface.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type { get; }

        /// <summary>
        /// A summary of the concrete syntax required to indicate a value of
        /// the type described by this interface (e.g. "&gt;Int32&lt;").
        /// </summary>
        public virtual string SyntaxSummary => string.Format(CultureInfo.CurrentCulture, "<{0}>", DisplayName);

        /// <summary>
        /// Tries to parse the provided string, extracting a value of the type
        /// described by this interface.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="stringToParse">The string to parse.</param>
        /// <param name="value">On success, receives the parsed value; null
        /// otherwise.</param>
        /// <returns>True on success; false otherwise.</returns>
        [SuppressMessage("Design", "CC0004:Catch block cannot be empty")]
        public bool TryParse(ArgumentParseContext context, string stringToParse, out object value)
        {
            if (stringToParse == null)
            {
                throw new ArgumentNullException(nameof(stringToParse));
            }

            try
            {
                value = Parse(context, stringToParse);
                return true;
            }
            catch (OverflowException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (FormatException)
            {
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Converts a value into a readable string form.  The value must be of
        /// the type described by this interface.
        /// </summary>
        /// <param name="value">The value to format into a string.</param>
        /// <returns>The formatted string.</returns>
        public virtual string Format(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if ((value.GetType() != Type) &&
                !Type.TryConvertFrom(value, out object convertedValue))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return value.ToString();
        }

        /// <summary>
        /// Filters the provided candidate string completion list based on the
        /// provided string to complete.
        /// </summary>
        /// <param name="context">Context for completion.</param>
        /// <param name="valueToComplete">String to complete.</param>
        /// <param name="candidates">Candidate strings to select from.</param>
        /// <returns>An enumeration of the selected strings.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1801:ReviewUnusedParameters", MessageId = "context")]
        [SuppressMessage("Usage", "CC0057:Unused parameters")]
        protected static IEnumerable<string> SelectCompletions(ArgumentCompletionContext context, string valueToComplete, IEnumerable<string> candidates) =>
            candidates.Where(name => name.StartsWith(valueToComplete, StringComparison.OrdinalIgnoreCase))
                      .OrderBy(name => name, StringComparer.OrdinalIgnoreCase);

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
            // By default, return an empty enumeration of strings.
            Enumerable.Empty<string>();

        /// <summary>
        /// Enumeration of all types that this type depends on / includes.
        /// </summary>
        public virtual IEnumerable<IArgumentType> DependentTypes =>
            Enumerable.Empty<IArgumentType>();

        /// <summary>
        /// Parses the provided string.  Throws an exception if the string
        /// cannot be parsed.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="stringToParse">String to parse.</param>
        /// <returns>The parsed object.</returns>
        protected abstract object Parse(ArgumentParseContext context, string stringToParse);

        /// <summary>
        /// Filters the provided candidate string completion list based on the
        /// provided string to complete.
        /// </summary>
        /// <param name="context">Context for completion.</param>
        /// <param name="valueToComplete">String to complete.</param>
        /// <param name="candidates">Candidate objects whose formatted strings
        /// should be selected from.</param>
        /// <returns>An enumeration of the selected strings.</returns>
        protected static IEnumerable<string> SelectCompletions(ArgumentCompletionContext context, string valueToComplete, IEnumerable<object> candidates) =>
            SelectCompletions(context, valueToComplete, candidates.Select(candidate => candidate.ToString()));
    }
}
