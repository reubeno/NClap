using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NClap.Utilities
{
    /// <summary>
    /// Assorted utilities for manipulating <see cref="IEnumerable"/> objects.
    /// </summary>
    internal static class EnumerableUtilities
    {
        /// <summary>
        /// Produce a new enumeration consisting of all elements in the provided
        /// enumeration, with the given value copied between each adjacent pair.
        /// </summary>
        /// <typeparam name="T">The type of element in the enumeration.
        /// </typeparam>
        /// <param name="values">The enumeration.</param>
        /// <param name="valueToInsert">The value to insert.</param>
        /// <returns>The resulting enumeration.</returns>
        public static IEnumerable<T> InsertBetween<T>(this IEnumerable<T> values, T valueToInsert)
        {
            var firstValue = true;
            foreach (var v in values)
            {
                if (!firstValue)
                {
                    yield return valueToInsert;
                }

                yield return v;

                firstValue = false;
            }
        }

        /// <summary>
        /// Flattens the provided enumeration of enumerations.
        /// </summary>
        /// <typeparam name="T">The innermost element type.</typeparam>
        /// <param name="values">The enumeration to flatten.</param>
        /// <returns>The resulting enumeration.</returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> values) =>
            values.SelectMany(v => v);

        /// <summary>
        /// Determines if the two provided enumerations contain any members in
        /// common.
        /// </summary>
        /// <typeparam name="T">The element type of the enumerations.</typeparam>
        /// <param name="values">The first enumeration.</param>
        /// <param name="otherValues">The second enumeration.</param>
        /// <param name="comparer">The comparer implementation to use, or null
        /// to use the default.</param>
        /// <returns>true if the they contain any members in common; false otherwise.</returns>
        public static bool Overlaps<T>(this IEnumerable<T> values, IEnumerable<T> otherValues, IEqualityComparer<T> comparer = null) =>
            values.Intersect(otherValues, comparer).Any();

        /// <summary>
        /// Retrieves the last element of the given list.
        /// </summary>
        /// <typeparam name="T">The list element type.</typeparam>
        /// <param name="values">The list.</param>
        /// <returns>The last element.</returns>
        public static T GetLast<T>(this IReadOnlyList<T> values) => values[values.Count - 1];

        /// <summary>
        /// Retrieves the last element of the given list, if one exists;
        /// otherwise returns null.
        /// </summary>
        /// <typeparam name="T">The list element type.</typeparam>
        /// <param name="values">The list.</param>
        /// <returns>The last element, or null.</returns>
        public static T GetLastOrDefault<T>(this IReadOnlyList<T> values) =>
            values.Count == 0 ? default(T) : values[values.Count - 1];
    }
}
