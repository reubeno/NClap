using System;
using System.Diagnostics.CodeAnalysis;

namespace NClap.Utilities
{
    /// <summary>
    /// Represents a subsection of a string object.
    /// </summary>
    public struct Substring : IEquatable<Substring>
    {
        /// <summary>
        /// Constructs a substring object from a string and starting offset.
        /// </summary>
        /// <param name="value">The base string object.</param>
        /// <param name="startingOffset">The starting offset at which the
        /// substring starts in the provided string.</param>
        public Substring(string value, int startingOffset) :
            this(value, startingOffset, value?.Length - startingOffset ?? 0)
        {
        }

        /// <summary>
        /// Constructs a substring object from a string, starting offset, and
        /// length.
        /// </summary>
        /// <param name="value">The base string object.</param>
        /// <param name="startingOffset">The starting offset at which the
        /// substring starts in the provided string.</param>
        /// <param name="length">The number of characters in the substring.
        /// </param>
        public Substring(string value, int startingOffset, int length)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (startingOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startingOffset));
            }

            if (startingOffset + length > value.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startingOffset));
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Base = value;
            StartingOffset = startingOffset;
            Length = length;
        }

        /// <summary>
        /// Creates a substring from a substring.
        /// </summary>
        /// <param name="substring">The substring to create a substring from.
        /// </param>
        /// <param name="startingOffset">The starting offset, relative to the
        /// containing substring.</param>
        public Substring(Substring substring, int startingOffset)
            : this(substring, startingOffset, substring.Length - startingOffset)
        {
        }

        /// <summary>
        /// Creates a substring from a substring.
        /// </summary>
        /// <param name="substring">The substring to create a substring from.
        /// </param>
        /// <param name="startingOffset">The starting offset, relative to the
        /// containing substring.</param>
        /// <param name="length">The length of the new substring.</param>
        public Substring(Substring substring, int startingOffset, int length) : this(substring.Base, substring.StartingOffset + startingOffset, length)
        {
            if (startingOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startingOffset));
            }
        }

        /// <summary>
        /// The full string.
        /// </summary>
        public string Base { get; }

        /// <summary>
        /// The offset in the containing string at which this substring starts.
        /// </summary>
        public int StartingOffset { get; }

        /// <summary>
        /// The offset in the containing string at which the substring ends.
        /// </summary>
        public int EndingOffset => StartingOffset + Length;

        /// <summary>
        /// The length of the substring, in characters.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Checks two substrings for equality.
        /// </summary>
        /// <param name="value">A substring.</param>
        /// <param name="otherValue">Another substring.</param>
        /// <returns>True if the values are equal; false otherwise.</returns>
        public static bool operator ==(Substring value, Substring otherValue)
        {
            return value.Equals(otherValue);
        }

        /// <summary>
        /// Checks two substrings for inequality.
        /// </summary>
        /// <param name="value">A substring.</param>
        /// <param name="otherValue">Another substring.</param>
        /// <returns>True if the values are not equal; false otherwise.
        /// </returns>
        public static bool operator !=(Substring value, Substring otherValue)
        {
            return !value.Equals(otherValue);
        }

        /// <summary>
        /// Converts the object into its own string object.
        /// </summary>
        /// <returns>The substring, as a string.</returns>
        public override string ToString() =>
            Base.Substring(StartingOffset, Length);

        /// <summary>
        /// Checks for equality against another object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object is a substring, and if it's
        /// equal to this one; false otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Substring))
            {
                return false;
            }

            return Equals((Substring)obj);
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
                hash = hash * 23 + Base.GetHashCode();
                hash = hash * 23 + StartingOffset.GetHashCode();
                hash = hash * 23 + Length.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Checks for equality against another substring.
        /// </summary>
        /// <param name="other">The other substring.</param>
        /// <returns>True if the substrings are equal; false otherwise.
        /// </returns>
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison")]
        public bool Equals(Substring other) =>
            Base.Equals(other.Base) &&
            StartingOffset == other.StartingOffset &&
            Length == other.Length;

        /// <summary>
        /// Looks for the first occurrence of the specified character in the
        /// substring.
        /// </summary>
        /// <param name="value">The character to look for.</param>
        /// <returns>On success, the 0-based index of the first occurrence of
        /// the character, expressed as an offset into the outer containing
        /// string (not the substring's start); otherwise, when the character
        /// does not exist in the substring, a negative integer is returned.
        /// </returns>
        public int IndexOf(char value) => Base.IndexOf(value, StartingOffset, Length);

        /// <summary>
        /// Checks if the substring contains the specified character.
        /// </summary>
        /// <param name="value">The character to look for.</param>
        /// <returns>True if the character was found, false otherwise.</returns>
        public bool Contains(char value) => IndexOf(value) >= 0;

        /// <summary>
        /// Checks if the substring contains a character that passes the
        /// provided filter.
        /// </summary>
        /// <param name="func">The function to invoke on each character in the
        /// substring.</param>
        /// <returns>True if the character was found, false otherwise.</returns>
        public bool Contains(Func<char, bool> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            for (var index = StartingOffset; index < EndingOffset; ++index)
            {
                if (func(Base[index]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
