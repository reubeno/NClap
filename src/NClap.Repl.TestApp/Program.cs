using System;
using System.Collections.Generic;
using NClap.Metadata;
using NClap.Parser;
using NClap.Types;
using NClap.Utilities;

namespace NClap.Repl.TestApp
{
    enum VerbType
    {
        [HelpVerb(HelpText = "Displays verb help")]
        Help,

        [Verb(typeof(Here), HelpText = "Simple here command")]
        Here,

        [Verb(typeof(HereToo), HelpText = "Here too, right?")]
        HereToo,

        [Verb(typeof(SetPrompt))]
        SetPrompt,

        [Verb(Exits = true, HelpText = "Exits the loop")]
        Exit
    }

    class SetPrompt
    {
        [PositionalArgument(ArgumentFlags.Required)] public string Prompt { get; set; }
    }

    class Here : IVerb
    {
        [NamedArgument(ArgumentFlags.AtMostOnce)] public int Hello { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)] public bool SomeBool { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)] public VerbType SomeEnum { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)] public Tuple<int, VerbType, string> SomeTuple { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)] public KeyValuePair<int, VerbType> SomePair { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)] public KeyValuePair<int, List<VerbType>> SomeListPair { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)] public Uri SomeUri { get; set; }

        [PositionalArgument(ArgumentFlags.Required)] public FileSystemPath Path { get; set; }

        public void Execute(object o)
        {
        }
    }

    class SomeArgCompleter : IStringCompleter
    {
        public IEnumerable<string> GetCompletions(ArgumentCompletionContext context, string valueToComplete) =>
            new[] { "xyzzy", "fizzy" };
    }

    class HereToo : IVerb
    {
        [PositionalArgument(ArgumentFlags.Required, Completer = typeof(SomeArgCompleter))] public string SomeArg { get; set; }

        public void Execute(object o)
        {
        }
    }

    class Program
    {
        private static int Main(string[] args)
        {
            var programArgs = new ProgramArguments();

            if (!CommandLineParser.ParseWithUsage(args, programArgs))
            {
                return -1;
            }

            RunInteractively();

            return 0;
        }

        private static void RunInteractively()
        {
            Console.WriteLine("Entering loop.");

            var options = new LoopOptions
            {
                EndOfLineCommentCharacter = '#'
            };

            var parameters = new LoopInputOutputParameters
            {
                Prompt = new ColoredString("Loop> ", ConsoleColor.Cyan)
            };

            Loop<VerbType>.Execute(parameters, options);

            Console.WriteLine("Exited loop.");
        }
    }
}
