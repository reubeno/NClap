using System.Threading;
using System.Threading.Tasks;

namespace NClap.Metadata
{
    /// <summary>
    /// Base class for implementing synchronously executing commands.
    /// </summary>
    public abstract class SynchronousCommand : Command
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Result of execution.</returns>
        public override Task<CommandResult> ExecuteAsync(CancellationToken cancel) =>
            Task.Run(() => Execute(), cancel);

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>Result of execution.</returns>
        public abstract CommandResult Execute();
    }
}
