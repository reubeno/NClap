using System;

namespace NClap.Parser
{
    /// <summary>
    /// Encapsulates the result of parsing an argument set.
    /// </summary>
    internal class ArgumentSetParseResult
    {
        private ArgumentSetParseResult(ArgumentSetParseResultType state)
        {
            State = state;
        }

        /// <summary>
        /// A singleton result for cases where parser is ready to parse more arguments.
        /// </summary>
        public static ArgumentSetParseResult Ready { get; } =
            new ArgumentSetParseResult(ArgumentSetParseResultType.Ready);

        /// <summary>
        /// Constructs a result for cases where parser has encountered an unknown named
        /// argument.
        /// </summary>
        /// <param name="namedArgType">The type of named argument encountered, if available.</param>
        /// <param name="name">The name encountered, if available.</param>
        /// <returns>The result object.</returns>
        public static ArgumentSetParseResult UnknownNamedArgument(ArgumentNameType? namedArgType = null, string name = null) =>
            new ArgumentSetParseResult(ArgumentSetParseResultType.UnknownNamedArgument)
            {
                NamedArgType = namedArgType,
                NamedArg = name
            };

        /// <summary>
        /// A singleton result for cases where parser has encountered an unknown positional
        /// argument.
        /// </summary>
        public static ArgumentSetParseResult UnknownPositionalArgument { get; } =
            new ArgumentSetParseResult(ArgumentSetParseResultType.UnknownPositionalArgument);

        /// <summary>
        /// A singleton result for cases where parser has generically failed parsing.
        /// </summary>
        public static ArgumentSetParseResult FailedParsing { get; } =
            new ArgumentSetParseResult(ArgumentSetParseResultType.FailedParsing);

        /// <summary>
        /// A singleton result for cases where parser has failed to finalize an argument
        /// set.
        /// </summary>
        public static ArgumentSetParseResult FailedFinalizing { get; } =
            new ArgumentSetParseResult(ArgumentSetParseResultType.FailedFinalizing);

        /// <summary>
        /// A singleton result for cases where parser has encountered an invalid answer
        /// file.
        /// </summary>
        public static ArgumentSetParseResult InvalidAnswerFile { get; } =
            new ArgumentSetParseResult(ArgumentSetParseResultType.InvalidAnswerFile);

        /// <summary>
        /// A singleton result for cases where parser is next looking to see an argument
        /// to an option.
        /// </summary>
        public static ArgumentSetParseResult RequiresOptionArgument(ArgumentDefinition arg)
        {
            if (arg == null) throw new ArgumentNullException(nameof(arg));
            return new ArgumentSetParseResult(ArgumentSetParseResultType.RequiresOptionArgument)
            {
                Argument = arg
            };
        }

        /// <summary>
        /// State of the parse result.
        /// </summary>
        public ArgumentSetParseResultType State { get; }

        /// <summary>
        /// Argument referred to by the state. Not applicable to all states.
        /// </summary>
        public ArgumentDefinition Argument { get; private set; }

        /// <summary>
        /// Named argument type referred to by the state. Not applicable to all states.
        /// </summary>
        public ArgumentNameType? NamedArgType { get; private set; }

        /// <summary>
        /// Name referred to by the state. Not applicable to all states.
        /// </summary>
        public string NamedArg { get; private set; }

        /// <summary>
        /// Convenience property for checking if the parser is ready to parse more.
        /// </summary>
        public bool IsReady => State == ArgumentSetParseResultType.Ready;

        /// <summary>
        /// Convenience property for checking if the parser has encountered an unknown
        /// argument.
        /// </summary>
        public bool IsUnknown =>
            State == ArgumentSetParseResultType.UnknownNamedArgument ||
            State == ArgumentSetParseResultType.UnknownPositionalArgument;
    }
}