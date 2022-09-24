using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using NClap.Parser;

namespace NClap.Utilities
{
    /// <summary>
    /// Assorted string utilities.
    /// </summary>
    internal static class StringUtilities
    {
        /// <summary>
        /// Append some text to the provided StringBuilder, wrapping text at
        /// the given width, and indenting wrapped text with the given
        /// indentation width.
        /// </summary>
        /// <param name="builder">StringBuilder to append to.</param>
        /// <param name="text">Text to append.</param>
        /// <param name="width">Maximum width of the text, in number of
        /// characters.</param>
        /// <param name="blockIndent">The number of characters to block-indent
        /// all lines. Use 0 to indicate no block indentation should occur.</param>
        public static void AppendWrappedLine(
            this StringBuilder builder,
            string text,
            int width,
            int blockIndent = 0)
        {
            builder.AppendWrapped(text, width, blockIndent);
            builder.AppendLine();
        }

        /// <summary>
        /// Append some text to the provided StringBuilder, wrapping text at
        /// the given width, and indenting wrapped text with the given
        /// indentation width.
        /// </summary>
        /// <param name="builder">StringBuilder to append to.</param>
        /// <param name="text">Text to append.</param>
        /// <param name="width">Maximum width of the text, in number of
        /// characters.</param>
        /// <param name="blockIndent">The number of characters to block-indent
        /// all lines. Use 0 to indicate no block indentation should occur.</param>
        public static void AppendWrapped(
            this StringBuilder builder,
            string text,
            int width,
            int blockIndent = 0)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Append(Wrap(text, width, blockIndent));
        }

        /// <summary>
        /// Wrap the provided text at the given width, indenting it with the
        /// given indentation width.
        /// </summary>
        /// <param name="text">Text to wrap.</param>
        /// <param name="width">Maximum width of the text, in number of
        /// characters.</param>
        /// <param name="blockIndent">The number of characters to block-indent
        /// all lines. Use 0 to indicate no block indentation should occur.</param>
        /// <param name="hangingIndent">The number of characters to hanging-indent
        /// the text; all lines after the first line are affected, and the first
        /// line is left unmodified.  Use 0 to indicate no hanging indentation
        /// should occur.</param>
        /// <returns>The wrapped text.</returns>
        public static ColoredString Wrap(this ColoredString text, int width, int blockIndent = 0, int hangingIndent = 0) =>
            text.Transform(content => Wrap(content, width, blockIndent, hangingIndent));

        /// <summary>
        /// Wrap the provided text at the given width, indenting it with the
        /// given indentation width.
        /// </summary>
        /// <param name="text">Text to wrap.</param>
        /// <param name="width">Maximum width of the text, in number of
        /// characters.</param>
        /// <param name="blockIndent">The number of characters to block-indent
        /// all lines. Use 0 to indicate no block indentation should occur.</param>
        /// <param name="hangingIndent">The number of characters to hanging-indent
        /// the text; all lines after the first line are affected, and the first
        /// line is left unmodified.  Use 0 to indicate no hanging indentation
        /// should occur.</param>
        /// <returns>The wrapped text.</returns>
        public static string Wrap(this string text, int width, int blockIndent = 0, int hangingIndent = 0) =>
            Wrap((IString)(StringWrapper)text, width, blockIndent, hangingIndent).ToString();

        /// <summary>
        /// Wrap the provided text at the given width, indenting it with the
        /// given indentation width.
        /// </summary>
        /// <param name="text">Text to wrap.</param>
        /// <param name="width">Maximum width of the text, in number of
        /// characters.</param>
        /// <param name="blockIndent">The number of characters to block-indent
        /// all lines. Use 0 to indicate no block indentation should occur.</param>
        /// <param name="hangingIndent">The number of characters to hanging-indent
        /// the text; all lines after the first line are affected, and the first
        /// line is left unmodified.  Use 0 to indicate no hanging indentation
        /// should occur.</param>
        /// <returns>The wrapped text.</returns>
        public static IString Wrap(this IString text, int width, int blockIndent = 0, int hangingIndent = 0)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (width < 0 || width <= blockIndent + hangingIndent)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            var builder = text.CreateNewBuilder();

            char[] whiteSpaceChars = { ' ', '\t', '\n' };

            var firstLine = true;

            var preprocessedAndSplitByLine =
                text.Replace("\r", string.Empty)
                    .Split('\n')
                    .Select(line => line.TrimEnd());

