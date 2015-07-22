using System;

namespace NClap.Utilities
{
    /// <summary>
    /// Represents a token from a token.
    /// </summary>
    public struct Token : IEquatable<Token>
    {
        /// <summary>
        /// Constructs a token object.
        /// </summary>
        /// <param name="contents">The token's contents.</param>
        public Token(Substring contents) : this(contents, false, false)
        {
        }

        /// <summary>
        /// Constructs a token object.
        /// </summary>
        /// <param name="contents">The token's contents.</param>
        /// <param name="startsWithQuote">True if the token is immediately
        /// preceded by an opening quote; false otherwise.</param>
        /// <param name="endsWithQuote">True if the token is immediately
        /// succeeded by an ending quote; false otherwise.</param>
        public Token(Substring contents, bool startsWithQuote, bool endsWithQuote)
        {
            Contents = contents;
            StartsWithQuote = startsWithQuote;
            EndsWithQuote = endsWithQuote;
        }

        /// <summary>
        /// The (unquoted) contents of the token.
        /// </summary>
        public Substring Contents { get; }

        /// <summary>
        /// True if the token starts with a quote; false otherwise.
        /// </summary>
        public bool StartsWithQuote { get; }

        /// <summary>
        /// True if the token ends with a quote; false otherwise.
        /// </summary>
        public bool EndsWithQuote { get; }

        /// <summary>
        /// Starting offset of the token.  If the token starts with quotes,
        /// the offset of the first character after the starting quotes is
        /// returned.
        /// </summary>
        public int InnerStartingOffset => Contents.StartingOffset;

        /// <summary>
        /// Ending offset of the token.  If the token ends with quotes, the
        /// ending offset of the last character before the ending quotes is
        /// returned.
        /// </summary>
        public int InnerEndingOffset => Contents.EndingOffset;

        /// <summary>
        /// Length of the token, excluding any quotes.
        /// </summary>
        public int InnerLength => Contents.Length;

        /// <summary>
        /// Starting offset of the token.  If the token starts with quotes,
        /// the starting offset of the starting quotes is returned.
        /// </summary>
        public int OuterStartingOffset => Contents.StartingOffset - (StartsWithQuote ? 1 : 0);

        /// <summary>
        /// Ending offset of the token.  If the token ends with quotes,
        /// the ending offset of the ending quotes is returned.
        /// </summary>
        public int OuterEndingOffset => Contents.EndingOffset + (EndsWithQuote? 1 : 0);

        /// <summary>
        /// Length of the token, including any quotes.
        /// </summary>
        public int OuterLength => Contents.Length + (StartsWithQuote ? 1 : 0) + (EndsWithQuote ? 1 : 0);

        /// <summary>
        /// Checks two tokens for equality.
        /// </summary>
        /// <param name="value">A token.</param>
        /// <param name="otherValue">Another token.</param>
        /// <returns>True if the values are equal; false otherwise.</returns>
        public static bool operator ==(Token value, Token otherValue)
        {
            return value.Equals(otherValue);
        }

        /// <summary>
        /// Checks two tokens for inequality.
        /// </summary>
        /// <param name="value">A token.</param>
        /// <param name="otherValue">Another token.</param>
        /// <returns>True if the values are not equal; false otherwise.
        /// </returns>
        public static bool operator !=(Token value, Token otherValue)
        {
            return !value.Equals(otherValue);
        }

        /// <summary>
        /// Checks for equality against another object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object is a substring, and if it's
        /// equal to this one; false otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Token))
            {
                return false;
            }

            return Equals((Token)obj);
        }

        /// <summary>
        /// Generate a hash code for the value.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            // Overflow is fine, just wrap
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + Contents.GetHashCode();
                hash = hash * 23 + StartsWithQuote.GetHashCode();
                hash = hash * 23 + EndsWithQuote.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Converts the object into its own string object.
        /// </summary>
        /// <returns>The token, as a string.</returns>
        public override string ToString() => Contents.ToString();

        /// <summary>
        /// Checks for equality against another token.
        /// </summary>
        /// <param name="other">The other token.</param>
        /// <returns>True if the tokens are equal; false otherwise.
        /// </returns>
        public bool Equals(Token other)
        {
            return (Contents.Equals(other.Contents)) && (StartsWithQuote == other.StartsWithQuote) && (EndsWithQuote == other.EndsWithQuote);
        }
    }
}
