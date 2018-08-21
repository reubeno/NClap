using System;
using System.Collections.Generic;
using System.Linq;
using NClap.Repl;
using NClap.Types;

namespace NClap.Metadata
{
    /// <summary>
    /// Helper class used by <see cref="HelpCommand{TCommandType}"/>.
    /// </summary>
    internal class HelpCommandArgumentCompleter : IStringCompleter
    {
        private readonly Loop _loop;

        /// <summary>
        /// Constructs a new completer for the given loop.
        /// </summary>
        /// <param name="loop">Loop to generate completions for.</param>
        public HelpCommandArgumentCompleter(Loop loop)
        {
            _loop = loop ?? throw new ArgumentNullException(nameof(loop));
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetCompletions(ArgumentCompletionContext context, string valueToComplete)
        {
            // We get the entire command line here, including the token that triggered
            // the help command to be invoked.  Any options to the help command would
            // also be present, which would pose a problem in the future if we add
            // more options to the help command.
            const int tokensToSkip = 1;
            return _loop.GetCompletions(context.Tokens.Skip(tokensToSkip), context.TokenIndex - tokensToSkip);
        }
    }
}
