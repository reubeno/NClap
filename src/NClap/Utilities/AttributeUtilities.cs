using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace NClap.Utilities
{
    /// <summary>
    /// Utilities for retrieving and manipulating custom attributes from
    /// .NET reflection objects.
    /// </summary>
    internal static class AttributeUtilities
    {
        /// <summary>
        /// Retrieves the sole attribute of the provided type that's associated
        /// with the specified attribute provider.  This can be used with
        /// methods, fields, classes, etc.  An exception is thrown if the
        /// object has multiple attributes of the specified type associated
        /// with it.
        /// </summary>
        /// <typeparam name="T">Type of the attribute to retrieve.</typeparam>
        /// <param name="attributeProvider">Object to find attributes on.</param>
        /// <returns>The only attribute of the specified type; the type's default
        /// value (e.g. null) if there are no such attributes present.</returns>
        public static T GetSingleAttribute<T>(this ICustomAttributeProvider attributeProvider)
        {
            Contract.Requires(attributeProvider != null);
            return attributeProvider.GetCustomAttributes(typeof(T), true).Cast<T>().SingleOrDefault();
        }
    }
}
