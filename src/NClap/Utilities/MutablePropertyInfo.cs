using System;
using System.Reflection;

namespace NClap.Utilities
{
    /// <summary>
    /// Implementation of IMutableFieldInfo for properties.
    /// </summary>
    internal class MutablePropertyInfo : IMutableMemberInfo
    {
        private readonly PropertyInfo _property;

        /// <summary>
        /// Constructs a new object from PropertyInfo.
        /// </summary>
        /// <param name="property">Info about the property.</param>
        public MutablePropertyInfo(PropertyInfo property)
        {
            _property = property;
        }

        /// <summary>
        /// Retrieve the member's base member info.
        /// </summary>
        public MemberInfo MemberInfo => _property;

        /// <summary>
        /// True if the member can be read at arbitrary points during execution;
        /// false otherwise.
        /// </summary>
        public bool IsReadable => _property.CanRead;

        /// <summary>
        /// True if the member can be written to at arbitrary points during
        /// execution; false otherwise.
        /// </summary>
        public bool IsWritable => _property.CanWrite;

        /// <summary>
        /// The type of the member.
        /// </summary>
        public Type MemberType => _property.PropertyType;

        /// <summary>
        /// Retrieve the value associated with this field in the specified
        /// containing object.
        /// </summary>
        /// <param name="containingObject">Object to look in.</param>
        /// <returns>The field's value.</returns>
        public object GetValue(object containingObject) => _property.GetValue(containingObject);

        /// <summary>
        /// Sets the value associated with this field in the specified
        /// containing object.
        /// </summary>
        /// <param name="containingObject">Object to look in.</param>
        /// <param name="value">Value to set.</param>
        public void SetValue(object containingObject, object value)
        {
            try
            {
                try
                {
                    _property.SetValue(containingObject, value);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }
            catch (ArgumentException)
            {
                object convertedValue;

                // Try to convert the value?
                if (!MemberType.TryConvertFrom(value, out convertedValue))
                {
                    throw;
                }

                try
                {
                    _property.SetValue(containingObject, convertedValue);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }
        }
    }
}
