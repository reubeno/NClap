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

        public static ArgumentSetParseResult Ready { get; } =
            new ArgumentSetParseResult(ArgumentSetParseResultType.Ready);

        public static ArgumentSetParseResult UnknownNamedArgument(ArgumentNameType? namedArgType = null, string name = null) =>
            new ArgumentSetParseResult(ArgumentSetParseResultType.UnknownNamedArgument)
            {
                NamedArgType = namedArgType,
                NamedArg = name
            };

        public static ArgumentSetParseResult UnknownPositionalArgument { get; } =
            new ArgumentSetParseResult(ArgumentSetParseResultType.UnknownPositionalArgument);

        public static ArgumentSetParseResult FailedParsing { get; } =
            new ArgumentSetParseResult(ArgumentSetParseResultType.FailedParsing);

        public static ArgumentSetParseResult FailedFinalizing { get; } =
            new ArgumentSetParseResult(ArgumentSetParseResultType.FailedFinalizing);

        public static ArgumentSetParseResult InvalidAnswerFile { get; } =
            new ArgumentSetParseResult(ArgumentSetParseResultType.InvalidAnswerFile);

        public static ArgumentSetParseResult RequiresOptionArgument(ArgumentDefinition arg)
        {
            if (arg == null) throw new ArgumentNullException(nameof(arg));
            return new ArgumentSetParseResult(ArgumentSetParseResultType.RequiresOptionArgument)
            {
                Argument = arg
            };
        }

        public ArgumentSetParseResultType State { get; }

        public ArgumentDefinition Argument { get; private set; }

        public ArgumentNameType? NamedArgType { get; private set; }

        public string NamedArg { get; private set; }

        public bool IsReady => State == ArgumentSetParseResultType.Ready;

        public bool IsUnknown =>
            State == ArgumentSetParseResultType.UnknownNamedArgument ||
            State == ArgumentSetParseResultType.UnknownPositionalArgument;
    }
}