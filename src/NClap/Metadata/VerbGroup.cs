using System;
using System.Threading;
using System.Threading.Tasks;

namespace NClap.Metadata
{
    /// <summary>
    /// Represents a group of verbs, i.e. a verb with sub-verbs.
    /// </summary>
    public class VerbGroup<TVerbType> : Verb, IArgumentProvider where TVerbType : struct
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="selectedVerbType">The selected verb type.</param>
        /// <param name="selectedVerb">The selected verb.</param>
        public VerbGroup(TVerbType selectedVerbType, IVerb selectedVerb)
        {
            SelectedVerbType = selectedVerbType;
            SelectedVerb = selectedVerb;
        }

        /// <summary>
        /// True if the group has a selection, false if no selection was yet
        /// made.
        /// </summary>
        public bool HasSelection => SelectedVerb != null;

        /// <summary>
        /// The enum value corresponding with the selected verb, or null if no
        /// selection has yet been made (and if there is no default).
        /// </summary>
        public TVerbType? SelectedVerbType { get; set; }

        /// <summary>
        /// The verb presently selected from this group, or null if no
        /// selection has yet been made (and if there is no default).
        /// </summary>
        public IVerb SelectedVerb { get; set; }

        /// <summary>
        /// Retrieve info for the object type that defines the arguments to be
        /// parsed.
        /// </summary>
        /// <returns>The defining type.</returns>
        public Type GetTypeDefiningArguments() => SelectedVerb.GetType();

        /// <summary>
        /// Retrieve a reference to the object into which parsed arguments
        /// should be stored.
        /// </summary>
        /// <returns>The object in question.</returns>
        public object GetDestinationObject() => SelectedVerb;
        
        /// <summary>
        /// Executes the verb.
        /// </summary>
        /// <param name="cancel">Cancellation token.</param>
        /// <returns>Result of execution.</returns>
        public override Task<VerbResult> ExecuteAsync(CancellationToken cancel)
        {
            if (SelectedVerb == null)
            {
                return Task.FromResult(VerbResult.UsageError);
            }

            return SelectedVerb.ExecuteAsync(cancel);
        }
    }
}
