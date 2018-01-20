using System.Collections.Generic;
using System.Linq;

namespace NClap.Utilities
{
    /// <summary>
    /// Extension methods for <see cref="Maybe{T}"/> objects.
    /// </summary>
    internal static class MaybeUtilities
    {
        /// <summary>
        /// From the given enumeration of <see cref="Maybe{T}"/> values, yield an enumeration
        /// with only the values present (and unwrap them from their <see cref="Maybe{T}"/>
        /// objects).
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="values">Input enumeration.</param>
        /// <returns>The resulting enumeration.</returns>
        public static IEnumerable<T> WhereHasValue<T>(this IEnumerable<Maybe<T>> values) =>
            values.Where(v => v.HasValue).Select(v => v.Value);
    }
}
