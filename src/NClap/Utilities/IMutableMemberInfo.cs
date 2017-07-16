using System;
using System.Reflection;

namespace NClap.Utilities
{
    /// <summary>
    /// Abstract interface for interacting with mutable members of types
    /// (e.g. fields and properties).
    /// </summary>
    interface IMutableMemberInfo
    {
        /// <summary>
        /// Retrieve the member's base member info.
        /// </summary>
        MemberInfo MemberInfo { get; }

        /// <summary>
        /// True if the member can be read at arbitrary points during execution;
        /// false otherwise.
        /// </summary>
        bool IsReadable { get; }

        /// <summary>
        /// True if the member can be written to at arbitrary points during
        /// execution; false otherwise.
        /// </summary>
        bool IsWritable { get; }

        /// <summary>
        /// The type of the member.
        /// </summary>
        Type MemberType { get; }

        /// <summary>
        /// Retrieve the value associated with this field in the specified
        /// containing object.
        /// </summary>
        /// <param name="containingObject">Object to look in.</param>
        /// <returns>The field's value.</returns>
        object GetValue(object containingObject);

        /// <summary>
        /// Sets the value associated with this field in the specified
        /// containing object.
        /// </summary>
        /// <param name="containingObject">Object to look in.</param>
        /// <param name="value">Value to set.</param>
        void SetValue(object containingObject, object value);
    }
}
