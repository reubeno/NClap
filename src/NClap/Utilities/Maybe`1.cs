using System;

namespace NClap.Utilities
{
    /// <summary>
    /// A maybe type. Represents a value or a lack of value.
    /// </summary>
    /// <typeparam name="T">Type of inner values.</typeparam>
    internal struct Maybe<T>
    {
        private readonly T _value;

        /// <summary>
        /// Constructs an instance with a value present.
        /// </summary>
        /// <param name="value">Value present; may be null.</param>
        public Maybe(T value)
        {
            HasValue = true;
            _value = value;
        }

        /// <summary>
        /// Implicit operator that allows coercing from a value to an object
        /// that wraps it.
        /// </summary>
        /// <param name="value">Value to wrap.</param>
        public static implicit operator Maybe<T>(T value) => new Maybe<T>(value);

        /// <summary>
        /// Implicit operator that allows coercing from a <see cref="None"/>
        /// object.
        /// </summary>
        /// <param name="none">The given none object.</param>
#pragma warning disable CA1801 // Parameter is never used
        public static implicit operator Maybe<T>(None none) => default(Maybe<T>);
#pragma warning restore CA1801

        /// <summary>
        /// Indicates whether or not a value is present in this object.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// Indicates whether or not a value is *not* present in this object.
        /// </summary>
        public bool IsNone => !HasValue;

        /// <summary>
        /// Retrieves the value present in this object. An exception is thrown if
        /// no value is present.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no value is present
        /// </exception>.
        public T Value => HasValue ? _value : throw new InvalidOperationException();

        /// <summary>
        /// Tries to retrieve the value present in this object; if no value is
        /// present, then the indicated default value is returned.
        /// </summary>
        /// <param name="defaultValue">Default value to return when this object
        /// does not hold a value.</param>
        /// <returns>The computed value.</returns>
        public T GetValueOrDefault(T defaultValue = default(T)) =>
            HasValue ? _value : defaultValue;
    }
}
