namespace NClap.Types
{
    /// <summary>
    /// Interface implemented by objects that can convert objects into strings.
    /// </summary>
    public interface IObjectFormatter
    {
        /// <summary>
        /// Converts a value into a readable string form.
        /// </summary>
        /// <param name="value">The value to format into a string.</param>
        /// <returns>The formatted string.</returns>
        string Format(object value);
    }
}
