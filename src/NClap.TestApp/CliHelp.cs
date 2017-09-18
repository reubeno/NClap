﻿using NClap.Metadata;
using NClap.Parser;

namespace NClap.TestApp
{
    class CliHelp : SynchronousVerb
    {
        public override VerbResult Execute()
        {
            var info = CommandLineParser.GetUsageInfo(typeof(ProgramArguments), UsageInfoOptions.Default);
            CommandLineParser.DefaultReporter(info);
            return VerbResult.Success;
        }
    }
}