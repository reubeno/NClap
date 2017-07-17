using System;
using System.Collections.Generic;
using System.Linq;
using NClap.Parser;
using NClap.Utilities;

namespace NClap.ConsoleInput
{
    /// <summary>
    /// Encapsulates a cache of token completions.
    /// </summary>
    internal class TokenCompletionSet
    {
        private TokenCompletionSet(string inputText, Token originalToken, IReadOnlyList<string> completions)
        {
            Completions = completions;
            InputText = inputText;
            OriginalToken = originalToken;
        }

        /// <summary>
        /// The input buffer's contents at the time when the completion set was
        /// generated.
        /// </summary>
        public string InputText { get; }

        /// <summary>
        /// The completions.
        /// </summary>
        public IReadOnlyList<string> Completions { get; }

        /// <summary>
        /// The uncompleted token.
        /// </summary>
        public Token OriginalToken { get; }

        /// <summary>
        /// Count of completions.
        /// </summary>
        public int Count => Completions.Count;

        /// <summary>
        /// True if the set is empty; false if it contains at least one
        /// completion.
        /// </summary>
        public bool Empty => Completions.Count == 0;

        /// <summary>
        /// Retrieves the specified completion from the set.
        /// </summary>
        /// <param name="index">The index of the completion to retrieve.</param>
        /// <returns>The specified completion.</returns>
        public string this[int index] => Completions[index];

        /// <summary>
        /// Generate completions for the "current" token in the specified input
        /// text.
        /// </summary>
        /// <param name="inputText">The input text string.</param>
        /// <param name="cursorIndex">The current cursor index into the string.
        /// </param>
        /// <param name="completionHandler">Completion handler to invoke.
        /// </param>
        /// <returns>The generated completion set.</returns>
        public static TokenCompletionSet Create(string inputText, int cursorIndex, ConsoleCompletionHandler completionHandler)
        {
            if (completionHandler == null)
            {
                throw new ArgumentNullException(nameof(completionHandler));
            }

            var completions = Create(inputText, cursorIndex, completionHandler, out int tokenStartIndex, out int tokenLength);
            var originalToken = new Token(new Substring(inputText, tokenStartIndex, tokenLength));
            return new TokenCompletionSet(inputText, originalToken, completions);
        }

        /// <summary>
        /// Generate completions for the "current" token in the specified input
        /// text.
        /// </summary>
        /// <param name="inputText">The input text string.</param>
        /// <param name="cursorIndex">The current cursor index into the string.
        /// </param>
        /// <param name="completionHandler">Completion handler to invoke.
        /// </param>
        /// <param name="existingTokenStartIndex">Receives the start index of
        /// the current token.</param>
        /// <param name="existingTokenLength">Receives the length of the current
        /// token.</param>
        /// <returns>The generated completions.</returns>
        private static IReadOnlyList<string> Create(string inputText, int cursorIndex, ConsoleCompletionHandler completionHandler, out int existingTokenStartIndex, out int existingTokenLength)
        {
            //
            // Try to parse the line.  If we fail to parse it, then just
            // return immediately.
            //

            var tokens = CommandLineParser.Tokenize(
                inputText,
                CommandLineTokenizerOptions.AllowPartialInput).ToList();

            //
            // Figure out which token we're in
            //

            int tokenIndex;
            for (tokenIndex = 0; tokenIndex < tokens.Count; ++tokenIndex)
            {
                var token = tokens[tokenIndex];
                if (cursorIndex > token.OuterEndingOffset)
                {
                    continue;
                }

                if (cursorIndex >= token.OuterStartingOffset)
                {
                    break;
                }

                // Insert an empty token here.
                tokens.Insert(
                    tokenIndex,
                    new Token(new Substring(inputText, cursorIndex, 0)));

                break;
            }

            if (tokenIndex < tokens.Count)
            {
                var token = tokens[tokenIndex];

                existingTokenStartIndex = token.OuterStartingOffset;
                existingTokenLength = token.OuterLength;
            }
            else
            {
                existingTokenStartIndex = cursorIndex;
                existingTokenLength = 0;
            }

            //
            // Ask for completions.
            //

            var tokenStrings = tokens.Select(token => RemoveQuotes(token.ToString())).ToArray();

            var completions = completionHandler.Invoke(tokenStrings, tokenIndex).ToList();

            // If necessary quote!
            for (var j = 0; j < completions.Count; j++)
            {
                var completion = completions[j];
                if (!completion.StartsWith("\"", StringComparison.OrdinalIgnoreCase))
                {
                    completions[j] = StringUtilities.QuoteIfNeeded(completions[j]);
                }
            }

            return completions;
        }

        /// <summary>
        /// Returns a copy of the provided string with all quotes removed.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <returns>The resulting processed string.</returns>
        private static string RemoveQuotes(string s) => s.Replace("\"", string.Empty);
    }
}
