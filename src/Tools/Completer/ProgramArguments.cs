using System.Collections.Generic;
using NClap.Metadata;
using NClap.Types;

namespace NClap.Completer
{
    [ArgumentSet(Style = ArgumentSetStyle.PowerShell)]
    class ProgramArguments
    {
        [NamedArgument(ArgumentFlags.Required, LongName = "Assembly")]
        public FileSystemPath AssemblyPath { get; set; }

        [NamedArgument(ArgumentFlags.Required, LongName = "Type")]
        public string TypeName { get; set; }

        [NamedArgument(ArgumentFlags.Required, LongName = "ArgIndex")]
        public int IndexOfArgToComplete { get; set; }

        [NamedArgument(ArgumentFlags.RestOfLine, LongName = "Args")]
        public List<string> Arguments { get; set; } = new List<string>();
    }
}
