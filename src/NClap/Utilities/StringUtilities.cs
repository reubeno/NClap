using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

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
        /// <param name="indent">The number of characters to indent
        /// lines after the first one; 0 to indicate no indentation
        /// should occur.</param>
        public static void AppendWrappedLine(
            this StringBuilder builder,
            string text,
            int width,
            int indent = 0)
        {
            builder.AppendWrapped(text, width, indent);
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
        /// <param name="indent">The number of characters to indent
        /// lines after the first one; 0 to indicate no indentation
        /// should occur.</param>
        public static void AppendWrapped(
            this StringBuilder builder,
            string text,
            int width,
            int indent = 0)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Append(Wrap(text, width, indent));
        }


        /// <summary>
        /// Wrap the provided text at the given width, indenting it with the
        /// given indentation width.
        /// </summary>
        /// <param name="text">Text to wrap.</param>
        /// <param name="width">Maximum width of the text, in number of
        /// characters.</param>
        /// <param name="indent">The number of characters to indent
        /// lines; 0 to indicate no indentation should occur.</param>
        /// <param name="hangingIndent">The number of characters to
        /// unindent the first line.</param>
        /// <returns>The wrapped text.</returns>
        public static ColoredString Wrap(this ColoredString text, int width, int indent = 0, int hangingIndent = 0) =>
            new ColoredString(Wrap(text.Content, width, indent, hangingIndent), text.ForegroundColor, text.BackgroundColor);

        /// <summary>
        /// Wrap the provided text at the given width, indenting it with the
        /// given indentation width.
        /// </summary>
        /// <param name="text">Text to wrap.</param>
        /// <param name="width">Maximum width of the text, in number of
        /// characters.</param>
        /// <param name="indent">The number of characters to indent
        /// lines; 0 to indicate no indentation should occur.</param>
        /// <param name="hangingIndent">The number of characters to
        /// unindent the first line.</param>
        /// <returns>The wrapped text.</returns>
        public static IString Wrap(this IString text, int width, int indent = 0, int hangingIndent = 0)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if ((width < 0) || (width <= indent) || (hangingIndent > indent))
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            var builder = text.CreateBuilder();

            char[] whiteSpaceChars = { ' ', '\t', '\n' };

            var firstLine = true;
            foreach (var line in text.Replace("\r", string.Empty)
                                     .Split('\n')
                                     .Select(line => line.TrimEnd()))
            {
                // Handle empty lines specially.
                if (line.Length == 0)
                {
                    if (!firstLine)
                    {
                        builder.Append(Environment.NewLine);
                    }

                    // If there's an indent, insert it.
                    if (indent > 0)
                    {
                        builder.Append(' ', indent);
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
                    var effectiveIndent = firstLine ? indent - hangingIndent : indent;
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
        /// <param name="value"></param>
        /// <returns>If the input string contains whitespace, the quoted version
        /// of the string; otherwise, the input string.</returns>
        public static string QuoteIfNeeded(string value)
        {
            if (!string.IsNullOrEmpty(value) &&
                (value.IndexOfAny(new[] { ' ', '\t' }) < 0))
            {
                return value;
            }

            return string.Concat("\"", value, "\"");
        }
        
        /// <summary>
        /// Tokenizes the provided input text line, observing quotes.
        /// </summary>
        /// <param name="line">Input line to parse.</param>
        /// <returns>Enumeration of tokens.</returns>
        public static IEnumerable<Token> Tokenize(string line) =>
            Tokenize(line, false /* allow partial input? */);

        /// <summary>
        /// Tokenizes the provided input text line, observing quotes.
        /// </summary>
        /// <param name="line">Input line to parse.</param>
        /// <param name="allowPartialInput">Allow the input line to be partial
        /// (i.e. incomplete).</param>
        /// <returns>Enumeration of tokens.</returns>
        public static IEnumerable<Token> Tokenize(string line, bool allowPartialInput)
        {
            //
            // State variables.
            //

            // This should be true if the current token started with a quote
            // character, regardless of whether we're still "inside" the quotes.
            var quoted = false;

            // This should be true only if we're in a quoted token and we
            // haven't yet seen the end quote character.
            var inQuotes = false;

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
                    if (tokenStartIndex.HasValue && !inQuotes)
                    {
                        completeToken = true;
                        endQuotePresent = quoted;
                    }

                    // Otherwise, if we're at the end of the input string,
                    // we're still inside the quotes from the last token,
                    // and we were told by our caller to allow partial input,
                    // then end the token here but make a note that we did
                    // *not* see the end quote for this last token.
                    else if ((index == line.Length) && inQuotes && allowPartialInput)
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
                        inQuotes = false;
                        endQuotePresent = false;
                    }
                }

                // Otherwise, specially handle quote characters.  We'll need
                // to decide whether the quote character marks the beginning
                // or end of a quoted token, or if it's embedded in the middle
                // of an unquoted token, or if it's errant.
                else if (line[index] == '\"')
                {
                    // If we're not in the midst of parsing a token, then this
                    // must be the start of a new token.  Update the parse state
                    // appropriately to reflect this.
                    if (!tokenStartIndex.HasValue)
                    {
                        Debug.Assert(!inQuotes);

                        inQuotes = true;
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
                        // If this quote character isn't the last in the
                        // input string, and if the character following this
                        // one is *not* a whitespace character, then we've
                        // encountered something wrong.  Unless we were told
                        // to allow "partial input" (i.e. ignore errors like
                        // these), we'll throw an exception.
                        if ((index + 1 != line.Length) &&
                            !char.IsWhiteSpace(line[index + 1]))
                        {
                            if (!allowPartialInput)
                            {
                                throw new ArgumentOutOfRangeException(nameof(line), Strings.TerminatingQuotesNotEndOfToken);
                            }
                        }
                        else
                        {
                            // Okay, this was the end quote for the token.
                            // Mark it as such.
                            inQuotes = false;
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
                Debug.Assert(inQuotes);
                Debug.Assert(!allowPartialInput);

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

            var lastColWidth = screenBufferWidth - colWidth * (cols - 1);

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
                    var index = colIndex * rows + rowIndex;
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

        private static string ToLowerCaseWithSeparator(string s, char separator)
        {
            var result = new StringBuilder();

            bool lastCharWasLowerCase = false;
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
                        result.Append(separator.ToString());
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
