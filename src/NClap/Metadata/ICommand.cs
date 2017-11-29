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
        /// Executes the command.
        /// </summary>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Result of execution.</returns>
        Task<CommandResult> ExecuteAsync(CancellationToken cancel);
    }
}
