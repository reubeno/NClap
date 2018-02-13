using System;
using System.Collections.Generic;

namespace NClap.Types
{
    /// <summary>
    /// Interface for advertising a value as being parseable using this
    /// assembly.
    /// </summary>
    public interface IArgumentValue
    {
        /// <summary>
        /// The value.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// True if the value has been disallowed from parsing use; false
        /// otherwise.
        /// </summary>
        bool Disallowed { get; }

        /// <summary>
        /// True if the value has been marked to be hidden from help and usage
        /// information; false otherwise.
        /// </summary>
        bool Hidden { get; }

        /// <summary>
        /// Display name for this value.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Long name of this value.
        /// </summary>
        string LongName { get; }

        /// <summary>
        /// Short name of this value, if it has one; null if it has none.
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Description of this value, if it has one; null if it has none.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Get any attributes of the given type associated with the value.
        /// </summary>
        /// <typeparam name="T">Type of attribute to look for.</typeparam>
        /// <returns>The attributes.</returns>
        IEnumerable<T> GetAttributes<T>()
            where T : Attribute;
    }
}
