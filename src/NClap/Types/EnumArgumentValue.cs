using System;
using System.Reflection;
using NClap.Metadata;
using NClap.Utilities;

namespace NClap.Types
{
    /// <summary>
    /// Implementation to describe enumeration values.
    /// </summary>
    internal class EnumArgumentValue : IArgumentValue
    {
        private readonly FieldInfo _fieldInfo;
        private readonly ArgumentValueAttribute _attribute;

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="fieldInfo">Information for the value.</param>
        public EnumArgumentValue(FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;

            try
            {
                Value = _fieldInfo.GetValue(null);
            }
            catch (InvalidOperationException)
            {
                Value = _fieldInfo.GetRawConstantValue();
            }

            if (Value == null)
            {
                throw new ArgumentOutOfRangeException(nameof(fieldInfo));
            }

            if (!TryGetArgumentValueAttribute(fieldInfo, out _attribute))
            {
                _attribute = new ArgumentValueAttribute();
            }
        }

        /// <summary>
        /// The value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// True if the value has been disallowed from parsing use; false
        /// otherwise.
        /// </summary>
        public bool Disallowed => _attribute.Flags.HasFlag(ArgumentValueFlags.Disallowed);

        /// <summary>
        /// True if the value has been marked to be hidden from help and usage
        /// information; false otherwise.
        /// </summary>
        public bool Hidden => _attribute.Flags.HasFlag(ArgumentValueFlags.Hidden);

        /// <summary>
        /// Display name for this value.
        /// </summary>
        public string DisplayName => _attribute.LongName ?? _fieldInfo.Name;

        /// <summary>
        /// Long name of this value.
        /// </summary>
        public string LongName => _attribute.LongName ?? _fieldInfo.Name.ToString();

        /// <summary>
        /// Short name of this value, if it has one; null if it has none.
        /// </summary>
        public string ShortName => _attribute.ShortName;

        /// <summary>
        /// Description of this value, if it has one; null if it has none.
        /// </summary>
        public string Description => _attribute.Description;

        /// <summary>
        /// Underlying field information.
        /// </summary>
        public FieldInfo ValueInfo => _fieldInfo;

        private static bool TryGetArgumentValueAttribute(FieldInfo fieldInfo, out ArgumentValueAttribute attribute)
        {
            // Look for an <see cref="ArgumentValueAttribute" /> attribute,
            // which might further customize how we can parse strings into
            // this value.
            attribute = fieldInfo.GetSingleAttribute<ArgumentValueAttribute>(inherit: false);
            return attribute != null;
        }
    }
}
