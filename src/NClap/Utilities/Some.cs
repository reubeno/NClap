namespace NClap.Utilities
{
    /// <summary>
    /// Convenience class for constructing <see cref="Maybe{T}" /> values.
    /// </summary>
    internal static class Some
    {
        /// <summary>
        /// Constructs an object with a value present.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The constructed object.</returns>
        public static Maybe<T> Of<T>(T value) => new Maybe<T>(value);
    }
}
