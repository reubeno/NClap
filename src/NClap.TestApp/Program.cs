using System;
using System.Collections.Generic;
using NClap.ConsoleInput;
using NClap.Metadata;
using NClap.Parser;
using NClap.Repl;
using NClap.Types;
using NClap.Utilities;

namespace NClap.TestApp
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
        SetPromptXy,

        [Verb(typeof(CliHelp))]
        CliHelp,

        [Verb(typeof(ExitVerb), HelpText = "Exits the loop")]
        Exit
    }

    class SetPrompt
    {
        [PositionalArgument(ArgumentFlags.Required)] public string Prompt { get; set; }
    }

    class CliHelp : SynchronousVerb
    {
        public override VerbResult Execute()
        {
            var info = CommandLineParser.GetUsageInfo(typeof(ProgramArguments), UsageInfoOptions.Default | UsageInfoOptions.CondenseOutput);
            CommandLineParser.DefaultReporter(info);
            return VerbResult.Success;
        }
    }

    [ArgumentSet(Style = ArgumentSetStyle.GetOpt)]
    class Here : SynchronousVerb
    {
        [NamedArgument(ArgumentFlags.AtMostOnce)] public int Hello { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)] public bool SomeBool { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)] public bool AnotherBool { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)] public VerbType SomeEnum { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)] public Tuple<int, VerbType, string> SomeTuple { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)] public KeyValuePair<int, VerbType> SomePair { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)] public KeyValuePair<int, List<VerbType>> SomeListPair { get; set; }

        [NamedArgument(ArgumentFlags.AtMostOnce)] public Uri SomeUri { get; set; }

        [PositionalArgument(ArgumentFlags.Required)] public FileSystemPath Path { get; set; }

        public override VerbResult Execute() => VerbResult.Success;
    }

    class SomeArgCompleter : IStringCompleter
    {
        public IEnumerable<string> GetCompletions(ArgumentCompletionContext context, string valueToComplete) =>
            new[] { "xyzzy", "fizzy" };
    }

    class HereToo : SynchronousVerb
    {
        [PositionalArgument(ArgumentFlags.Required, Completer = typeof(SomeArgCompleter))] public string SomeArg { get; set; }

        public override VerbResult Execute() => VerbResult.Success;
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

            var keyBindingSet = ConsoleKeyBindingSet.CreateDefaultSet();
            keyBindingSet.Bind('c', ConsoleModifiers.Control, ConsoleInputOperation.Abort);

            var parameters = new LoopInputOutputParameters
            {
                Prompt = new ColoredString("Loop> ", ConsoleColor.Cyan),
                KeyBindingSet = keyBindingSet
            };

            Loop<VerbType>.Execute(parameters, options);

            Console.WriteLine("Exited loop.");
        }
    }
}
