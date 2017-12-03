using System.Collections.Generic;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Abstract interface for an object that can generate completions for a console
    /// input token.
    /// </summary>
    public interface ITokenCompleter
    {
        /// <summary>
        /// Retrieves the completions for the given token, in the context of the given
        /// set of tokens.
        /// </summary>
        /// <param name="tokens">The current set of tokens.</param>
        /// <param name="tokenIndex">The 0-based index of the token to get completions
        /// for.</param>
        /// <returns>The enumeration of completions for the token.</returns>
        IEnumerable<string> GetCompletions(IEnumerable<string> tokens, int tokenIndex);
    }
}
