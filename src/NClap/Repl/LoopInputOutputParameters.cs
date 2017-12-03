using NClap.ConsoleInput;
using NClap.Utilities;

namespace NClap.Repl
{
    /// <summary>
    /// Parameters for constructing a loop with advanced line input.  The
    /// parameters indicate how the loop's textual input and output should
    /// be implemented.
    /// </summary>
    public class LoopInputOutputParameters
    {
        /// <summary>
        /// Line input object to use, or null for a default one to be
        /// constructed.
        /// </summary>
        public IConsoleLineInput LineInput { get; set; }

        /// <summary>
        /// The console input interface to use, or null to use the default one.
        /// </summary>
        public IConsoleInput ConsoleInput { get; set; }

        /// <summary>
        /// The console output interface to use, or null to use the default one.
        /// </summary>
        public IConsoleOutput ConsoleOutput { get; set; }

        /// <summary>
        /// The console key binding set to use, or null to use the default one.
        /// </summary>
        public IReadOnlyConsoleKeyBindingSet KeyBindingSet { get; set; }

        /// <summary>
        /// Input prompt, or null to use the default one.
        /// </summary>
        public ColoredString? Prompt { get; set; }

        /// <summary>
        /// The character that starts a comment.
        /// </summary>
        public char? EndOfLineCommentCharacter { get; set; }
    }
}
