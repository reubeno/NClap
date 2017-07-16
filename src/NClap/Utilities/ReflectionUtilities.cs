using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace NClap.Utilities
{
    /// <summary>
    /// Utilities useful in conjunction with .NET's reflection facilities.
    /// </summary>
    static class ReflectionUtilities
    {
        private const string ImplicitConversionMethodName = "op_Implicit";

        /// <summary>
        /// Retrieves the fields and properties from the provided type.
        /// </summary>
        /// <param name="type">Type to look in.</param>
        /// <param name="bindingFlags">Binding flags to use during search.
        /// </param>
        /// <returns>The fields and properties.</returns>
        public static IEnumerable<IMutableMemberInfo> GetFieldsAndProperties(this Type type, BindingFlags bindingFlags)
        {
            return type.GetTypeInfo().GetFields(bindingFlags).Select(field => new MutableFieldInfo(field)).Cast<IMutableMemberInfo>().Concat(
                type.GetTypeInfo().GetProperties(bindingFlags).Select(property => new MutablePropertyInfo(property)));
        }

        /// <summary>
        /// Constructs an instance of the provided type's default value.
        /// </summary>
        /// <param name="type">Type to get default for.</param>
        /// <returns>The type's default value.</returns>
        public static object GetDefaultValue(this Type type) =>
            typeof(ReflectionUtilities).GetTypeInfo()
                                       .GetMethod("GetDefaultValue", Array.Empty<Type>())
                                       .MakeGenericMethod(type).Invoke(null, null);

        /// <summary>
        /// Wraps default(T) into a method.
        /// </summary>
        /// <typeparam name="T">Type to get default value for.</typeparam>
        /// <returns>The type's default value.</returns>
        public static T GetDefaultValue<T>() => default(T);

        /// <summary>
        /// Checks if the specified type is implicitly convertible from the
        /// specified value.
        /// </summary>
        /// <param name="destType">Destination type.</param>
        /// <param name="sourceValue">Source value.</param>
        /// <returns>True if the type is implicitly convertible; false
        /// otherwise.</returns>
        public static bool IsImplicitlyConvertibleFrom(this Type destType, object sourceValue)
        {
            if (sourceValue != null)
            {
                var method = destType.GetTypeInfo().GetMethod(ImplicitConversionMethodName, new[] {sourceValue.GetType()});
                if ((method != null) &&
                    method.IsStatic &&
                    (method.ReturnType == destType))
                {
                    return true;
                }

                method = sourceValue.GetType().GetTypeInfo().GetMethod(ImplicitConversionMethodName, new[] {sourceValue.GetType()});
                if ((method != null) &&
                    method.IsStatic &&
                    (method.ReturnType == destType))
                {
                    return true;
                }
            }

            try
            {
                var convertedValue = Convert.ChangeType(sourceValue, destType, CultureInfo.InvariantCulture);
                return true;
            }
            catch (InvalidCastException)
            {
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }

            return false;
        }

        /// <summary>
        /// Tries to convert the specified value to the specified type.
        /// </summary>
        /// <param name="destType">The destination type.</param>
        /// <param name="sourceValue">The value to convert.</param>
        /// <param name="convertedValue">On success, receives the converted
        /// value.</param>
        /// <returns>True if the conversion succeeded; false otherwise.
        /// </returns>
        public static bool TryConvertFrom(this Type destType, object sourceValue, out object convertedValue)
        {
            if (sourceValue != null)
            {
                var implicitConversionMethod = destType.GetTypeInfo()
                    .GetMethod(ImplicitConversionMethodName, new[] { sourceValue.GetType() });

                if ((implicitConversionMethod != null) &&
                    implicitConversionMethod.IsStatic &&
                    implicitConversionMethod.ReturnType == destType)
                {
                    try
                    {
                        convertedValue = implicitConversionMethod.Invoke(null, new[] { sourceValue });
                        return true;
                    }
                    catch (TargetInvocationException)
                    {
                    }
                }

                implicitConversionMethod = sourceValue.GetType().GetTypeInfo()
                    .GetMethod(ImplicitConversionMethodName, new[] { sourceValue.GetType() });

                if ((implicitConversionMethod != null) &&
                    implicitConversionMethod.IsStatic &&
                    implicitConversionMethod.ReturnType == destType)
                {
                    try
                    {
                        convertedValue = implicitConversionMethod.Invoke(null, new [] { sourceValue });
                        return true;
                    }
                    catch (TargetInvocationException)
                    {
                    }
                }
            }

            try
            {
                convertedValue = Convert.ChangeType(sourceValue, destType, CultureInfo.InvariantCulture);
                return true;
            }
            catch (InvalidCastException)
            {
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }

            convertedValue = null;
            return false;
        }
    }
}
