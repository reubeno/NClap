using System.Threading;
using System.Threading.Tasks;

namespace NClap.Metadata
{
    /// <summary>
    /// Represents a command (a.k.a. verb).
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// The parent command under which this child is nested, or null if this
        /// command has no parent command.
        /// </summary>
        ICommand Parent { get; set; }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Result of execution.</returns>
        Task<CommandResult> ExecuteAsync(CancellationToken cancel);
    }
}
