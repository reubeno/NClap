using System.Collections.Generic;

namespace NClap.Types
{
    /// <summary>
    /// Context for generating argument completions.
    /// </summary>
    public class ArgumentCompletionContext
    {
        /// <summary>
        /// The context that should be used if completion requires parsing.
        /// </summary>
        public ArgumentParseContext ParseContext { get; set; }

        /// <summary>
        /// The current tokenized state of the input.
        /// </summary>
        public IReadOnlyList<string> Tokens { get; set; }

        /// <summary>
        /// The zero-based index of the token being completed.
        /// </summary>
        public int TokenIndex { get; set; }

        /// <summary>
        /// If this completion is being generated for command-line arguments
        /// being parsed, and if this object is non-null, then it is the
        /// object that results from parsing and processing the tokens *before*
        /// the one being completed.
        /// </summary>
        public object InProgressParsedObject { get; set; }
    }
}
