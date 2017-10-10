using System;
using NClap.Metadata;
using NClap.Types;

namespace NClap.TestApp
{
    class CompleteCommand : SynchronousCommand
    {
        [NamedArgument]
        public FileSystemPath Path { get; set; }

        [NamedArgument]
        public int Integer { get; set; }

        [NamedArgument]
        public string String { get; set; }

        [NamedArgument]
        public Guid Guid { get; set; }

        [NamedArgument]
        public bool Boolean { get; set; }

        public override CommandResult Execute()
        {
            return CommandResult.Success;
        }
    }
}
