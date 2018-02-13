using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        /// Retrieves custom attributes associated with the given attribute provider.
        /// </summary>
        /// <typeparam name="T">Type of the attribute to retrieve.</typeparam>
        /// <param name="attributeProvider">Object to find attributes on.</param>
        /// <param name="inherit">Whether or not to inherit attributes from ancestor
        /// types.</param>
        /// <returns>The attributes.</returns>
        public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider attributeProvider, bool inherit = true)
        {
            Debug.Assert(attributeProvider != null);

            try
            {
                return attributeProvider.GetCustomAttributes(typeof(T), inherit).Cast<T>();
            }
            catch (InvalidOperationException)
            {
                IReadOnlyList<CustomAttributeData> attribs = null;
                if (attributeProvider is MemberInfo mi)
                {
                    attribs = mi.CustomAttributes.ToList();
                }
                else
                {
                    throw;
                }

                var matchingAttribs = attribs.Select(d =>
                {
                    var attribType = Type.GetType(d.AttributeType.FullName, /*throwIfNotFound=*/false, /*ignoreCase=*/false);
                    if (attribType == null)
                    {
                        return new None();
                    }

                    if (!typeof(T).GetTypeInfo().IsAssignableFrom(attribType))
                    {
                        return new None();
                    }

                    return Some.Of(new { Type = attribType, Data = d });
                }).WhereHasValue();

                return matchingAttribs.Select(attrib =>
                {
                    var constructorParamTypes = attrib.Data.Constructor
                        .GetParameters()
                        .Select(p => Type.GetType(p.ParameterType.FullName)).ToArray();
                    var constructorArgs = attrib.Data.ConstructorArguments
                        .Select(a => a.Value)
                        .ToArray();

                    var namedArgs = attrib.Data.NamedArguments
                        .Select(a => new { MemberName = a.MemberName, Value = a.TypedValue.Value });

                    var constructor = attrib.Type.GetTypeInfo().GetConstructor(constructorParamTypes);
                    var instance = constructor.Invoke(constructorArgs);

                    foreach (var arg in namedArgs)
                    {
                        var member = instance.GetType().GetTypeInfo().GetMember(arg.MemberName).First();
                        var mutableMember = member.ToMutableMemberInfo();

                        var value = arg.Value;
                        if (value is ReadOnlyCollection<CustomAttributeTypedArgument> collection &&
                            mutableMember.MemberType.IsArray)
                        {
                            var elementType = mutableMember.MemberType.GetElementType();
                            var array = Array.CreateInstance(elementType, collection.Count);
                            for (var i = 0; i < collection.Count; ++i)
                            {
                                array.SetValue(collection[i].Value, i);
                            }

                            value = array;
                        }

                        mutableMember.SetValue(instance, value);
                    }

                    return (T)instance;
                });
            }
        }

        /// <summary>
        /// Retrieves the sole attribute of the provided type that's associated
        /// with the specified attribute provider.  This can be used with
        /// methods, fields, classes, etc.  An exception is thrown if the
        /// object has multiple attributes of the specified type associated
        /// with it.
        /// </summary>
        /// <typeparam name="T">Type of the attribute to retrieve.</typeparam>
        /// <param name="attributeProvider">Object to find attributes on.</param>
        /// <param name="inherit">Whether or not to inherit attributes from ancestor
        /// types.</param>
        /// <returns>The only attribute of the specified type; the type's default
        /// value (e.g. null) if there are no such attributes present.</returns>
        public static T GetSingleAttribute<T>(this ICustomAttributeProvider attributeProvider, bool inherit = true) =>
            attributeProvider.GetAttributes<T>(inherit: inherit).SingleOrDefault();
    }
}
