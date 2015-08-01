using System;
using System.Collections.Generic;
using NClap.Metadata;
using NClap.Utilities;

namespace NClap.Exceptions
{
    /// <summary>
    /// Exception thrown when an invalid argument set is used.
    /// </summary>
    [Serializable]
    public class InvalidArgumentSetException : Exception
    {
        private readonly string _innerMessage;

        /// <summary>
        /// Constructor that takes an <see cref="Metadata.Argument"/> object.
        /// </summary>
        /// <param name="arg">Info about the problematic argument.</param>
        /// <param name="message">The message string.</param>
        /// <param name="innerException">The inner exception.</param>
        internal InvalidArgumentSetException(Argument arg, string message = null, Exception innerException = null) : this(arg.Member, message, innerException)
        {
            Argument = arg;
        }

        /// <summary>
        /// Constructor that takes member info.
        /// </summary>
        /// <param name="memberInfo">Info about the problematic member.</param>
        /// <param name="message">The message string.</param>
        /// <param name="innerException">The inner exception.</param>
        internal InvalidArgumentSetException(IMutableMemberInfo memberInfo, string message = null, Exception innerException = null) : this(memberInfo.MemberInfo.DeclaringType, message, innerException)
        {
            MemberInfo = memberInfo;
        }

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        public InvalidArgumentSetException() : this((Type)null)
        {
        }

        /// <summary>
        /// Constructor that takes a message string.
        /// </summary>
        /// <param name="message">The message string.</param>
        public InvalidArgumentSetException(string message) : this(null, message)
        {
        }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="argumentSetType">The type of the argument set.</param>
        public InvalidArgumentSetException(Type argumentSetType) : this(argumentSetType, null)
        {
        }

        /// <summary>
        /// Constructor that takes the type of the argument set and a message
        /// string.
        /// </summary>
        /// <param name="argumentSetType">The type of the argument set.</param>
        /// <param name="message">The message string.</param>
        public InvalidArgumentSetException(Type argumentSetType, string message) : this(argumentSetType, message, null)
        {
        }

        /// <summary>
        /// Constructor that takes the type of the argument set, a message
        /// string and an inner exception.
        /// </summary>
        /// <param name="argumentSetType">The type of the argument set.</param>
        /// <param name="message">The message string.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidArgumentSetException(Type argumentSetType, string message, Exception innerException) : base(message, innerException)
        {
            ArgumentSetType = argumentSetType;
            _innerMessage = message;
        }

        /// <summary>
        /// If present, indicates the type of the problematic argument set.
        /// </summary>
        public Type ArgumentSetType { get; }

        /// <summary>
        /// If present, indicates the problematic argument.
        /// </summary>
        internal Argument Argument { get; }

        /// <summary>
        /// If present, indicates the problematic member of the argument set.
        /// </summary>
        internal IMutableMemberInfo MemberInfo { get; }

        /// <summary>
        /// The message to display.
        /// </summary>
        public override string Message
        {
            get
            {
                var summary = new List<string>();

                if (ArgumentSetType != null)
                {
                    summary.Add($"The type '{ArgumentSetType.FullName}' is not a valid argument set.");
                }

                if (MemberInfo != null)
                {
                    summary.Add($"Member '{MemberInfo.MemberInfo.Name}' is not supported as an argument or has invalid argument metadata.");
                }

                if (!string.IsNullOrEmpty(_innerMessage))
                {
                    summary.Add(
                        $"{Environment.NewLine}{Environment.NewLine}Details:{Environment.NewLine}    {_innerMessage}");
                }

                return string.Join(" ", summary);
            }
        }
    }
}
