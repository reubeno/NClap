using System.Collections.Generic;

namespace NClap.Types
{
    /// <summary>
    /// Implementation to describe System.Boolean.
    /// </summary>
    class BoolArgumentType : ArgumentTypeBase
    {
        private static readonly BoolArgumentType s_instance = new BoolArgumentType();

        /// <summary>
        /// Primary constructor.
        /// </summary>
        private BoolArgumentType() : base(typeof(bool))
        {
        }

        /// <summary>
        /// The type's human-readable (display) name.
        /// </summary>
        public override string DisplayName => "bool";

        /// <summary>
        /// Public factory method.
        /// </summary>
        /// <returns>A constructed object.</returns>
        public static BoolArgumentType Create() => s_instance;
        
        /// <summary>
        /// Generates a set of valid strings--parseable to this type--that
        /// contain the provided string as a strict prefix.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="valueToComplete">The string to complete.</param>
        /// <returns>An enumeration of a set of completion strings; if no such
        /// strings could be generated, or if the type doesn't support
        /// completion, then an empty enumeration is returned.</returns>
        public override IEnumerable<string> GetCompletions(ArgumentCompletionContext context, string valueToComplete) =>
            SelectCompletions(context, valueToComplete, new object[] { false, true });

        /// <summary>
        /// Parses the provided string.  Throws an exception if the string
        /// cannot be parsed.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="stringToParse">String to parse.</param>
        /// <returns>The parsed object.</returns>
        protected override object Parse(ArgumentParseContext context, string stringToParse)
        {
            if (string.IsNullOrEmpty(stringToParse))
            {
                return true;
            }

            switch (stringToParse)
            {
                case "+":
                    return true;
                case "-":
                    return false;
                default:
                    return bool.Parse(stringToParse);
            }
        }
    }
}
