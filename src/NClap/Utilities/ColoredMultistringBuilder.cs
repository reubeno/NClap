using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NClap.Utilities
{
    /// <summary>
    /// Simplified, colored string version of StringBuilder.
    /// </summary>
    public class ColoredMultistringBuilder : IStringBuilder
    {
        private readonly List<ColoredString> _pieces = new List<ColoredString>();
        private int _totalLength;

        /// <summary>
        /// Retrieves the current length of the builder's contents.
        /// </summary>
        public int Length => _totalLength;

        /// <summary>
        /// Accesses the character at the specified index in the builder.
        /// </summary>
        /// <param name="index">0-based index into the builder.</param>
        /// <returns>The character at the specified index.</returns>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "[Legacy]")]
        public char this[int index]
        {
            get
            {
                var s = GetContainingPiece(index, out int offset);
                return s.Content[offset];
            }

            set
            {
                var s = GetContainingPiece(index, out int offset);

                Remove(index, 1);
                Insert(index, s.Transform(_ => new string(value, 1)));
            }
        }

        /// <summary>
        /// Append a colored string.
        /// </summary>
        /// <param name="value">The colored string to append.</param>
        public void Append(ColoredString value) => Insert(Length, value);

        /// <summary>
        /// Append the provided colored strings.
        /// </summary>
        /// <param name="values">The colored strings to append.</param>
        public void Append(IEnumerable<ColoredString> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            foreach (var value in values)
            {
                Append(value);
            }
        }

        /// <summary>
        /// Append the contents of the provided multistring.
        /// </summary>
        /// <param name="value">The multistring to append.</param>
        public void Append(ColoredMultistring value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            Append(value.Content);
        }

        /// <summary>
        /// Append the contents of the provided multistrings.
        /// </summary>
        /// <param name="values">The multistrings to append.</param>
        public void Append(IEnumerable<ColoredMultistring> values) => Append(values.SelectMany(v => v.Content));

        /// <summary>
        /// Append a newline.
        /// </summary>
        public void AppendLine() => Append(Environment.NewLine);

        /// <summary>
        /// Append a colored string followed by a newline.
        /// </summary>
        /// <param name="value">The colored string to append.</param>
        public void AppendLine(ColoredString value)
        {
            Append(value);
            Append(value.Transform(content => Environment.NewLine));
        }

        /// <summary>
        /// Append the provided colored strings followed by a newline.
        /// </summary>
        /// <param name="values">The colored multistrings to append.</param>
        public void AppendLine(IEnumerable<ColoredString> values)
        {
            Append(values);
            AppendLine();
        }

        /// <summary>
        /// Append the contents of the provided multistring followed by a
        /// newline.
        /// </summary>
        /// <param name="value">The multistring to append.</param>
        public void AppendLine(ColoredMultistring value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            AppendLine(value.Content);
        }

        /// <summary>
        /// Append the contents of the provided multistrings followed by a
        /// newline.
        /// </summary>
        /// <param name="values">The multistrings to append.</param>
        public void AppendLine(IEnumerable<ColoredMultistring> values)
        {
            Append(values);
            AppendLine();
        }

        /// <summary>
        /// Converts the current contents of the builder to a bare string.
        /// </summary>
        /// <returns>The bare string.</returns>
        public override string ToString() => string.Concat(_pieces);

        /// <summary>
        /// Converts the current contents of the builder to a colored
        /// multistring.
        /// </summary>
        /// <returns>The multistring.</returns>
        public ColoredMultistring ToMultistring() => new ColoredMultistring(_pieces);

        /// <summary>
        /// Appends a new string to the end of this builder's current content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        public void Append(IString s) => Append((ColoredMultistring)s);

        /// <summary>
        /// Appends a new string to the end of this builder's current content.
        /// </summary>
        /// <param name="s">The string to append.</param>
        public void Append(string s) => Append((ColoredString)s);

        /// <summary>
        /// Appends the specified character to the end of this builder's current
        /// content, repeated the indicated number of times.
        /// </summary>
        /// <param name="c">The char to append.</param>
        /// <param name="count">The number of times to append it.</param>
        public void Append(char c, int count) => Append(new string(c, count));

        /// <summary>
        /// Generates a composed string from the contents of this builder.
        /// </summary>
        /// <returns>The composed string.</returns>
        public IString Generate() => ToMultistring();

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
        public void CopyTo(int startingIndex, char[] buffer, int outputOffset, int count)
        {
            var inputCursor = 0;
            var charsLeftToCopy = count;
            var outputCursor = outputOffset;

            var copying = false;
            foreach (var piece in _pieces)
            {
                if (charsLeftToCopy == 0)
                {
                    break;
                }

                var offsetIntoPiece = 0;
                if (startingIndex >= inputCursor &&
                    startingIndex < inputCursor + piece.Length)
                {
                    copying = true;
                    offsetIntoPiece = startingIndex - inputCursor;
                }

                if (copying)
                {
                    var copyingThisTime = Math.Min(charsLeftToCopy, piece.Length - offsetIntoPiece);
                    piece.Content.CopyTo(offsetIntoPiece, buffer, outputCursor, copyingThisTime);

                    charsLeftToCopy -= copyingThisTime;
                    outputCursor += copyingThisTime;
                }

                inputCursor += piece.Length;
            }

            if (charsLeftToCopy > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        /// <summary>
        /// Inserts the given character at the specified index.
        /// </summary>
        /// <param name="index">0-based index.</param>
        /// <param name="c">The character to insert.</param>
        public void Insert(int index, char c) => Insert(index, new string(c, 1));

        /// <summary>
        /// Inserts the given string at the specified index.
        /// </summary>
        /// <param name="index">0-based index.</param>
        /// <param name="s">The string to insert.</param>
        public void Insert(int index, string s) => Insert(index, new ColoredString(s));

        /// <summary>
        /// Inserts the given string at the specified index.
        /// </summary>
        /// <param name="index">0-based index.</param>
        /// <param name="s">The string to insert.</param>
        public void Insert(int index, ColoredString s)
        {
            if (index < 0 || index > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            // Optimization: don't bother if string is empty.
            if (s.IsEmpty())
            {
                return;
            }

            //
            // At this point, we're guaranteed it's either before or in
            // the middle of the existing contents.
            //

            var pieceIndex = 0;
            var offset = 0;
            ColoredString? lastPiece = null;
            while (pieceIndex < _pieces.Count)
            {
                var piece = _pieces[pieceIndex];
                Debug.Assert(!piece.IsEmpty());

                // Case 1: insertion point is just before this piece.
                if (index == offset)
                {
                    if (s.IsSameColorAs(piece))
                    {
                        _pieces.RemoveAt(pieceIndex);
                        _pieces.Insert(pieceIndex,
                            piece.Transform(content => s.Content + content));
                    }
                    else
                    {
                        _pieces.Insert(pieceIndex, s);
                    }

                    _totalLength += s.Length;
                    return;
                }

                // Case 2: insertion point is in middle of this piece.
                else if (index < offset + piece.Length)
                {
                    _pieces.RemoveAt(pieceIndex);

                    if (s.IsSameColorAs(piece))
                    {
                        _pieces.Insert(pieceIndex, piece.Transform(content =>
                            content.Substring(0, index - offset) +
                            s.Content +
                            content.Substring(index - offset)));
                    }
                    else
                    {
                        _pieces.Insert(pieceIndex, piece.Substring(0, index - offset));
                        _pieces.Insert(pieceIndex + 1, s);
                        _pieces.Insert(pieceIndex + 2, piece.Substring(index - offset));
                    }

                    _totalLength += s.Length;
                    return;
                }

                // Case 3: insertion point is just after this piece.
                // Only insert during this loop iteration if new piece
                // can be merged with this one.  We'll otherwise get it
                // the next time around.
                else if (index == offset + piece.Length)
                {
                    if (s.IsSameColorAs(piece))
                    {
                        _pieces.RemoveAt(pieceIndex);
                        _pieces.Insert(pieceIndex,
                            piece.Transform(content => content + s.Content));

                        _totalLength += s.Length;
                        return;
                    }
                }

                offset += piece.Length;
                lastPiece = piece;
                ++pieceIndex;
            }

            // If we're still here, then it goes at the end.
            Debug.Assert(index == Length);

            // Append.
            _pieces.Add(s);
            _totalLength += s.Length;
        }

        /// <summary>
        /// Removes the specified number of characters from the given index into
        /// the builder's contents.
        /// </summary>
        /// <param name="index">0-based index.</param>
        /// <param name="count">The number of characters to remove.</param>
        public void Remove(int index, int count)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var pieceIndex = 0;
            var offset = 0;
            var nextIndexToRemove = index;
            var charsLeftToRemove = count;
            while (charsLeftToRemove > 0 && pieceIndex < _pieces.Count)
            {
                var piece = _pieces[pieceIndex];
                Debug.Assert(!piece.IsEmpty());

                if (nextIndexToRemove >= offset &&
                    nextIndexToRemove < offset + piece.Length)
                {
                    var offsetIntoPiece = nextIndexToRemove - offset;
                    var charsToRemoveThisTime = Math.Min(charsLeftToRemove, piece.Length - offsetIntoPiece);

                    // Case 1: strict prefix to remove.
                    if (offsetIntoPiece == 0 && charsToRemoveThisTime < piece.Length)
                    {
                        _pieces.RemoveAt(pieceIndex);
                        _pieces.Insert(pieceIndex, piece.Substring(charsToRemoveThisTime));

                        pieceIndex += 1;
                    }

                    // Case 2: strict suffix to remove.
                    else if (offsetIntoPiece > 0 && offsetIntoPiece + charsToRemoveThisTime == piece.Length)
                    {
                        _pieces.RemoveAt(pieceIndex);
                        _pieces.Insert(pieceIndex, piece.Substring(0, offsetIntoPiece));

                        pieceIndex += 1;
                    }

                    // Case 3: remove whole piece.
                    else if (offsetIntoPiece == 0 && charsToRemoveThisTime == piece.Length)
                    {
                        _pieces.RemoveAt(pieceIndex);

                        // TODO: check for coalesce opportunity.
                    }

                    // Case 4: remove middle of piece.
                    else
                    {
                        _pieces.RemoveAt(pieceIndex);

                        _pieces.Insert(
                            pieceIndex,
                            piece.Transform(pieceContent =>
                                pieceContent.Substring(0, offsetIntoPiece) +
                                  pieceContent.Substring(offsetIntoPiece + charsToRemoveThisTime)));

                        pieceIndex += 1;
                    }

                    charsLeftToRemove -= charsToRemoveThisTime;
                    _totalLength -= charsToRemoveThisTime;
                    offset += piece.Length - charsToRemoveThisTime;
                }
                else
                {
                    offset += piece.Length;
                    ++pieceIndex;
                }
            }

            if (charsLeftToRemove > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        /// <summary>
        /// Clears the current contents of the builder.
        /// </summary>
        public void Clear()
        {
            _pieces.Clear();
            _totalLength = 0;
        }

        /// <summary>
        /// Truncates the contents of the builder to the specified length.
        /// </summary>
        /// <param name="newLength">New length, expressed as a character
        /// count.</param>
        public void Truncate(int newLength)
        {
            if (newLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newLength));
            }

            var charsToRemove = Length - newLength;

            if (charsToRemove < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newLength));
            }
            else if (charsToRemove > 0)
            {
                Remove(newLength, charsToRemove);
            }
        }

        private ColoredString GetContainingPiece(int index, out int offsetIntoPiece)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var currentIndex = index;
            foreach (var piece in _pieces)
            {
                Debug.Assert(currentIndex >= 0);

                if (currentIndex < piece.Length)
                {
                    offsetIntoPiece = currentIndex;
                    return piece;
                }

                currentIndex -= piece.Length;
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
