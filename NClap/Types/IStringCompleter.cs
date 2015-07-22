using System.Collections.Generic;

namespace NClap.Types
{
    /// <summary>
    /// Interface implemented by objects that can generate completions for a
    /// given string.
    /// </summary>
    public interface IStringCompleter
    {
        /// <summary>
        /// Generates a set of valid strings--parseable to this type--that
        /// contain the provided string as a strict prefix.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="valueToComplete">The string to complete.</param>
        /// <returns>An enumeration of a set of completion strings; if no such
        /// strings could be generated, or if the type doesn't support
        /// completion, then an empty enumeration is returned.</returns>
        IEnumerable<string> GetCompletions(ArgumentCompletionContext context, string valueToComplete);
    }
}
