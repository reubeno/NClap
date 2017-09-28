using System;
using System.Reflection;

namespace NClap.Utilities
{
    /// <summary>
    /// Extension methods for manipulating <see cref="Type"/> objects.
    /// </summary>
    internal static class TypeUtilities
    {
        /// <summary>
        /// Checks if two types are effectively the same.
        /// </summary>
        /// <param name="type">First type.</param>
        /// <param name="otherType">Second type.</param>
        /// <returns>True if the two types are effectively the same.</returns>
        public static bool IsEffectivelySameAs(this Type type, Type otherType) =>
            type.GetTypeInfo().GUID == otherType.GetTypeInfo().GUID &&
            type.AssemblyQualifiedName.Equals(otherType.AssemblyQualifiedName, StringComparison.Ordinal);
    }
}
