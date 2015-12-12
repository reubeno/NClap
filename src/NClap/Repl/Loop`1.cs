namespace NClap.Repl
{
    /// <summary>
    /// Wrapper class for Loop&lt;TVerbType, TContext&gt;.
    /// </summary>
    /// <typeparam name="TVerbType">Enum type that defines possible verbs.
    /// </typeparam>
    public static class Loop<TVerbType> where TVerbType : struct
    {
        /// <summary>
        /// Executes the loop.
        /// </summary>
        public static void Execute() => Execute((LoopInputOutputParameters)null, null);

        /// <summary>
        /// Executes the loop.
        /// </summary>
        /// <param name="loopClient">Options for loop.</param>
        public static void Execute(ILoopClient loopClient) => Execute(loopClient, null);

        /// <summary>
        /// Executes the loop.
        /// </summary>
        /// <param name="options">Options for loop.</param>
        public static void Execute(LoopOptions options) => Execute((LoopInputOutputParameters)null, options);

        /// <summary>
        /// Executes the loop.
        /// </summary>
        /// <param name="loopClient">The client to use.</param>
        /// <param name="options">Options for loop.</param>
        public static void Execute(ILoopClient loopClient, LoopOptions options) =>
            Execute<object>(loopClient, options, null);

        /// <summary>
        /// Executes the loop.
        /// </summary>
        /// <param name="parameters">Optionally provides parameters controlling
        /// the loop's input and output behaviors; if not provided, default
        /// parameters are used.</param>
        /// <param name="options">Options for loop.</param>
        public static void Execute(LoopInputOutputParameters parameters, LoopOptions options) =>
            Execute<object>(parameters, options, null);

        /// <summary>
        /// Executes the loop.
        /// </summary>
        /// <param name="loopClient">The client to use.</param>
        /// <param name="options">Options for loop.</param>
        /// <param name="context">Context object for the loop.</param>
        public static void Execute<TContext>(ILoopClient loopClient, LoopOptions options, TContext context)
        {
            var loop = new Loop<TVerbType, TContext>(loopClient, options, context);
            loop.Execute();
        }

        /// <summary>
        /// Executes the loop.
        /// </summary>
        /// <param name="parameters">Optionally provides parameters controlling
        /// the loop's input and output behaviors; if not provided, default
        /// parameters are used.</param>
        /// <param name="options">Options for loop.</param>
        /// <param name="context">Context object for the loop.</param>
        public static void Execute<TContext>(LoopInputOutputParameters parameters, LoopOptions options, TContext context)
        {
            var loop = new Loop<TVerbType, TContext>(parameters, options, context);
            loop.Execute();
        }
    }
}