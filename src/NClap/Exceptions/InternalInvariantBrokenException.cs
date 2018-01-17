using System;

namespace NClap.Exceptions
{
    /// <summary>
    /// Exception thrown when an internal invariant is broken; if this is
    /// thrown, then there is a code defect in this library.
    /// </summary>
    public sealed class InternalInvariantBrokenException : Exception
    {
        /// <summary>
        /// Standard parameterless constructor.
        /// </summary>
        public InternalInvariantBrokenException()
        {
        }

        /// <summary>
        /// Standard constructor that takes a string message.
        /// </summary>
        /// <param name="message">Message.</param>
        public InternalInvariantBrokenException(string message) : base(message)
        {
        }

        /// <summary>
        /// Standard constructor that wraps an inner exception.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception to wrap.</param>
        public InternalInvariantBrokenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
