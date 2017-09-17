using System.Threading;
using System.Threading.Tasks;

namespace NClap.Metadata
{
    /// <summary>
    /// Base class for implementing verbs.
    /// </summary>
    [ArgumentType(DisplayName = "command")]
    public abstract class Verb : IVerb
    {
        /// <summary>
        /// Executes the verb.
        /// </summary>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Result of execution.</returns>
        public virtual Task<VerbResult> ExecuteAsync(CancellationToken cancel) =>
            Task.FromResult(VerbResult.UsageError);
    }
}
