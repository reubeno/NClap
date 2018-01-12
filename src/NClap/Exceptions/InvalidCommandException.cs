using System;
using System.Reflection;

namespace NClap.Exceptions
{
    /// <summary>
    /// Exception thrown when an invalid command is used.
    /// </summary>
    public class InvalidCommandException : ArgumentException
    {
        private readonly Type _commandType;
        private readonly MemberInfo _commandValue;

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        public InvalidCommandException() : this(null, null, null, null)
        {
        }

        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// <param name="commandType">Command type.</param>
        /// <param name="commandValue">Command value.</param>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        public InvalidCommandException(Type commandType, MemberInfo commandValue, string message, Exception innerException) : base(message, innerException)
        {
            _commandType = commandType;
            _commandValue = commandValue;
        }

        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// <param name="commandType">Command type.</param>
        /// <param name="commandValue">Command value.</param>
        /// <param name="message">Message.</param>
        public InvalidCommandException(Type commandType, MemberInfo commandValue, string message) : this(commandType, commandValue, message, null)
        {
        }

        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        public InvalidCommandException(string message, Exception innerException) : this(null, null, message, innerException)
        {
        }
        
        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// <param name="message">Message.</param>
        public InvalidCommandException(string message) : this(null, null, message, null)
        {
        }
    }
}
