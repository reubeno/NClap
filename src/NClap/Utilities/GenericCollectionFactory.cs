using System;
using System.Collections;
using System.Collections.Generic;

namespace NClap.Utilities
{
    /// <summary>
    /// Factory for creating typed collections when types are not
    /// known at compile time.
    /// </summary>
    internal static class GenericCollectionFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="List{T}"/> that can
        /// hold instance of the given type <paramref name="t"/>.
        /// </summary>
        /// <param name="t">Type of instance.</param>
        /// <returns>Constructed list.</returns>
        public static IList CreateList(Type t)
        {
            var listType = typeof(List<>).MakeGenericType(new[] { t });
            return (IList)Activator.CreateInstance(listType);
        }

        /// <summary>
        /// Creates an array that may hold values of type <paramref name="t"/>
        /// with the values in given enumeration <paramref name="values"/>.
        /// </summary>
        /// <param name="values">Values to store.</param>
        /// <param name="t">Type for the array's elements.</param>
        /// <returns>The constructed and initialized array.</returns>
        public static Array ToArray(this IEnumerable values, Type t)
        {
            var list = CreateList(t);
            foreach (var value in values)
            {
                list.Add(value);
            }

            var array = Array.CreateInstance(t, list.Count);
            list.CopyTo(array, 0);

            return array;
        }
    }
}
