using System.Threading;
using System.Threading.Tasks;

namespace NClap.Metadata
{
    /// <summary>
    /// Represents a verb/command.
    /// </summary>
    public interface IVerb
    {
        /// <summary>
        /// Executes the verb.
        /// </summary>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Result of execution.</returns>
        Task<VerbResult> ExecuteAsync(CancellationToken cancel);
    }
}