            foreach (var line in preprocessedAndSplitByLine)
            {
                // Handle empty lines specially.
                if (line.Length == 0)
                {
                    if (!firstLine)
                    {
                        builder.Append(Environment.NewLine);
                    }

                    // If there's a block indent, insert it.
                    if (blockIndent > 0)
                    {
                        builder.Append(' ', blockIndent);
                    }

                    // If we're not on the first line and there's a hanging indent,
                    // insert it.
                    if (hangingIndent > 0 && !firstLine)
                    {
                        builder.Append(' ', hangingIndent);
                    }

                    firstLine = false;
                    continue;
                }

                // Process this line.
                var index = 0;
                while (index < line.Length)
                {
                    // If we're not on the first line, then make sure to insert the
                    // newline.
                    if (!firstLine)
                    {
                        builder.Append(Environment.NewLine);
                    }

                    // If there's an indent for this line, then insert it.
                    var effectiveIndent = firstLine ? blockIndent : blockIndent + hangingIndent;
                    if (effectiveIndent > 0)
                    {
                        builder.Append(' ', effectiveIndent);
                    }

                    // Figure out how many non-indent characters we'll be able to write.
                    var textWidth = width - effectiveIndent;

                    // Figure out the end of the range we can include.
                    var endIndex = Math.Min(index + textWidth, line.Length);
                    var count = endIndex - index;

                    // Now sort out whether we need to pick a better line break, on a
                    // whitespace (word) boundary. If the next character is a whitespace
                    // character, then we don't need to worry about this. If this is
                    // the last character in the line, then we don't need to worry either.
                    if ((endIndex < line.Length) && !char.IsWhiteSpace(line[endIndex]))
                    {
                        // Find the last whitespace character in the range of text
                        // we're considering adding.  If we found one, then break
                        // there.
                        var lastWhitespaceIndex = line.LastIndexOfAny(whiteSpaceChars, endIndex - 1, count);
                        if (lastWhitespaceIndex > index)
                        {
                            endIndex = lastWhitespaceIndex;
                        }
                    }

                    // Add chars.
                    builder.Append(line.Substring(index, endIndex - index));

                    // Advance the index to match what we just added.
                    index = endIndex;

                    // Don't start a new line with non-newline whitespace.
                    while ((index < line.Length) && char.IsWhiteSpace(line[index]))
                    {
                        ++index;
                    }

                    firstLine = false;
                }
            }

