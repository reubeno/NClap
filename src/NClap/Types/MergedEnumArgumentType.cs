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
        public MergedEnumArgumentType(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                AddValuesFromType(type);
            }
        }
    }
}
