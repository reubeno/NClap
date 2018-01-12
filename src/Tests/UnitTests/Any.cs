using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NClap.Tests
{
    /// <summary>
    /// Utility class for generating arbitrary test values.
    /// </summary>
    internal class Any
    {
        private static Random _random = new Random();

        /// <summary>
        /// Selects a negative integer.
        /// </summary>
        /// <returns>The selected integer.</returns>
        public static int NegativeInt() => Int(inclusiveMax: -1);

        /// <summary>
        /// Selects a non-negative integer.
        /// </summary>
        /// <returns>The selected integer.</returns>
        public static int NonNegativeInt() => Int(inclusiveMin: 0);

        /// <summary>
        /// Selects a positive integer.
        /// </summary>
        /// <returns>The selected integer.</returns>
        public static int PositiveInt() => Int(inclusiveMin: 1);

        /// <summary>
        /// Selects any integer in the given range.
        /// </summary>
        /// <param name="inclusiveMin">Optionally provides minimum (inclusive).</param>
        /// <param name="inclusiveMax">Optionally provides maximum (inclusive).</param>
        /// <returns>Selected integer.</returns>
        public static int Int(
            int? inclusiveMin = null,
            int? inclusiveMax = null)
        {
            if (inclusiveMin == null) inclusiveMin = int.MinValue;
            if (inclusiveMax == null) inclusiveMax = int.MaxValue - 1;
            return _random.Next(inclusiveMin.Value, inclusiveMax.Value + 1);
        }

        /// <summary>
        /// Selects any enum value from the given enum type.
        /// </summary>
        /// <typeparam name="T">enum type.</typeparam>
        /// <returns>The selected value.</returns>
        public static T Enum<T>() where T : struct
        {
            if (!typeof(T).GetTypeInfo().IsEnum)
            {
                throw new NotSupportedException($"Type {typeof(T).FullName} is not an enum");
            }

            return Of<T>(typeof(T).GetTypeInfo().GetEnumValues().Cast<T>());
        }

        public static T Of<T>(IEnumerable<T> candidates)
        {
            var candidatesList = candidates.ToList();
            if (candidatesList.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(candidates));
            }

            var index = Int(0, candidatesList.Count - 1);
            return candidatesList[index];
        }
    }
}
