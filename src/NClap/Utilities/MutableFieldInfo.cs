using System;
using System.Reflection;

namespace NClap.Utilities
{
    /// <summary>
    /// Implementation of IMutableFieldInfo for fields.
    /// </summary>
    internal class MutableFieldInfo : IMutableMemberInfo
    {
        private readonly FieldInfo _field;

        /// <summary>
        /// Constructs a new object from FieldInfo.
        /// </summary>
        /// <param name="field">Info about the field.</param>
        public MutableFieldInfo(FieldInfo field)
        {
            _field = field;
        }

        /// <summary>
        /// Retrieve the member's base member info.
        /// </summary>
        public MemberInfo MemberInfo => _field;

        /// <summary>
        /// True if the member can be read at arbitrary points during execution;
        /// false otherwise.
        /// </summary>
        public bool IsReadable => true;

        /// <summary>
        /// True if the member can be written to at arbitrary points during
        /// execution; false otherwise.
        /// </summary>
        public bool IsWritable => !_field.IsInitOnly && !_field.IsLiteral;

        /// <summary>
        /// The type of the member.
        /// </summary>
        public Type MemberType => _field.FieldType;

        /// <summary>
        /// Retrieve the value associated with this field in the specified
        /// containing object.
        /// </summary>
        /// <param name="containingObject">Object to look in.</param>
        /// <returns>The field's value.</returns>
        public object GetValue(object containingObject) => _field.GetValue(containingObject);

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
                _field.SetValue(containingObject, value);
            }
            catch (ArgumentException)
            {
                // Try to convert the value?
                if (!MemberType.TryConvertFrom(value, out object convertedValue))
                {
                    throw;
                }

                _field.SetValue(containingObject, convertedValue);
            }
        }
    }
}
