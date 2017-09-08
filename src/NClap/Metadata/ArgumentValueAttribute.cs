using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NClap.Exceptions;

namespace NClap.Metadata
{
    /// <summary>
    /// Attribute for annotating values that can be used with arguments. It is
    /// most frequently used with values on enum types.
    /// </summary>
    [SuppressMessage("Performance", "CC0023:Unsealed Attribute")]
    [AttributeUsage(AttributeTargets.Field)]
    public class ArgumentValueAttribute : Attribute
    {
        private string _longName;

        /// <summary>
        /// Flags controlling the use of this value.
        /// </summary>
        public ArgumentValueFlags Flags { get; set; }

        /// <summary>
        /// The short name used to identify this value.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// The long name used to identify this value; null indicates that the
        /// "default" long name should be used.  The long name for every value
        /// in the containing type must unique.  It is an error to specify a
        /// long name of string.Empty.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public string LongName
        {
            get
            {
                var value = _longName;

                if ((value != null) && (value.Length == 0))
                {
                    throw new InvalidArgumentSetException(string.Format(
                        CultureInfo.CurrentCulture,
                        Strings.InvalidValueLongName));
                }

                return value;
            }

            set => _longName = value;
        }

        /// <summary>
        /// The help text for the argument.
        /// </summary>
        public string HelpText { get; set; }
    }
}
