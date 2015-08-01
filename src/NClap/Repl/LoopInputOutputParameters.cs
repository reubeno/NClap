using System.IO;
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
        /// Writer to use for error output.
        /// </summary>
        public TextWriter ErrorWriter { get; set; }

        /// <summary>
        /// Line input object to use.
        /// </summary>
        public IConsoleLineInput LineInput { get; set; }

        /// <summary>
        /// The console input interface to use.
        /// </summary>
        public IConsoleInput ConsoleInput { get; set; }

        /// <summary>
        /// The console output interface to use.
        /// </summary>
        public IConsoleOutput ConsoleOutput { get; set; }

        /// <summary>
        /// Input prompt.
        /// </summary>
        public ColoredString Prompt { get; set; }
    }
}
