namespace NClap.Utilities
{
    /// <summary>
    /// Abstract interface for interacting with a string builder.
    /// </summary>
    public interface IStringBuilder
    {
        /// <summary>
        /// Appends a new string to the end of this builder's current content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        void Append(IString s);

        /// <summary>
        /// Appends a new string to the end of this builder's current content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        void Append(string s);

        /// <summary>
        /// Appends the specified character to the end of this builder's current
        /// content, repeated the indicated number of times.
        /// </summary>
        /// <param name="c">The char to append.</param>
        /// <param name="count">The number of times to append it.</param>
        void Append(char c, int count);

        /// <summary>
        /// Generates a composed string from the contents of this builder.
        /// </summary>
        /// <returns>The composed string.</returns>
        IString Generate();
    }
}
