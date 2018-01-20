using System;
using System.Collections.Generic;

namespace NClap.Utilities
{
    /// <summary>
    /// Utilities for interacting with circular enumerators.
    /// </summary>
    internal static class CircularEnumerator
    {
        /// <summary>
        /// Creates a new circular enumerator.
        /// </summary>
        /// <typeparam name="T">Type of the item in the enumerated list.
        /// </typeparam>
        /// <param name="values">List to be enumerated.</param>
        /// <returns>The enumerator.</returns>
        public static CircularEnumerator<T> Create<T>(IReadOnlyList<T> values) =>
            new CircularEnumerator<T>(values);
    }
}
