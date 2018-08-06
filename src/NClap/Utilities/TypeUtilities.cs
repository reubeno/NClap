using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Tries to retrieve a parameterless constructor for the given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The constructor if found, or null if not found.</returns>
        public static ConstructorInfo GetParameterlessConstructor(this Type type) =>
            type.GetTypeInfo().GetConstructor(Array.Empty<Type>());

        /// <summary>
        /// Retrieves a function that invokes the constructor of the given type that takes 0 or more
        /// of the possible arguments provided.
        /// </summary>
        /// <typeparam name="T">Required return value type.</typeparam>
        /// <param name="type">Type of the constructor.</param>
        /// <param name="possibleArgs">Possible arguments.</param>
        /// <param name="considerParameterlessConstructor">True to consider
        /// parameterless constructors; false to ignore them.</param>
        /// <returns>The wrapped function.</returns>
        public static Func<T> GetConstructor<T>(this Type type, IEnumerable<object> possibleArgs, bool considerParameterlessConstructor)
            where T : class
        {
            // Make sure types are compatible, first and foremost.
            if (!typeof(T).GetTypeInfo().IsAssignableFrom(type))
            {
                throw new NotSupportedException($"Type '{type.FullName}' does not implement destination type {typeof(T).FullName}");
            }

            // Compute a map of all types that we might support in the constructor.
            Dictionary<Type, object> objectMap = possibleArgs.SelectMany(a =>
                    GetAllUsableTypes(a).Select(usableType => new { Type = usableType, Value = a }))
                .ToDictionary(item => item.Type, item => item.Value);

            var allConstructors = type.GetTypeInfo().GetConstructors();
            var compatibleConstructors = allConstructors.Where(c =>
            {
                var constructorParams = c.GetParameters();

                if (constructorParams.Length == 0)
                {
                    return considerParameterlessConstructor;
                }

                // Otherwise, we need to have an object to pass to each parameter.
                return constructorParams.All(p => objectMap.ContainsKey(p.ParameterType));
            }).ToList();

            if (compatibleConstructors.Count == 0)
            {
                throw new NotSupportedException(
                    $"Type '{type.FullName}' does not contain any compatible constructors; " + Environment.NewLine +
                    $"  types of possible arguments include: {{{string.Join(", ", possibleArgs.Select(a => a.GetType().Name))}}}" + Environment.NewLine +
                    $"  types of constructors: {string.Join(", ", allConstructors.Select(c => c.ToString()))}");
            }
            else if (compatibleConstructors.Count > 1)
            {
                throw new NotSupportedException($"Type '{type.FullName}' contains multiple compatible constructors");
            }

            var constructor = compatibleConstructors.Single();
            var constructorArgs = constructor.GetParameters().Select(p => objectMap[p.ParameterType]).ToArray();

            return () => (T)constructor.Invoke(constructorArgs);
        }

        private static IEnumerable<Type> GetAllUsableTypes(object value)
        {
            var ty = value.GetType();
            return new[] { ty }.Concat(ty.GetTypeInfo().GetInterfaces());
        }
    }
}
