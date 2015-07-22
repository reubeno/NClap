using System;
using System.Collections.Generic;
using System.Linq;

using NClap.ConsoleInput;

namespace NClap.Tests.ConsoleInput
{
    class SimulatedConsoleInput : IConsoleInput
    {
        private int _keyIndex;

        public SimulatedConsoleInput(IEnumerable<ConsoleKeyInfo> keyStream = null)
        {
            KeyStream = keyStream?.ToList() ?? new List<ConsoleKeyInfo>();
        }

        /// <summary>
        /// True if Control-C is treated as a normal input character; false if
        /// it's specially handled.
        /// </summary>
        public bool TreatControlCAsInput { get; set; }

        /// <summary>
        /// The key stream surfaced from this console.
        /// </summary>
        public IReadOnlyList<ConsoleKeyInfo> KeyStream { get; }

        /// <summary>
        /// Reads a key press from the console.
        /// </summary>
        /// <param name="suppressEcho">True to suppress auto-echoing the key's
        /// character; false to echo it as normal.</param>
        /// <returns>Info about the press.</returns>
        public ConsoleKeyInfo ReadKey(bool suppressEcho)
        {
            if (_keyIndex >= KeyStream.Count)
            {
                throw new InvalidOperationException("There are no more key events to read.");
            }

            return KeyStream[_keyIndex++];
        }
    }
}
