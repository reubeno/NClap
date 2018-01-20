using System;
using System.Collections.Generic;

namespace NClap.Types
{
    /// <summary>
    /// Implementation to describe merged enum types.
    /// </summary>
    internal class MergedEnumArgumentType : EnumArgumentType
    {
        /// <summary>
        /// Constructs a type that merges the given enum types.
        /// </summary>
        /// <param name="types">Types to merge.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when zero types are
        /// provided.</exception>
        public MergedEnumArgumentType(IEnumerable<Type> types)
        {
            var typeCount = 0;
            foreach (var type in types)
            {
                AddValuesFromType(type);
                ++typeCount;
            }

            // We throw if 0 types are merged.
            if (typeCount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(types));
            }
        }
    }
}
