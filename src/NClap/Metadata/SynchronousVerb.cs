using System.Threading;
using System.Threading.Tasks;

namespace NClap.Metadata
{
    /// <summary>
    /// Base class for implementing synchronously executing verbs.
    /// </summary>
    public abstract class SynchronousVerb : IVerb
    {
        /// <summary>
        /// Executes the verb.
        /// </summary>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Result of execution.</returns>
        public Task<VerbResult> ExecuteAsync(CancellationToken cancel) =>
            Task.Run(() => Execute(), cancel);

        /// <summary>
        /// Executes the verb.
        /// </summary>
        /// <returns>Result of execution.</returns>
        public abstract VerbResult Execute();
    }
}
