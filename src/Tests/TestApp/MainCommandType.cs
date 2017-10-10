using NClap.Metadata;

namespace NClap.TestApp
{
    [ArgumentType(DisplayName = "REPL command")]
    enum MainCommandType
    {
        [HelpCommand(Description = "Displays command help")]
        Help,

        [Command(typeof(CliHelp), Description = "Displays toplevel help")]
        CliHelp,

        [Command(typeof(CompleteCommand), Description = "Useful only for completing")]
        Complete,

        [Command(typeof(LogoCommand), Description = "Display 'logo'")]
        Logo,

        [Command(typeof(ReadLineCommand), ShortName = "readl", Description = "Reads a line of input")]
        ReadLine,

        [Command(typeof(ReplCommand), Description = "Starts interactive loop")]
        Repl,

        [Command(typeof(CommandGroup<SubCommandType>), Description = "Test sub-command group")]
        SubCommand,

        [Command(typeof(ExitCommand), Description = "Exits the loop")]
        Exit
    }
}
