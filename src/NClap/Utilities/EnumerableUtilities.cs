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
            bool firstValue = true;
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
        /// <returns></returns>
        public static bool Overlaps<T>(this IEnumerable<T> values, IEnumerable<T> otherValues, IEqualityComparer<T> comparer = null)
        {
            var valuesSet = new HashSet<T>(values, comparer);
            return otherValues.Any(v => valuesSet.Contains(v));
        }

        /// <summary>
        /// Retrieves the last element of the given list.
        /// </summary>
        /// <typeparam name="T">The list element type.</typeparam>
        /// <param name="values">The list.</param>
        /// <returns>The last element.</returns>
        public static T GetLast<T>(this IReadOnlyList<T> values) => values[values.Count - 1];
    }
}
