namespace NClap.Utilities
{
    /// <summary>
    /// Abstract interface for interacting with a string builder.
    /// </summary>
    public interface IStringBuilder
    {
        /// <summary>
        /// Accesses the character at the specified index in the builder.
        /// </summary>
        /// <param name="index">0-based index into the builder.</param>
        /// <returns>The character at the specified index.</returns>
        char this[int index] { get; set; }

        /// <summary>
        /// Retrieves the current length of the builder's contents.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Copies the specified number of characters from the given starting
        /// index into the builder's current contents, writing the characters
        /// to the provided buffer (at the specified offset).
        /// </summary>
        /// <param name="startingIndex">0-based index at which to start reading
        /// from the builder.</param>
        /// <param name="buffer">Output buffer.</param>
        /// <param name="outputOffset">0-based index into the output buffer
        /// at which to start writing.</param>
        /// <param name="count">The number of characters to copy.</param>
        void CopyTo(int startingIndex, char[] buffer, int outputOffset, int count);

        /// <summary>
        /// Inserts the given character at the specified index.
        /// </summary>
        /// <param name="index">0-based index.</param>
        /// <param name="c">The character to insert.</param>
        void Insert(int index, char c);

        /// <summary>
        /// Inserts the given string at the specified index.
        /// </summary>
        /// <param name="index">0-based index.</param>
        /// <param name="s">The string to insert.</param>
        void Insert(int index, string s);

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
        /// Removes the specified number of characters from the given index into
        /// the builder's contents.
        /// </summary>
        /// <param name="index">0-based index.</param>
        /// <param name="count">The number of characters to remove.</param>
        void Remove(int index, int count);

        /// <summary>
        /// Clears the current contents of the builder.
        /// </summary>
        void Clear();

        /// <summary>
        /// Truncates the contents of the builder to the specified length.
        /// </summary>
        /// <param name="newLength">New length, expressed as a character
        /// count.</param>
        void Truncate(int newLength);

        /// <summary>
        /// Generates a composed string from the contents of this builder.
        /// </summary>
        /// <returns>The composed string.</returns>
        IString Generate();
    }
}
