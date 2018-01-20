using System;
using System.Collections.Generic;
using NClap.ConsoleInput;
using NClap.Help;
using NClap.Parser;
using NClap.Utilities;

namespace NClap.Metadata
{
    /// <summary>
    /// Static class useful for configuring the behavior of help commands.
    /// </summary>
    public static class HelpCommand
    {
        /// <summary>
        /// The default options to use for generate help.
        /// </summary>
        public static ArgumentSetHelpOptions DefaultHelpOptions { get; set; } =
            new ArgumentSetHelpOptions
            {
                Logo = new ArgumentMetadataHelpOptions { Include = false },
                Name = string.Empty
            };

        /// <summary>
        /// The output handler function for this class.
        /// </summary>
        public static Action<ColoredMultistring> OutputHandler { get; set; }
    }
}
