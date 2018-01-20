using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace NClap.Types
{
    /// <summary>
    /// Base abstract class, intended for use by custom object types that
    /// wish to implement the IArgumentType interface to describe themselves.
    /// </summary>
    public abstract class CustomArgumentTypeBase : IArgumentType
    {
        /// <summary>
        /// The Type object associated with values described by this interface.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "[Legacy]")]
        public Type Type => GetType();

        /// <summary>
        /// The type's human-readable (display) name.
        /// </summary>
        public string DisplayName => Type.Name;

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
        public abstract bool TryParse(ArgumentParseContext context, string stringToParse, out object value);

        /// <summary>
        /// Converts a value into a readable string form.  The value must be of
        /// the type described by this interface.
        /// </summary>
        /// <param name="value">The value to format into a string.</param>
        /// <returns>The formatted string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/>
        /// is null.</exception>
        public virtual string Format(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (value.GetType() != Type)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
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
        public virtual IEnumerable<string> GetCompletions(ArgumentCompletionContext context, string valueToComplete)
        {
            // By default, return an empty enumeration of strings.
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Enumeration of all types that this type depends on / includes.
        /// </summary>
        public virtual IEnumerable<IArgumentType> DependentTypes => Enumerable.Empty<IArgumentType>();
    }
}
