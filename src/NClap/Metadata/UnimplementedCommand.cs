using System;

namespace NClap.Metadata
{
    /// <summary>
    /// Simple stub command implementation that is not implemented.
    /// </summary>
    public class UnimplementedCommand : SynchronousCommand
    {
        /// <summary>
        /// Throws an exception.
        /// </summary>
        /// <returns>Does not return.</returns>
        public override CommandResult Execute()
        {
            throw new NotImplementedException("Executed unimplemented command.");
        }
    }
}
