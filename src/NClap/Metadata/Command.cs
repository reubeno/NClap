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
        /// The parent command under which this child is nested, or null if this
        /// command has no parent command.
        /// </summary>
        public ICommand Parent { get; set; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Result of execution.</returns>
        public virtual Task<CommandResult> ExecuteAsync(CancellationToken cancel) =>
            Task.FromResult(CommandResult.UsageError);
    }
}
