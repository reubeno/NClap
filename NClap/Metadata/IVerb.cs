namespace NClap.Metadata
{
    /// <summary>
    /// Represents a verb/command in the REPL.
    /// </summary>
    /// <typeparam name="TContext">Type of the context object passed into this
    /// class's methods.</typeparam>
    public interface IVerb<in TContext>
    {
        /// <summary>
        /// Executes the verb.
        /// </summary>
        /// <param name="context">The context for the verb.</param>
        void Execute(TContext context);
    }

    /// <summary>
    /// Wrapper for verbs that take no context.
    /// </summary>
    public interface IVerb : IVerb<object>
    {
    }
}
