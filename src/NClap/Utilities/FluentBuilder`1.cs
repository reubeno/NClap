using System;
using System.Collections.Generic;

namespace NClap.Utilities
{
    /// <summary>
    /// Utility class for fluent builders that manipulate state of the given type.
    /// </summary>
    /// <typeparam name="TState">Type of the state.</typeparam>
    public class FluentBuilder<TState>
    {
        private readonly TState _startingState;
        private readonly List<Action<TState>> _transformers = new List<Action<TState>>();

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="startingState">The starting state.</param>
        public FluentBuilder(TState startingState)
        {
            _startingState = startingState;
        }

        // CA1062: Validate arguments of public methods
#pragma warning disable CA1062
        /// <summary>
        /// Operator that allows implicit casting from a builder to its applied result.
        /// </summary>
        /// <param name="builder">Fluent builder to apply and generate state from.</param>
        public static implicit operator TState(FluentBuilder<TState> builder) => builder.Apply();
#pragma warning restore CA1062

        /// <summary>
        /// Operator that allows implicitly forming a fluent builder from a starting
        /// state.
        /// </summary>
        /// <param name="state">Input state object to create a fluent builder from.</param>
        public static implicit operator FluentBuilder<TState>(TState state) =>
            new FluentBuilder<TState>(state);

        /// <summary>
        /// Appends a new transformer function.
        /// </summary>
        /// <param name="transformer">Function.</param>
        public void AddTransformer(Action<TState> transformer) =>
            _transformers.Add(transformer);

        /// <summary>
        /// Applies all accumulated transformers, producing a final state.
        /// </summary>
        /// <returns>The final transformed state.</returns>
        public TState Apply()
        {
            var state = _startingState;
            foreach (var transformer in _transformers)
            {
                transformer(state);
            }

            return state;
        }
    }
}
