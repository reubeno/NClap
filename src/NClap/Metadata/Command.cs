using System.Threading;
using System.Threading.Tasks;

namespace NClap.Metadata
{
    /// <summary>
    /// Base class for implementing commands.
    /// </summary>
    [ArgumentType(DisplayName = "command")]
    public abstract class Command : ICommand
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Result of execution.</returns>
        public virtual Task<CommandResult> ExecuteAsync(CancellationToken cancel) =>
            Task.FromResult(CommandResult.UsageError);
    }
}
