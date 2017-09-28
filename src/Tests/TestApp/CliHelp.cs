using NClap.Metadata;

namespace NClap.TestApp
{
    class CliHelp : SynchronousCommand
    {
        public override CommandResult Execute()
        {
            var info = CommandLineParser.GetUsageInfo(typeof(ProgramArguments), UsageInfoOptions.Default);
            CommandLineParser.DefaultReporter(info);
            return CommandResult.Success;
        }
    }
}