            return builder.Generate();
        }

        /// <summary>
        /// Quotes the provided string if it contains whitespace.
        /// </summary>
        /// <param name="value">String to conditionally quote.</param>
        /// <param name="quoteChar">Quote character to use.</param>
        /// <returns>If the input string contains whitespace, the quoted version
        /// of the string; otherwise, the input string.</returns>
        public static string QuoteIfNeeded(string value, char quoteChar = '\"')
        {
            if (!string.IsNullOrEmpty(value) &&
                (value.IndexOfAny(new[] { ' ', '\t' }) < 0))
            {
                return value;
            }

            return quoteChar + value + quoteChar;
        }

        /// <summary>
        /// Tokenizes the provided input text line, observing quotes.
        /// </summary>
        /// <param name="line">Input line to parse.</param>
        /// <param name="options">Options for tokenizing.</param>
        /// <returns>Enumeration of tokens.</returns>
        public static IEnumerable<Token> Tokenize(string line, TokenizerOptions options)
        {
            //
            // State variables.
            //

            // This should be true if the current token started with a quote
            // character, regardless of whether we're still "inside" the quotes.
            var quoted = false;

            // This should be non-null only if we're in a quoted token and we
            // haven't yet seen the end quote character.  When non-null, its
            // value should be the specific quote character that opened the
            // token.
            char? inQuotes = null;

            // This should be true if an end quote was present in this token.
            // It would be false if "partial input" is allowed and the current
            // token starts with a quote character but has no end quote.
            var endQuotePresent = false;

            // The start index for the current token, or null if we haven't
            // yet seen any part of the next token.
            int? tokenStartIndex = null;

            // The end index for the current token, or null if we haven't seen
            // the end (yet).
            int? tokenEndIndex = null;

            //
            // Main loop.
            //

            // Iterate through each character of the input string, and then once
            // more after having reached the end of string so we can finalize
            // any last token in progress.
            for (var index = 0; index <= line.Length; ++index)
            {
                // If we've reached the end of the input string or a whitespace
                // character, then this may be the end of the token.  Remember,
                // though, that we need to skip past whitespace embedded within
                // a quoted token.
                if ((index == line.Length) || char.IsWhiteSpace(line[index]))
                {
                    var completeToken = false;

                    // If we're in the middle of parsing a token (i.e. we've
                    // either seen the open quotes for a quoted token, or
                    // we've seen at least one non-whitespace character for
                    // all other tokens), and if we're not currently still
                    // inside the quotes of a quoted token, then this must
                    // be the end of the token.
                    if (tokenStartIndex.HasValue && !inQuotes.HasValue)
                    {
                        completeToken = true;
                        endQuotePresent = quoted;
                    }

                    // Otherwise, if we're at the end of the input string,
                    // we're still inside the quotes from the last token,
                    // and we were told by our caller to allow partial input,
                    // then end the token here but make a note that we did
                    // *not* see the end quote for this last token.
                    else if ((index == line.Length) &&
                             inQuotes.HasValue &&
                             options.HasFlag(TokenizerOptions.AllowPartialInput))
                    {
                        Debug.Assert(tokenStartIndex.HasValue);
                        completeToken = true;
                    }

                    // If this is the end of a token, then it's time to yield
                    // it to our caller and reset our internal state for the
                    // next iteration.
                    if (completeToken)
                    {
                        if (!tokenEndIndex.HasValue)
                        {
                            tokenEndIndex = index;
                        }

                        yield return new Token(
                            new Substring(line, tokenStartIndex.Value, tokenEndIndex.Value - tokenStartIndex.Value),
                            quoted,
                            endQuotePresent);

                        tokenStartIndex = null;
                        tokenEndIndex = null;
                        quoted = false;
                        inQuotes = null;
                        endQuotePresent = false;
                    }
                }

                // Otherwise, specially handle quote characters.  We'll need
                // to decide whether the quote character marks the beginning
                // or end of a quoted token, or if it's embedded in the middle
                // of an unquoted token, or if it's errant.
                else if ((line[index] == '\"' && options.HasFlag(TokenizerOptions.HandleDoubleQuoteAsTokenDelimiter)) ||
                         (line[index] == '\'' && options.HasFlag(TokenizerOptions.HandleSingleQuoteAsTokenDelimiter)))
                {
                    // If we're not in the midst of parsing a token, then this
                    // must be the start of a new token.  Update the parse state
                    // appropriately to reflect this.
                    if (!tokenStartIndex.HasValue)
                    {
                        Debug.Assert(!inQuotes.HasValue);

                        inQuotes = line[index];
                        quoted = true;
                        tokenStartIndex = index + 1;
                    }

                    // Otherwise, we must be in the midst of parsing a token.
                    // If we're still inside the quotes for the token, then
                    // this may be the terminating quotes.  Otherwise, we'll
                    // fall through and just consider the quote character
                    // a normal character embedded within the current token.
                    else if (quoted)
                    {
                        Debug.Assert(inQuotes.HasValue);

                        // If this quote character is different from the one
                        // that opened this token, then we consider it a normal
                        // character.
                        if (inQuotes.Value != line[index])
                        {
                            // Nothing to do here.
                        }

                        // If this quote character isn't the last in the
                        // input string, and if the character following this
                        // one is *not* a whitespace character, then we've
                        // encountered something wrong.  Unless we were told
                        // to allow "partial input" (i.e. ignore errors like
                        // these), we'll throw an exception.
                        else if ((index + 1 != line.Length) &&
                                 !char.IsWhiteSpace(line[index + 1]))
                        {
                            if (!options.HasFlag(TokenizerOptions.AllowPartialInput))
                            {
                                throw new ArgumentOutOfRangeException(nameof(line), Strings.TerminatingQuotesNotEndOfToken);
                            }
                        }
                        else
                        {
                            // Okay, this was the end quote for the token.
                            // Mark it as such.
                            inQuotes = null;
                            endQuotePresent = true;
                            tokenEndIndex = index;
                        }
                    }
                }

                // Otherwise, it's a normal character.  It will end up in the
                // current token.  If we're not in the midst of a token, then
                // it's time to start a new one here.
                else if (!tokenStartIndex.HasValue)
                {
                    tokenStartIndex = index;
                }
            }

            // Now that we've gone past the end of the input string, check to
            // make sure we're not still inside quotes.  If we are, and if we're
            // not allowing partial input, then we throw an exception.  It's
            // bogus.
            if (tokenStartIndex.HasValue)
            {
                Debug.Assert(inQuotes.HasValue);
                Debug.Assert(!options.HasFlag(TokenizerOptions.AllowPartialInput));

                throw new ArgumentOutOfRangeException(nameof(line), Strings.UnterminatedQuotes);
            }
        }

        /// <summary>
        /// Formats the provided strings into a single, columnized string.
        /// </summary>
        /// <param name="values">The values to format.</param>
        /// <param name="screenBufferWidth">The width of the intended output
        /// screen buffer, in characters.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatInColumns(IReadOnlyList<string> values, int screenBufferWidth)
        {
            const int spaceBetweenCols = 2;

            if (values.Count == 0) return string.Empty;

            var builder = new StringBuilder();

            var maxLength = values.Max(v => v.Length);

            var colWidth = Math.Min(maxLength + spaceBetweenCols, screenBufferWidth);

            var cols = Math.Max(1, screenBufferWidth / colWidth);

            var lastColWidth = screenBufferWidth - (colWidth * (cols - 1));

            var rows = values.Count / cols;
            if (values.Count % cols != 0)
            {
                ++rows;
            }

            for (var rowIndex = 0; rowIndex < rows; ++rowIndex)
            {
                var charsInCol = 0;
                for (var colIndex = 0; colIndex < cols; ++colIndex)
                {
                    var index = (colIndex * rows) + rowIndex;
                    if (index >= values.Count)
                    {
                        builder.AppendLine();
                        break;
                    }

                    var thisColWidth = (colIndex == cols - 1) ? lastColWidth : colWidth;

                    var format = "{0,-" + thisColWidth + "}";
                    builder.AppendFormat(CultureInfo.CurrentCulture, format, values[index]);

                    charsInCol += values[index].Length;
                }

                if (charsInCol > screenBufferWidth)
                {
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Make a best effort to convert a string to being a hyphenated,
        /// lower-case string.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <returns>The converted string.</returns>
        public static string ToHyphenatedLowerCase(this string s) => ToLowerCaseWithSeparator(s, '-');

        /// <summary>
        /// Make a best effort to convert a string to being a snake-cased
        /// string.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <returns>The converted string.</returns>
        public static string ToSnakeCase(this string s) => ToLowerCaseWithSeparator(s, '_');

        /// <summary>
        /// Computes the Damerau-Levenshtein edit distance between two strings.
        ///
        /// Source: https://gist.github.com/wickedshimmy/449595.
        ///
        /// Copyright (c) 2010, 2012 Matt Enright
        /// Permission is hereby granted, free of charge, to any person obtaining
        /// a copy of this software and associated documentation files (the
        /// "Software"), to deal in the Software without restriction, including
        /// without limitation the rights to use, copy, modify, merge, publish,
        /// distribute, sublicense, and/or sell copies of the Software, and to
        /// permit persons to whom the Software is furnished to do so, subject to
        /// the following conditions:
        ///
        /// The above copyright notice and this permission notice shall be
        /// included in all copies or substantial portions of the Software.
        ///
        /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
        /// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
        /// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
        /// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
        /// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
        /// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
        /// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
        /// </summary>
        /// <param name="original">Original string.</param>
        /// <param name="modified">Modified string.</param>
        /// <returns>Edit instance.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", Justification = "[Legacy]")]
        public static int GetDamerauLevenshteinEditDistance(string original, string modified)
        {
            var lenOrig = original.Length;
            var lenDiff = modified.Length;

            var matrix = new int[lenOrig + 1, lenDiff + 1];
            for (int i = 0; i <= lenOrig; i++)
            {
                matrix[i, 0] = i;
            }

            for (int j = 0; j <= lenDiff; j++)
            {
                matrix[0, j] = j;
            }

            for (int i = 1; i <= lenOrig; i++)
            {
                for (int j = 1; j <= lenDiff; j++)
                {
                    var cost = modified[j - 1] == original[i - 1] ? 0 : 1;
                    var vals = new int[]
                    {
                        matrix[i - 1, j] + 1,
                        matrix[i, j - 1] + 1,
                        matrix[i - 1, j - 1] + cost
                    };

                    matrix[i, j] = vals.Min();
                    if (i > 1 && j > 1 && original[i - 1] == modified[j - 2] && original[i - 2] == modified[j - 1])
                        matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + cost);
                }
            }

            return matrix[lenOrig, lenDiff];
        }

        private static string ToLowerCaseWithSeparator(string s, char separator)
        {
            var result = new StringBuilder();

            var lastCharWasLowerCase = false;
            for (int i = 0; i < s.Length; ++i)
            {
                var c = s[i];

                if (i == 0)
                {
                    c = char.ToLower(c);
                }
                else if (c == '_' || c == '-')
                {
                    c = separator;
                    lastCharWasLowerCase = false;
                }
                else if (char.IsUpper(c))
                {
                    if (lastCharWasLowerCase)
                    {
                        result.Append(separator);
                    }

                    c = char.ToLower(c);
                    lastCharWasLowerCase = false;
                }
                else
                {
                    lastCharWasLowerCase = true;
                }

                result.Append(c);
            }

            return result.ToString();
        }
    }
}
