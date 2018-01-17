using System.Diagnostics.CodeAnalysis;

namespace NClap.Types
{
    /// <summary>
    /// Interface implemented by objects that can parse strings.
    /// </summary>
    public interface IStringParser
    {
        /// <summary>
        /// Tries to parse the provided string, extracting a value of the type
        /// described by this interface.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="stringToParse">The string to parse.</param>
        /// <param name="value">On success, receives the parsed value; null
        /// otherwise.</param>
        /// <returns>True on success; false otherwise.</returns>
        bool TryParse(ArgumentParseContext context, string stringToParse, out object value);
    }
}
